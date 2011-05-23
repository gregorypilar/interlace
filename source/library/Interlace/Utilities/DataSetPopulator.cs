#region Using Directives and Copyright Notice

// Copyright (c) 2010, Bit Plantation
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Bit Plantation nor the
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using Interlace.Collections;

#endregion

namespace Interlace.Utilities
{
    public class DataSetPopulator
    {
        string _xmlSchema;
        ObjectIDGenerator _generator;
        DataSet _dataSet;

        public DataSetPopulator(string xmlSchema)
        {
            _xmlSchema = xmlSchema;
            _generator = new ObjectIDGenerator();

            _dataSet = new DataSet();
            _dataSet.ReadXmlSchema(new StringReader(_xmlSchema));
        }

        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        public void Add(string rootTableName, IList rootList)
        {
            DataTable rootTable = _dataSet.Tables[rootTableName];

            AddToTable(_dataSet, rootTable, rootList, null);
        }

        void AddToTable(DataSet dataSet, DataTable table, IList values, object parentValue)
        {
            if (values.Count == 0) return;

            object prototype = values[0];

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(prototype);

            foreach (object value in values)
            {
                AddValueToTable(table, properties, value, parentValue);
            }

            foreach (DataRelation relation in table.ChildRelations)
            {
                foreach (object value in values)
                {
                    object valueListObject = properties[relation.RelationName].GetValue(value);

                    if (valueListObject == null) continue;

                    if (valueListObject is IList)
                    {
                        IList valueList = (IList)valueListObject;

                        AddToTable(dataSet, relation.ChildTable, valueList, value);
                    }
                    else
                    {
                        AddToTable(dataSet, relation.ChildTable, new object[] { valueListObject }, value);
                    }
                }
            }
        }

        static readonly Set<string> _internalColumnNames = new Set<string>("__RelationId", "__RelationParentId", "__This");

        void AddValueToTable(DataTable table, PropertyDescriptorCollection properties, object value, object parentValue)
        {
            DataRow row = table.Rows.Add();

            foreach (DataColumn column in table.Columns)
            {
                if (_internalColumnNames.Contains(column.ColumnName)) continue;

                object cell = properties[column.ColumnName].GetValue(value);

                if (cell == null) cell = DBNull.Value;

                row[column] = cell;
            }

            bool firstTime;

            if (table.Columns.Contains("__RelationId"))
            {
                row["__RelationId"] = _generator.GetId(value, out firstTime);
            }

            if (parentValue != null && table.Columns.Contains("__RelationParentId"))
            {
                row["__RelationParentId"] = _generator.GetId(parentValue, out firstTime);
            }

            if (table.Columns.Contains("__This"))
            {
                row["__This"] = value;
            }
        }
    }
}
