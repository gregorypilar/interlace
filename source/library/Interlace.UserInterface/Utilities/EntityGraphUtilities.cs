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
using System.Diagnostics;
using System.Text;

using SD.LLBLGen.Pro.ORMSupportClasses;

using Interlace.Collections;

#endregion

namespace Interlace.Utilities
{
    public static class EntityGraphUtilities
    {
        public static List<IEntityCollection2> GetAllMemberEntityCollections(IEnumerable<IEntity2> entities)
        {
            List<IEntityCollection2> collections = new List<IEntityCollection2>();

            foreach (IEntity2 entity in entities)
            {
                foreach (IEntityCollection2 collection in entity.GetMemberEntityCollections())
                {
                    collections.Add(collection);
                }
            }

            return collections;
        }

        public static IEnumerable<IEntity2> GetAllEntitiesInGraph(IEntity2 rootObject)
        {
            Queue<IEntity2> seen = new Queue<IEntity2>();
            Set<IEntity2> visited = new Set<IEntity2>();

            seen.Enqueue(rootObject);

            while (seen.Count > 0)
            {
                IEntity2 current = seen.Dequeue();
                visited.UnionUpdate(current);

                // Add the connected entities to a temporary list:
                List<IEntity2> newEntities = new List<IEntity2>();

                newEntities.AddRange(current.GetDependentRelatedEntities());
                newEntities.AddRange(current.GetDependingRelatedEntities());

                foreach (IEntityCollection2 collection in current.GetMemberEntityCollections())
                {
                    foreach (IEntity2 entity in collection) newEntities.Add(entity);
                }

                // Ensure they're visited:
                foreach (IEntity2 entity in newEntities)
                {
                    if (!visited.Contains(entity)) seen.Enqueue(entity);
                }
            }

            return visited;
        }

        public static bool IsGraphDirty(IEntity2 rootObject)
        {
            EntityGraphIsDirtyVisitor visitor = new EntityGraphIsDirtyVisitor();

            VisitAllEntities(rootObject, visitor);

            return visitor.Result;
        }

        #region Debugging Helpers

        public static void PrintAllEntitiesInGraph(IEntity2 rootObject)
        {
            EntityGraphPrintingVisitor visitor = new EntityGraphPrintingVisitor(new string[] { });

            VisitAllEntities(rootObject, visitor);

            Console.Write(visitor.Result);
        }

        public static string ExamineAllEntitiesInGraph(IEntity2 rootObject, params string[] fieldsToDisplay)
        {
            EntityGraphPrintingVisitor visitor = new EntityGraphPrintingVisitor(fieldsToDisplay);

            VisitAllEntities(rootObject, visitor);

            return visitor.Result;
        }

        public static string ExamineDirtyEntities(IEntity2 rootObject)
        {
            EntityGraphModificationPrintingVisitor visitor = new EntityGraphModificationPrintingVisitor();

            VisitAllEntities(rootObject, visitor);

            return visitor.Result;
        }

        public static void PrintEntitiesWithMatchingProperty(IEntity2 rootObject, Type entityType, string propertyName, object propertyValue)
        {
            VisitAllEntities(rootObject, new EntityGraphSearchingVisitor(entityType, propertyName, propertyValue));
        }

        public static void VisitAllEntities(IEntity2 rootObject, EntityGraphVisitor visitor)
        {
            VisitAllEntitiesRecursion(new CactusStack<IEntity2>(rootObject), new Set<IEntity2>(), visitor);
        }

        static void VisitAllEntitiesRecursion(CactusStack<IEntity2> current, Set<IEntity2> visited, EntityGraphVisitor visitor)
        {
            visited.UnionUpdate(current.Value);

            visitor.VisitEntity(current);

            // Add the connected entities to a temporary list:
            List<IEntity2> newEntities = new List<IEntity2>();

            newEntities.AddRange(current.Value.GetDependentRelatedEntities());
            newEntities.AddRange(current.Value.GetDependingRelatedEntities());

            foreach (IEntityCollection2 collection in current.Value.GetMemberEntityCollections())
            {
                foreach (IEntity2 entity in collection) newEntities.Add(entity);
            }

            // Recurse into them:
            foreach (IEntity2 entity in newEntities)
            {
                if (!visited.Contains(entity))
                {
                    visitor.BeginVisitingChildren(current);

                    VisitAllEntitiesRecursion(current.Push(entity), visited, visitor);

                    visitor.EndVisitingChildren(current);
                }
                else
                {
                    visitor.VisitAlreadyVisitedEntity(current.Push(entity));
                }
            }
        }

        #endregion
    }
}
