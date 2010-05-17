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

using SD.LLBLGen.Pro.ORMSupportClasses;

#endregion

namespace Interlace.Utilities
{
    public abstract class TreeManager<TEntity, TCollection> 
        where TEntity : EntityBase2
        where TCollection : IEntityCollection2
    {
        protected abstract EntityField2 IdField { get; }
        protected abstract EntityField2 ParentIdField { get; }
        protected abstract EntityField2 NestedLeftField { get; }
        protected abstract EntityField2 NestedRightField { get; }

        protected abstract TCollection CreateEntityCollection();
        protected abstract TCollection GetChildrenCollection(TEntity entity);

        /// <summary>
        /// Checks the database for root entity, which would indicate that the
        /// database has been initialised.
        /// </summary>
        /// <returns><c>true</c> if the root entity exists; otherwise, <c>false</c>.</returns>
        public bool RootEntityExists(IDataAccessAdapter adapter)
        {
            TCollection rootEntity = CreateEntityCollection();

            RelationPredicateBucket bucket = new RelationPredicateBucket(
                new PredicateExpression(ParentIdField == DBNull.Value));

            adapter.FetchEntityCollection(rootEntity, bucket, 2);

            if (rootEntity.Count > 1) 
            {
                throw new InvalidOperationException("More than one root entity was found " +
                    "in a database table using trees.");
            }

            return rootEntity.Count == 1;
        }

        /// <summary>
        /// Checks the database to test if the table is empty.
        /// </summary>
        /// <returns><c>true</c> if any entity exists; otherwise, <c>false</c>.</returns>
        public bool AnyEntityExists(IDataAccessAdapter adapter)
        {
            TCollection anyEntity = CreateEntityCollection();

            adapter.FetchEntityCollection(anyEntity, null, 1);

            return anyEntity.Count > 0;
        }

        /// <summary>
        /// Fixes the NestedLeft and NestedRight properties of a tree
        /// once it has been modified.
        /// </summary>
        /// <param name="rootAccount">The root entity of the tree.</param>
        /// <remarks>
        /// <para>A subtree can not be passed in to this
        /// function; it assumes 0 as the starting NestedLeft, and it
        /// could not allocate new numbers if it was passed a subtree.</para>
        /// <para>Calling this function on a tree of entities always
        /// results in all entities being marked dirty.
        /// </para>
        /// </remarks>
		public void RebuildNesting(TEntity rootEntity)
		{
			// To rebuild the nesting, we do a depth first search on the entity tree. The
			// left nesting value is set on entry, the right on exit. Each time
			// a nesting value is set the next one is incremented.

            if (rootEntity.GetCurrentFieldValue(ParentIdField.FieldIndex) != null)
            {
                throw new ArgumentException("A non-root entity was passed; only the root " +
                    "entity can be passed to rebuild a tree", "rootEntity");
            }

			int clock = 0;
			RebuildNestingRecursor(rootEntity, ref clock);
		}

		void RebuildNestingRecursor(TEntity entity, ref int clock)
		{
            entity.SetNewFieldValue(NestedLeftField.FieldIndex, clock++);

			foreach (TEntity child in GetChildrenCollection(entity))
			{
				RebuildNestingRecursor(child, ref clock);
			}

            entity.SetNewFieldValue(NestedRightField.FieldIndex, clock++);
		}

		/// <summary>
		/// Fetches all elements in to a tree.
		/// </summary>
		/// <returns>The root node of the tree, with each nodes
		/// Children collections populated with child elements.</returns>
		public TEntity FetchTree(IDataAccessAdapter adapter)
		{
            TCollection entities = CreateEntityCollection();

            adapter.FetchEntityCollection(entities, null);

            // Build a hash of all entities by key:
            Dictionary<object, TEntity> entitiesMap = new Dictionary<object, TEntity>();

            foreach (TEntity entity in entities)
            {
                entitiesMap[entity.GetCurrentFieldValue(IdField.FieldIndex)] = entity;
            }

            // Add child entities to the parent collection:
            TEntity rootEntity = null;

            foreach (TEntity entity in entities)
            {
                object parentId = entity.GetCurrentFieldValue(ParentIdField.FieldIndex);

                if (parentId == null)
                {
                    if (rootEntity != null)
                    {
                        throw new InvalidOperationException(string.Format(
                            "Multiple root entities found while building entity tree; " +
                            "\"{0}\" and \"{1}\" are the keys of the first two found.",
                            parentId, entity.GetCurrentFieldValue(IdField.FieldIndex)));
                    }

                    rootEntity = entity;
                }
                else
                {
                    if (!entitiesMap.ContainsKey(parentId))
                    {
                        throw new InvalidOperationException(string.Format(
                            "The entity with the key \"{0}\" refers to a non-existant " +
                            "parent element with the key \"{1}\".",
                            entity.GetCurrentFieldValue(IdField.FieldIndex), parentId));
                    }

                    GetChildrenCollection(entitiesMap[parentId]).Add(entity);
                }
            }

            if (rootEntity == null)
            {
                throw new InvalidOperationException(
                    "No root entity was found in the queried tree table.");
            }

            return rootEntity;
		}

        /// <summary>
        /// Checks the database to find if an entity has any child fields.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the entity has children; otherwise, <c>false</c>.</returns>
        public bool EntityHasChildren(IDataAccessAdapter adapter, TEntity entity)
        {
            TCollection childEntities = CreateEntityCollection();

            RelationPredicateBucket bucket = new RelationPredicateBucket();
            bucket.PredicateExpression.Add(GetDescendantsPredicate(entity));

            adapter.FetchEntityCollection(childEntities, bucket, 1);

            return childEntities.Count > 0;
        }

        /// <summary>
        /// Gets a predicate that would generate the set of all descendant nodes to
        /// the given node.
        /// </summary>
        /// <remarks>The specified node is not included in the generated set.
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public IPredicateExpression GetDescendantsPredicate(TEntity entity)
        {
            return
                NestedLeftField > entity.GetCurrentFieldValue(NestedLeftField.FieldIndex) &
                NestedRightField < entity.GetCurrentFieldValue(NestedRightField.FieldIndex);
        }

        /// <summary>
        /// Gets a predicate that would generate the set of all descendant nodes
        /// plus the specified node.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public IPredicateExpression GetDescendantsAndParentPredicate(TEntity entity)
        {
            return
                NestedLeftField >= entity.GetCurrentFieldValue(NestedLeftField.FieldIndex) &
                NestedRightField <= entity.GetCurrentFieldValue(NestedRightField.FieldIndex);
        }
    }
}
