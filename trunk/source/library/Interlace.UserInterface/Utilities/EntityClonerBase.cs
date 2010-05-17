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
using System.ComponentModel;
using System.Text;

using SD.LLBLGen.Pro.ORMSupportClasses;

using Interlace.Collections;

#endregion

namespace Interlace.Utilities
{
    public abstract class EntityClonerBase
    {
        Type _entityType;
        EntityBase2 _entityPrototype;

        EntityClonerFieldList _fieldsToCopy;
        EntityClonerFieldList _fieldsToIgnore;

        EntityClonerRelationList _relationsToCopy;
        EntityClonerRelationList _relationsToIgnore;

        bool _isRecursive;

        public EntityClonerBase(Type entityType, EntityBase2 entityPrototype)
        {
            _entityType = entityType;
            _entityPrototype = entityPrototype;

            _fieldsToCopy = new EntityClonerFieldList();
            _fieldsToIgnore = new EntityClonerFieldList();

            _relationsToCopy = new EntityClonerRelationList(true);
            _relationsToIgnore = new EntityClonerRelationList(false);

            _isRecursive = true;
        }

        protected abstract EntityClonerMapBase CreateMap();

        public bool IsRecursive
        {
            get { return _isRecursive; }
            set { _isRecursive = value; }
        }

        void EnsureFieldIsNotPrimaryKey(EntityClonerField field)
        {
            if (field.IsPrimaryKey)
            {
                throw new EntityClonerException(string.Format(
                    "Primary keys should never be cloned; the attempt to copy the primary key " +
                    "field \"{0}\" in \"{1}\" failed.",
                    field.Name, _entityType.Name));
            }
        }

        public void EnsureAll()
        {
            EnsureFieldSetsAreDisjoint();
            EnsureAllFieldsAreNotPrimaryKeys();
            EnsureAllFieldsMentioned();

            if (_isRecursive)
            {
                EnsureAllCollectionsMentioned();
                EnsureRelationSetsAreDisjoint();
            }
            else
            {
                if (_relationsToCopy.Dictionary.Count > 0 || _relationsToIgnore.Dictionary.Count > 0)
                {
                    throw new EntityClonerException(string.Format(
                        "An entity cloner for a \"{0}\" is marked as non-recursive but relations " +
                        "have been added to either the copy or ignore lists.", _entityType.Name));
                }
            }
        }

        public bool UpdateBase(EntityBase2 sourceEntity, EntityBase2 destinationEntity)
        {
            EnsureAll();

            if (_isRecursive)
            {
                throw new EntityClonerException(string.Format(
                    "An update was attempted on an entity cloner for a \"{0}\", but the cloner is marked " +
                    "as recursive and is therefore not suitable for updates.", _entityType.Name));
            }

            bool updatesRequired = false;

            foreach (EntityClonerField field in _fieldsToCopy.Set)
            {
                if (!field.IsPrimaryKey)
                {
                    object sourceValue = sourceEntity.Fields[field.FieldIndex].CurrentValue;

                    if (!object.Equals(destinationEntity.Fields[field.FieldIndex].CurrentValue, sourceValue))
                    {
                        destinationEntity.SetNewFieldValue(field.FieldIndex, sourceValue);

                        updatesRequired = true;
                    }
                }
            }

            return updatesRequired;
        }

        public EntityClonerMapBase CloneBase(EntityBase2 sourceEntity)
        {
            EnsureAll();

            EntityClonerMapBase map = CreateMap();

            CloneBaseRecursive(sourceEntity, map);

            map.SetRootEntities(sourceEntity, map.Mappings[sourceEntity]);

            return map;
        }

        public EntityBase2 CloneBaseRecursive(EntityBase2 sourceEntity, EntityClonerMapBase map)
        {
            // Return an existing entity if it has already been cloned; when the graph passed in is a DAG but not 
            // a tree this will occur:
            if (map.Mappings.ContainsKey(sourceEntity))
            {
                return map.Mappings[sourceEntity];
            }

            EntityBase2 destinationEntity = sourceEntity.GetEntityFactory().Create() as EntityBase2;

            foreach (EntityClonerField field in _fieldsToCopy.Set)
            {
                if (!field.IsPrimaryKey)
                {
                    destinationEntity.Fields[field.FieldIndex].CurrentValue = 
                        sourceEntity.Fields[field.FieldIndex].CurrentValue;
                }
            }

            map.AddMappingOfEntities(sourceEntity, destinationEntity);

            if (_isRecursive)
            {
                foreach (IPrefetchPathElement2 pathElement in _relationsToCopy.Dictionary.Keys)
                {
                    object relatedDataObject = sourceEntity.GetRelatedData()[pathElement.PropertyName];
                    
                    if (relatedDataObject is IEntityCollection2)
                    {
                        IEntityCollection2 collection = relatedDataObject as IEntityCollection2;

                        if (collection != null)
                        {
                            foreach (EntityBase2 entity in collection)
                            {
                                EntityBase2 newEntity = _relationsToCopy.Dictionary[pathElement].CloneBaseRecursive(entity, map);
                                destinationEntity.SetRelatedEntityProperty(pathElement.PropertyName, newEntity);
                            }
                        }
                    }
                    else if (relatedDataObject is EntityBase2)
                    {
                        EntityBase2 relatedEntity = relatedDataObject as EntityBase2;
                        EntityBase2 newEntity = _relationsToCopy.Dictionary[pathElement].CloneBaseRecursive(relatedEntity, map);

                        destinationEntity.SetRelatedEntityProperty(pathElement.PropertyName, newEntity);
                    }
                }
            }

            return destinationEntity;
        }

