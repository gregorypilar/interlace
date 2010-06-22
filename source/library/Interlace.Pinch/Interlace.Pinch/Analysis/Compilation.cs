#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;

using Interlace.Collections;
using Interlace.Pinch.Dom;

#endregion

namespace Interlace.Pinch.Analysis
{
    public class Compilation
    {
        Dictionary<NamespaceName, Protocol> _namespaces;
        Dictionary<NamespaceName, Declaration> _declarations;

        List<Document> _documents;

        Dictionary<string, IntrinsicTypeReference> _intrinsics;
        Dictionary<string, ContainerType> _containers;

        public Compilation()
        {
            _namespaces = new Dictionary<NamespaceName, Protocol>();
            _declarations = new Dictionary<NamespaceName, Declaration>();
            _documents = new List<Document>();

            _intrinsics = new Dictionary<string, IntrinsicTypeReference>();
            _intrinsics["float32"] = new IntrinsicTypeReference(IntrinsicType.Float32);
            _intrinsics["float64"] = new IntrinsicTypeReference(IntrinsicType.Float64);
            _intrinsics["int8"] = new IntrinsicTypeReference(IntrinsicType.Int8);
            _intrinsics["int16"] = new IntrinsicTypeReference(IntrinsicType.Int16);
            _intrinsics["int32"] = new IntrinsicTypeReference(IntrinsicType.Int32);
            _intrinsics["int64"] = new IntrinsicTypeReference(IntrinsicType.Int64);
            _intrinsics["decimal"] = new IntrinsicTypeReference(IntrinsicType.Decimal);
            _intrinsics["bool"] = new IntrinsicTypeReference(IntrinsicType.Bool);
            _intrinsics["string"] = new IntrinsicTypeReference(IntrinsicType.String);
            _intrinsics["bytes"] = new IntrinsicTypeReference(IntrinsicType.Bytes);

            _containers = new Dictionary<string, ContainerType>();
            _containers["list"] = ContainerType.List;
            _containers["map"] = ContainerType.Map;
            _containers["set"] = ContainerType.Set;
        }

        public void AddDocument(Document document)
        {
            // Check each of the protocols:
            foreach (Protocol protocol in document.Protocols)
            {
                if (_namespaces.ContainsKey(protocol.Name))
                {
                    throw new SemanticException("The protocol name \"{0}\" has been used more than once.");
                }

                CheckProtocolVersions(protocol);
                CheckNames(protocol);
                CheckStructures(protocol);
            }

            // Add each protocol and declaration to the symbol tables:
            foreach (Protocol protocol in document.Protocols)
            {
                _namespaces.Add(protocol.Name, protocol);

                foreach (Declaration declaration in protocol.Declarations)
                {
                    _declarations.Add(protocol.GetFullNameOfDeclaration(declaration), declaration);
                }
            }

            _documents.Add(document);
        }

        public void Resolve()
        {
            foreach (Document document in _documents)
            {
                foreach (Protocol protocol in document.Protocols)
                {
                    foreach (Declaration declaration in protocol.Declarations)
                    {
                        Structure structure = declaration as Structure;

                        if (structure == null) continue;

                        ResolveMembers(protocol, structure);
                    }
                }
            }
        }

        public void Number()
        {
            foreach (Document document in _documents)
            {
                foreach (Protocol protocol in document.Protocols)
                {
                    VersionableUtilities.NumberVersionables(protocol.Declarations);

                    // Loop through the declarations,
                    foreach (Declaration declaration in protocol.Declarations)
                    {
                        declaration.SortAndNumberVersionables();
                    }
                }
            }
        }

        void ResolveMembers(Protocol protocol, Structure structure)
        {
            Set<NamespaceName> references = new Set<NamespaceName>();

            foreach (StructureMember member in structure.Members)
            {
                NamespaceName name = member.FieldType;
                NamespaceName qualifiedName;

                if (name.IsQualified)
                {
                    qualifiedName = name;
                }
                else
                {
                    qualifiedName = new NamespaceName(protocol.Name, name.UnqualifiedName);
                }

                if (references.Contains(qualifiedName))
                {
                    throw new SemanticException(string.Format(
                        "The member \"{0}\" in the choice \"{1}\" in the protocol \"{2}\"; " +
                        "choices can only contain structures and messages.",
                        member.Identifier, structure.Identifier, protocol.Name));
                }

                if (_declarations.ContainsKey(qualifiedName))
                {
                    member.FieldTypeReference = new DeclarationTypeReference(_declarations[qualifiedName]);
                }
                else if (!name.IsQualified && _intrinsics.ContainsKey(name.UnqualifiedName))
                {
                    member.FieldTypeReference = _intrinsics[name.UnqualifiedName];

                    if (structure.StructureKind == StructureKind.Choice)
                    {
                        throw new SemanticException(string.Format(
                            "The member \"{0}\" in the choice \"{1}\" in the protocol \"{2}\"; " +
                            "choices can only contain structures and messages.",
                            member.Identifier, structure.Identifier, protocol.Name));

                    }
                }
                else
                {
                    throw new SemanticException(string.Format(
                        "The type \"{0}\" in \"{1}\" in the protocol \"{2}\" could not be resolved.",
                        name, member.Identifier, protocol.Name));
                }

                if (member.FieldContainer != null)
                {
                    if (!member.FieldContainer.IsQualified && _containers.ContainsKey(member.FieldContainer.UnqualifiedName))
                    {
                        member.FieldContainerReference = _containers[member.FieldContainer.UnqualifiedName];
                    }
                    else
                    {
                        throw new SemanticException(string.Format(
                            "The container \"{0}\" in \"{1}\" in the protocol \"{2}\" is not a valid container type.",
                            member.FieldContainer, member.Identifier, protocol.Name));
                    }
                }
            }
        }

