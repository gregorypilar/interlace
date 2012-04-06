using System;
using System.Collections.Generic;
using System.Text;
using Interlace.Pinch.Dom;
using Interlace.Pinch.Analysis;
using Interlace.Pinch.Implementation;
using System.IO;

namespace Interlace.Pinch.Dynamic
{
    public class DynamicPincher
    {
        Dictionary<string, DynamicPincherProtocol> _protocols;

        public DynamicPincher(string documentPath)
        {
            Document document = Document.Parse(documentPath);

            Compilation compilation = new Compilation();
            compilation.AddDocument(document);
            compilation.Resolve();
            compilation.Number();

            _protocols = new Dictionary<string, DynamicPincherProtocol>();

            foreach (Protocol protocol in document.Protocols)
            {
                _protocols[protocol.Name.ToString()] = new DynamicPincherProtocol(protocol);
            }
        }

        public DynamicStructure Decode(string fullName, byte[] encodedBytes)
        {
            using (MemoryStream stream = new MemoryStream(encodedBytes))
            {
                return Decode(fullName, stream);
            }
        }

        public DynamicStructure Decode(string fullName, Stream stream)
        {
            PinchDecoder decoder = new PinchDecoder(stream);

            return Decode(fullName, decoder);
        }

        public DynamicStructure Decode(string fullName, IPinchDecoder decoder)
        {
            Declaration declaration = ResolveDeclaration(fullName);

            if (!(declaration is Structure))
            {
                throw new PinchException(string.Format("The declaration \"{0}\" is not a structure and can therefore not be decoded."));
            }

            return Decode((Structure)declaration, decoder);
        }

