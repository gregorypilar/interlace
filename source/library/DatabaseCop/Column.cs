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

using DatabaseCop.RuleHelpers;

#endregion

namespace DatabaseCop
{
    [Serializable]
    public class Column
    {
        Table _table;

        string _name;
        ParsedIdentifier _parsedName;

        Column _previousColumn;
        Column _nextColumn;

        int _ordinalPosition;
        string _columnDefault;
        bool _isNullable;
        string _dataType;
        int? _characterMaximumLength;
        int? _characterOctetLength;
        int? _numericPrecision;
        int? _numericPrecisionRadix;
        int? _numericScale;
        int? _datetimePrecision;
        string _collationName;

        public Column(Table table, string name, int ordinalPosition, string columnDefault, 
            bool isNullable, string dataType, int? characterMaximumLength, int? characterOctetLength,
            int? numericPrecision, int? numericPrecisionRadix, int? numericScale, 
            int? datetimePrecision, string collationName)
        {
            _table = table;

            _name = name;
            _parsedName = new ParsedIdentifier(name);

            _ordinalPosition = ordinalPosition;
            _columnDefault = columnDefault;
            _isNullable = isNullable;
            _dataType = dataType;
            _characterMaximumLength = characterMaximumLength;
            _characterOctetLength = characterOctetLength;
            _numericPrecision = numericPrecision;
            _numericPrecisionRadix = numericPrecisionRadix;
            _numericScale = numericScale;
            _datetimePrecision = datetimePrecision;
            _collationName = collationName;
        }

        internal void SetPreviousColumn(Column column)
        {
            _previousColumn = column;
            _previousColumn._nextColumn = this;
        }

        public Table Table
        {
            get { return _table; }
        }

        public Database Database
        {
            get { return _table.Database; }
        }

        public string Name
        {
            get { return _name; }
        }

        public ParsedIdentifier ParsedName
        {
            get { return _parsedName; }
        }

        public int OrdinalPosition
        {
            get { return _ordinalPosition; }
        }

        public string ColumnDefault
        {
            get { return _columnDefault; }
        }

        public bool IsNullable
        {
            get { return _isNullable; }
        }

        public string DataType
        {
            get { return _dataType; }
        }

        public int? CharacterMaximumLength
        {
            get { return _characterMaximumLength; }
        }

        public int? CharacterOctetLength
        {
            get { return _characterOctetLength; }
        }

        public int? NumericPrecision
        {
            get { return _numericPrecision; }
        }

        public int? NumericPrecisionRadix
        {
            get { return _numericPrecisionRadix; }
        }

        public int? NumericScale
        {
            get { return _numericScale; }
        }

        public int? DatetimePrecision
        {
            get { return _datetimePrecision; }
        }

        public string CollationName
        {
            get { return _collationName; }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Table, _name);
        }

        public Column PreviousColumn
        {
            get
            {
                return _previousColumn;
            }
        }

        public Column NextColumn
        {
            get
            {
                return _nextColumn;
            }
        }
    }
}
