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
using Interlace.Pinch.Analysis;

#endregion

namespace Interlace.Pinch.Dom
{
    public class Structure : Declaration
    {
        TrackedBindingList<StructureMember> _members;
        List<StructureMember> _codingOrderMembers;
        List<StructureMemberVersionGroup> _versionGroupedMembers;
        List<StructureMember> _nonRemovedMembers;
        StructureKind _structureKind;

        public Structure()
        {
            _members = new TrackedBindingList<StructureMember>();
            _members.Added += new EventHandler<TrackedBindingListEventArgs<StructureMember>>(_members_Added);
            _members.Removed += new EventHandler<TrackedBindingListEventArgs<StructureMember>>(_members_Removed);

            _members.ListChanged += new System.ComponentModel.ListChangedEventHandler(_members_ListChanged);

            _codingOrderMembers = null;
            _versionGroupedMembers = null;
            _nonRemovedMembers = null;
        }

        void _members_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            _codingOrderMembers = null;
            _versionGroupedMembers = null;
            _nonRemovedMembers = null;
        }

        void _members_Added(object sender, TrackedBindingListEventArgs<StructureMember> e)
        {
            e.Item.Parent = this;
        }

        void _members_Removed(object sender, TrackedBindingListEventArgs<StructureMember> e)
        {
            e.Item.Parent = null;
        }

        public IList<StructureMember> Members
        {
            get { return _members; }
        }

        public IList<StructureMember> CodingOrderMembers
        {
            get
            {
                if (_codingOrderMembers == null)
                {
                    List<StructureMember> members = new List<StructureMember>(_members);
                    members.Sort(delegate(StructureMember lhs, StructureMember rhs)
                    {
                        return lhs.Number.CompareTo(rhs.Number);
                    });

                    _codingOrderMembers = members;
                }

                return _codingOrderMembers;
            }
        }

        public IList<StructureMemberVersionGroup> VersionGroupedMembers
        {
            get
            {
                if (_versionGroupedMembers == null)
                {
                    _versionGroupedMembers = new List<StructureMemberVersionGroup>();

                    StructureMemberVersionGroup group = null;

                    foreach (StructureMember member in CodingOrderMembers)
                    {
                        int version = member.Versioning.AddedInVersion;

                        if (group == null || group.Version != version)
                        {
                            group = new StructureMemberVersionGroup(this, version);

                            _versionGroupedMembers.Add(group);
                        }

                        group.Members.Add(member);
                    }
                }

                return _versionGroupedMembers;
            }
        }

        public IList<StructureMember> NonRemovedMembers
        {
            get 
            { 
                if (_nonRemovedMembers == null)
                {
                    _nonRemovedMembers = new List<StructureMember>();

                    foreach (StructureMember member in _members)
                    {
                        if (!member.IsRemoved) _nonRemovedMembers.Add(member);
                    }
                }

                return _nonRemovedMembers;
            }
        }

        public override IEnumerable<DeclarationMember> MemberBases
        {
            get 
            {
                foreach (StructureMember member in _members)
                {
                    yield return member;
                }
            }
        }

        public override void SortAndNumberVersionables()
        {
            VersionableUtilities.NumberVersionables(_members);
        }

        public override string KindTag 
        {
            get { return _structureKind == StructureKind.Choice ? "choice" : "structure"; } 
        }

        public StructureKind StructureKind
        { 	 
            get { return _structureKind; }
            set { _structureKind = value; }
        }
    }
}