        void DecodeMember(DynamicStructure dynamicStructure, StructureMember member, IPinchDecoder decoder)
        {
            if (member.IsRemoved)
            {
                decoder.SkipRemoved();
            }
            else
            {
                if (member.FieldContainerReference == ContainerType.None)
                {
                    object value = DecodeMemberValue(member, decoder);

                    dynamicStructure.Members[member.Identifier] = new DynamicMember(member.Identifier, value);
                }
                else if (member.FieldContainerReference == ContainerType.List)
                {
                    int count = decoder.OpenSequence();

                    List<object> values = new List<object>();

                    for (int i = 0; i < count; i++)
                    {
                        object value = DecodeMemberValue(member, decoder);

                        values.Add(value);
                    }

                    decoder.CloseSequence();

                    dynamicStructure.Members[member.Identifier] = new DynamicMember(member.Identifier, values);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        object DecodeMemberValue(StructureMember member, IPinchDecoder decoder)
        {
            // Handle removed members:
            if (member.IsRemoved) throw new InvalidOperationException();

            // Set up a few handy variables for the decoders:
            bool required = (member.Modifier & FieldModifier.Required) != FieldModifier.None;

            PinchFieldProperties properties = new PinchFieldProperties(
                member.Number, member.Versioning.AddedInVersion, member.Versioning.RemovedInVersion);

            if (member.FieldTypeReference is IntrinsicTypeReference)
            {
                // Handle intrinsic types:
                IntrinsicTypeReference reference = (IntrinsicTypeReference)member.FieldTypeReference;

                switch (reference.Type)
                {
                    case IntrinsicType.Float32:
                        return required ? decoder.DecodeRequiredFloat32(properties) : decoder.DecodeOptionalFloat32(properties);

                    case IntrinsicType.Float64:
                        return required ? decoder.DecodeRequiredFloat64(properties) : decoder.DecodeOptionalFloat64(properties);

                    case IntrinsicType.Int8:
                        return required ? decoder.DecodeRequiredInt8(properties) : decoder.DecodeOptionalInt8(properties);

                    case IntrinsicType.Int16:
                        return required ? decoder.DecodeRequiredInt16(properties) : decoder.DecodeOptionalInt16(properties);

                    case IntrinsicType.Int32:
                        return required ? decoder.DecodeRequiredInt32(properties) : decoder.DecodeOptionalInt32(properties);

                    case IntrinsicType.Int64:
                        return required ? decoder.DecodeRequiredInt64(properties) : decoder.DecodeOptionalInt64(properties);

                    case IntrinsicType.Decimal:
                        return required ? decoder.DecodeRequiredDecimal(properties) : decoder.DecodeOptionalDecimal(properties);

                    case IntrinsicType.Bool:
                        return required ? decoder.DecodeRequiredBool(properties) : decoder.DecodeOptionalBool(properties);

                    case IntrinsicType.String:
                        return required ? decoder.DecodeRequiredString(properties) : decoder.DecodeOptionalString(properties);

                    case IntrinsicType.Bytes:
                        return required ? decoder.DecodeRequiredBytes(properties) : decoder.DecodeOptionalBytes(properties);

                    default:
                        throw new PinchException("An unknown type was found in the specification object tree.");
                }
            }
            else if (member.FieldTypeReference is DeclarationTypeReference)
            {
                // Handle structure and enumeration types:
                Declaration declaration = ((DeclarationTypeReference)member.FieldTypeReference).Declaration;

                if (declaration is Enumeration)
                {
                    int? value = (int?)(required ? 
                        decoder.DecodeRequiredEnumeration(properties) : decoder.DecodeOptionalEnumeration(properties));

                    if (!value.HasValue) return null;

                    Enumeration enumeration = (Enumeration)declaration;
                    string enumerationValueName = null;

                    foreach (EnumerationMember enumerationMember in enumeration.Members)
                    {
                        if (member.Number == value.Value) 
                        {
                            enumerationValueName = member.Identifier;
                        }
                    }

                    return new DynamicEnumerationValue(enumeration.QualifiedName.ToString(), enumerationValueName, value.Value);
                }
                else if (declaration is Structure)
                {
                    return Decode((Structure)declaration, decoder);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        DynamicStructure Decode(Structure structure, IPinchDecoder decoder)
        {
            if (structure.StructureKind == StructureKind.Message || structure.StructureKind == StructureKind.Structure)
            {
                // Decode a structure:
                DynamicStructure dynamicStructure = new DynamicStructure(structure.QualifiedName.ToString());

                int? remainingFieldsNullable = decoder.OpenOptionalSequence();

                if (!remainingFieldsNullable.HasValue) return null;

                int remainingFields = remainingFieldsNullable.Value;

                foreach (StructureMemberVersionGroup group in structure.VersionGroupedMembers)
                {
                    if (remainingFields >= group.Members.Count)
                    {
                        foreach (StructureMember member in group.Members)
                        {
                            DecodeMember(dynamicStructure, member, decoder);
                        }
                    
                        remainingFields -= group.Members.Count;
                    }
                    else
                    {
                        if (remainingFields != 0) throw new PinchInvalidCodingException();
                    }
                }
                
                if (remainingFields > 0) 
                {
                    decoder.SkipFields(remainingFields);
                }
                
                decoder.CloseSequence();

                return dynamicStructure;
            }
            else if (structure.StructureKind == StructureKind.Choice)
            {
                // Decode a choice:
                int? choiceMarkerNullable = decoder.DecodeOptionalChoiceMarker();

                if (!choiceMarkerNullable.HasValue) return null;

                int choiceMarker = choiceMarkerNullable.Value;

                DeclarationTypeReference declaration = null;

                foreach (StructureMember member in structure.Members)
                {
                    if (member.Number == choiceMarker)
                    {
                        declaration = (DeclarationTypeReference)member.FieldTypeReference;
                    }
                }

                if (declaration == null)
                {
                    throw new PinchException(
                        "A structure in a choice could not be found in the specification.");
                }

                if (!(declaration.Declaration is Structure))
                {
                    throw new PinchException(
                        "A choice value is a non-structure, which is not supported.");
                }

                return Decode((Structure)declaration.Declaration, decoder);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public Declaration ResolveDeclaration(string fullName)
        {
            string[] parts = fullName.Split('.');

            if (parts.Length < 2) throw new PinchException(string.Format(
                "The declaration \"{0}\" was not found in the loaded specification files (and is not a full legal name).", fullName));

            string declarationName = parts[parts.Length - 1];
            string protocolName = string.Join(".", parts, 0, parts.Length - 1);

            if (!_protocols.ContainsKey(protocolName))
            {
                throw new PinchException(string.Format(
                    "The protocol containing the declaration \"{0}\" was not found in the loaded specification files.", fullName));
            }

            Declaration declaration = _protocols[protocolName].FindDeclaration(declarationName);

            if (declaration == null)
            {
                throw new PinchException(string.Format(
                    "The the declaration \"{0}\" was not found in the loaded specification files.", fullName));
            }

            return declaration;
        }
    }
}