        private void EnsureRelationSetsAreDisjoint()
        {
            // Not yet implemented.
        }

        private void EnsureFieldSetsAreDisjoint()
        {
            Set<EntityClonerField> intersection =
                Set<EntityClonerField>.Intersection(_fieldsToCopy.Set, _fieldsToIgnore.Set);

            if (intersection.Count > 0)
            {
                throw new EntityClonerException(string.Format(
                    "One or more of the fields (\"{0}\") are both in the copy and ignore lists for " +
                    "an entity cloner for a \"{1}\".",
                    NaturalStrings.FormatList(intersection, "\"{0}\""), _entityType.Name));
            }
        }

        void EnsureAllCollectionsMentioned()
        {
            List<IEntityRelation> missingRelations = new List<IEntityRelation>();

            foreach (IEntityRelation relation in _entityPrototype.GetAllRelations())
            {
                if (IsRelationMissingFrom(relation, _relationsToCopy) && IsRelationMissingFrom(relation, _relationsToIgnore))
                {
                    missingRelations.Add(relation);
                }
            }

            if (missingRelations.Count > 0)
            {
                List<string> relationNames = missingRelations.ConvertAll<string>(
                    delegate(IEntityRelation relation) { return relation.MappedFieldName; });

                throw new EntityClonerException(string.Format(
                    "The {0} \"{1}\" was not mentioned while attempting to clone a \"{2}\". " +
                    "All fields and relations must be marked copied or ignored before the cloner can be used.", 
                    relationNames.Count == 1 ? "relation" : "relations",
                    NaturalStrings.FormatList(relationNames, "\"{0}\""), 
                    _entityType.Name));
            }
        }

        bool IsRelationMissingFrom(IEntityRelation relation, EntityClonerRelationList list)
        {
            foreach (IPrefetchPathElement2 pathElement in list.Dictionary.Keys)
            {
                if (pathElement.Relation.MappedFieldName == relation.MappedFieldName)
                {
                    return false;
                }
            }

            return true;
        }

        void EnsureAllFieldsAreNotPrimaryKeys()
        {
            foreach (EntityClonerField field in _fieldsToCopy.Set)
            {
                EnsureFieldIsNotPrimaryKey(field);
            }

            foreach (EntityClonerField field in _fieldsToIgnore.Set)
            {
                EnsureFieldIsNotPrimaryKey(field);
            }
        }

        void EnsureAllFieldsMentioned()
        {
            // Find the set of fields in the entity:
            Set<EntityClonerField> prototypeFields = new Set<EntityClonerField>();

            foreach (EntityField2 field in _entityPrototype.Fields)
            {
                if (!field.IsPrimaryKey)
                {
                    prototypeFields.UnionUpdate(new EntityClonerField(field));
                }
            }

            // Find all mentioned fields:
            Set<EntityClonerField> mentionedFields = Set<EntityClonerField>.Union(
                _fieldsToCopy.Set, _fieldsToIgnore.Set);

            // Check the difference:
            Set<EntityClonerField> missingFields = Set<EntityClonerField>.Difference(
                prototypeFields, mentionedFields);

            if (missingFields.Count > 0)
            {
                throw new EntityClonerException(string.Format(
                    "The {0} \"{1}\" was not mentioned while attempting to clone a \"{2}\". " +
                    "All fields and relations must be marked copied or ignored before the cloner can be used.", 
                    missingFields.Count == 1 ? "entity field" : "entity fields",
                    NaturalStrings.FormatList(missingFields, "\"{0}\""), 
                    _entityType.Name));
            }

            Set<EntityClonerField> extraFields = Set<EntityClonerField>.Difference(
                mentionedFields, prototypeFields);

            if (extraFields.Count > 0)
            {
                throw new EntityClonerException(string.Format(
                    "The {0} \"{1}\" was included in a cloner for a \"{2}\" when the field is not a field " +
                    "in this entity.",
                    extraFields.Count == 1 ? "entity field" : "entity fields",
                    NaturalStrings.FormatList(extraFields, "\"{0}\""), 
                    _entityType.Name));
            }
        }

        bool IsFieldMissingFrom(EntityClonerFieldList fields, EntityField2 comparisonField)
        {
            foreach (EntityClonerField field in fields.Set)
            {
                if (field.Name == comparisonField.Name)
                {
                    return false;
                }
            }

            return true;
        }

        public EntityClonerFieldList Copied
        {
            get { return _fieldsToCopy; }
        }

        public EntityClonerFieldList Ignored
        {
            get { return _fieldsToIgnore; }
        }

        public EntityClonerRelationList CopiedRelations
        {
            get { return _relationsToCopy; }
        }

        public EntityClonerRelationList IgnoredRelations
        {
            get { return _relationsToIgnore; }
        }
    }
}