        void CheckProtocolVersions(Protocol protocol)
        {
            // Check that the protocol version makes sense:
            int latestReferencedVersion = 0;

            foreach (Declaration declaration in protocol.Declarations)
            {
                if (declaration.Versioning.HasZeroVersionNumber)
                {
                    throw new InvalidOperationException(string.Format(
                        "The declaration for \"{0}\" has a zero version number, which is an invalid value.",
                        declaration.Identifier));
                }

                if (declaration.Versioning.WasRemovedBeforeAdded)
                {
                    throw new InvalidOperationException(string.Format(
                        "The declaration for \"{0}\" has a removal version number that is equal to or earlier than the " +
                        "initial add version number.", declaration.Identifier));
                }

                if (declaration.Versioning.LatestReferencedVersion > protocol.Version)
                {
                    throw new SemanticException(string.Format("The declaration for \"{0}\" references " +
                        "version {1}, but the protocol version is only version {2}.",
                        declaration.Identifier, declaration.Versioning.LatestReferencedVersion, protocol.Version));
                }

                latestReferencedVersion = Math.Max(latestReferencedVersion, declaration.Versioning.LatestReferencedVersion);

                foreach (DeclarationMember member in declaration.MemberBases)
                {
                    if (declaration.Versioning.HasZeroVersionNumber)
                    {
                        throw new InvalidOperationException(string.Format(
                            "The member \"{0}\" in declaration \"{1}\" has a zero version number, which is an invalid value.",
                            member.Identifier, declaration.Identifier));
                    }

                    if (declaration.Versioning.WasRemovedBeforeAdded)
                    {
                        throw new InvalidOperationException(string.Format(
                            "The member \"{0}\" in declaration \"{1}\" has a removal version number that is equal to or earlier than the " +
                            "initial add version number.", 
                            member.Identifier, declaration.Identifier));
                    }

                    if (member.Versioning.LatestReferencedVersion > protocol.Version)
                    {
                        throw new SemanticException(string.Format("The member \"{0}\" within \"{1}\" references " +
                            "version {2}, but the protocol version is only version {3}.", member.Identifier,
                            declaration.Identifier, member.Versioning.LatestReferencedVersion, protocol.Version));
                    }

                    latestReferencedVersion = Math.Max(latestReferencedVersion, member.Versioning.LatestReferencedVersion);
                }
            }

            if (latestReferencedVersion < protocol.Version)
            {
                throw new SemanticException(string.Format(
                    "The most recent version \"{0}\" referenced in the protocol \"{1}\" " +
                    "is earlier than the version of the protocol (\"{1}\").",
                    latestReferencedVersion, protocol.Name, protocol.Version));
            }
        }

        void CheckNames(Protocol protocol)
        {
            Set<string> usedDeclarationNames = new Set<string>();

            foreach (Declaration declaration in protocol.Declarations)
            {
                if (usedDeclarationNames.Contains(declaration.Identifier))
                {
                    throw new SemanticException(string.Format("The name \"{0}\" is used in more than one declaration.",
                        declaration.Identifier));
                }

                usedDeclarationNames.UnionUpdate(declaration.Identifier);

                Set<string> usedMemberNames = new Set<string>();

                foreach (DeclarationMember member in declaration.MemberBases)
                {
                    if (usedMemberNames.Contains(member.Identifier))
                    {
                        throw new SemanticException(string.Format(
                            "The name \"{0}\" in the declaration \"{1}\" is used in more " +
                            " than once within the declaration.", member.Identifier, declaration.Identifier));
                    }
                }
            }
        }
        
        void CheckStructures(Protocol protocol)
        {
            foreach (Declaration declaration in protocol.Declarations)
            {
                if (!(declaration is Structure)) continue;

                Structure structure = declaration as Structure;

                foreach (StructureMember member in structure.Members)
                {
                    if (member.Modifier == FieldModifier.None && structure.StructureKind != StructureKind.Choice)
                    {
                        throw new SemanticException(string.Format(
                            "The member \"{0}\" in the structure or message \"{1}\" does not have the required \"required\" or \"optional\" " +
                            " qualifier.", member.Identifier, declaration.Identifier));
                    }

                    if (member.Modifier != FieldModifier.None && structure.StructureKind == StructureKind.Choice)
                    {
                        throw new SemanticException(string.Format(
                            "The member \"{0}\" in the choice \"{1}\" must not have a \"required\" or \"optional\" " +
                            " qualifier.", member.Identifier, declaration.Identifier));
                    }

                    if (member.Modifier == FieldModifier.Required && member.Versioning.RemovedInVersion.HasValue)
                    {
                        throw new SemanticException(string.Format(
                            "The member \"{0}\" in the structure or message \"{1}\" is marked as removed, but can not be " +
                            "removed because it is a \"required\" field.",
                            member.Identifier, declaration.Identifier));
                    }
                }
            }
        }
    }
}
