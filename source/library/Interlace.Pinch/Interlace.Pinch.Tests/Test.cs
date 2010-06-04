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

using Interlace.Pinch.Implementation;

#endregion

namespace Interlace.Pinch.Tests
{
    public enum Enumeration
    {
        A = 1,
        B = 3,
        C = 2,
        D = 4
    }
    
    public class ChoiceMessageFactory : IPinchableFactory
    {
        static ChoiceMessageFactory _instance = new ChoiceMessageFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new ChoiceMessage(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class ChoiceMessage : IPinchable, INotifyPropertyChanged
    {
        int _test; 
        ChoiceStructure _choice; 

        static PinchFieldProperties _testProperties = new PinchFieldProperties(2, 1, null); 
        static PinchFieldProperties _choiceProperties = new PinchFieldProperties(1, 1, null); 
        
        public ChoiceMessage()
        {
            

        }
    
        public ChoiceMessage(IPinchDecodingContext context)
        {
        }
        
        public int Test
        {
            get { return _test; }
            set 
            { 
                _test = value; 
                
                FirePropertyChanged("Test");
            }
        }
        
        public ChoiceStructure Choice
        {
            get { return _choice; }
            set 
            { 
                _choice = value; 
                
                FirePropertyChanged("Choice");
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 3;
            }
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenUncountedContainer();
            
            encoder.PrepareEncodeRequiredStructure(_choice, _choiceProperties);
            encoder.PrepareEncodeRequiredInt32(_test, _testProperties);
            
            encoder.PrepareContainer();
            
            encoder.EncodeRequiredStructure(_choice, _choiceProperties);
            encoder.EncodeRequiredInt32(_test, _testProperties);
            
            encoder.CloseContainer();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            decoder.OpenUncountedContainer();
            
            decoder.PrepareDecodeRequiredStructure(_choiceProperties);
            decoder.PrepareDecodeRequiredInt32(_testProperties);
            
            decoder.PrepareContainer();
            
            _choice = (ChoiceStructure)decoder.DecodeRequiredStructure(ChoiceStructureFactory.Instance, _choiceProperties);
            _test = (int)decoder.DecodeRequiredInt32(_testProperties);
            
            decoder.CloseContainer();
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class ChoiceStructureFactory : IPinchableFactory
    {
        static ChoiceStructureFactory _instance = new ChoiceStructureFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new ChoiceStructure(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public enum ChoiceStructureKind
    {
        None = 0,
        
        Small = 3, 
        RequiredDecimal = 2, 
        OptionalDecimal = 1, 
        Versioning = 4, 
    }
    
    public partial class ChoiceStructure : IPinchable, INotifyPropertyChanged
    {
        object _value;
        ChoiceStructureKind _valueKind;
        
        static PinchFieldProperties _smallProperties = new PinchFieldProperties(3, 1, null); 
        static PinchFieldProperties _requiredDecimalProperties = new PinchFieldProperties(2, 1, null); 
        static PinchFieldProperties _optionalDecimalProperties = new PinchFieldProperties(1, 1, null); 
        static PinchFieldProperties _versioningProperties = new PinchFieldProperties(4, 1, null); 
        
        public ChoiceStructure(SmallStructure value)
        {
            _value = value;
            _valueKind = ChoiceStructureKind.Small;
        }
        
        public static implicit operator ChoiceStructure(SmallStructure value)
        {
            return new ChoiceStructure(value);
        }
        
        
        public ChoiceStructure(RequiredDecimalStructure value)
        {
            _value = value;
            _valueKind = ChoiceStructureKind.RequiredDecimal;
        }
        
        public static implicit operator ChoiceStructure(RequiredDecimalStructure value)
        {
            return new ChoiceStructure(value);
        }
        
        
        public ChoiceStructure(OptionalDecimalStructure value)
        {
            _value = value;
            _valueKind = ChoiceStructureKind.OptionalDecimal;
        }
        
        public static implicit operator ChoiceStructure(OptionalDecimalStructure value)
        {
            return new ChoiceStructure(value);
        }
        
        
        public ChoiceStructure(VersioningStructure value)
        {
            _value = value;
            _valueKind = ChoiceStructureKind.Versioning;
        }
        
        public static implicit operator ChoiceStructure(VersioningStructure value)
        {
            return new ChoiceStructure(value);
        }
        

    
        public ChoiceStructure()
        {
            _value = null;
            _valueKind = ChoiceStructureKind.None;
        }
    
        public ChoiceStructure(IPinchDecodingContext context)
        {
        }
        
        public object Value 
        {
            get { return _value; }
        }
        
        public ChoiceStructureKind ValueKind
        {
            get { return _valueKind; }
        }
        
        public SmallStructure Small
        {
            get { return _valueKind == ChoiceStructureKind.Small ? (SmallStructure)_value : null; }
            set 
            { 
                ChoiceStructureKind existingKind = _valueKind;
                
                _value = value; 
                _valueKind = ChoiceStructureKind.Small;
                
                if (existingKind != _valueKind) FirePropertyChanged(existingKind);
                FirePropertyChanged(_valueKind);
            }
        }
        
        public RequiredDecimalStructure RequiredDecimal
        {
            get { return _valueKind == ChoiceStructureKind.RequiredDecimal ? (RequiredDecimalStructure)_value : null; }
            set 
            { 
                ChoiceStructureKind existingKind = _valueKind;
                
                _value = value; 
                _valueKind = ChoiceStructureKind.RequiredDecimal;
                
                if (existingKind != _valueKind) FirePropertyChanged(existingKind);
                FirePropertyChanged(_valueKind);
            }
        }
        
        public OptionalDecimalStructure OptionalDecimal
        {
            get { return _valueKind == ChoiceStructureKind.OptionalDecimal ? (OptionalDecimalStructure)_value : null; }
            set 
            { 
                ChoiceStructureKind existingKind = _valueKind;
                
                _value = value; 
                _valueKind = ChoiceStructureKind.OptionalDecimal;
                
                if (existingKind != _valueKind) FirePropertyChanged(existingKind);
                FirePropertyChanged(_valueKind);
            }
        }
        
        public VersioningStructure Versioning
        {
            get { return _valueKind == ChoiceStructureKind.Versioning ? (VersioningStructure)_value : null; }
            set 
            { 
                ChoiceStructureKind existingKind = _valueKind;
                
                _value = value; 
                _valueKind = ChoiceStructureKind.Versioning;
                
                if (existingKind != _valueKind) FirePropertyChanged(existingKind);
                FirePropertyChanged(_valueKind);
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 3;
            }
        }
        
        public void FirePropertyChanged(ChoiceStructureKind kind)
        {
            switch (kind)
            {
                case ChoiceStructureKind.None:
                    break;
                    
                case ChoiceStructureKind.Small:
                    FirePropertyChanged("Small");
                    break; 
                    
                case ChoiceStructureKind.RequiredDecimal:
                    FirePropertyChanged("RequiredDecimal");
                    break; 
                    
                case ChoiceStructureKind.OptionalDecimal:
                    FirePropertyChanged("OptionalDecimal");
                    break; 
                    
                case ChoiceStructureKind.Versioning:
                    FirePropertyChanged("Versioning");
                    break; 
                    
                default:
                    break;
            }
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenCountedContainer((int)_valueKind);
            
            switch (_valueKind)
            {
                case ChoiceStructureKind.None:
                    throw new PinchNullRequiredFieldException();
                    
                case ChoiceStructureKind.Small:
                    encoder.PrepareEncodeRequiredStructure((IPinchable)_value, _smallProperties);
                    encoder.PrepareContainer();
                    encoder.EncodeRequiredStructure((IPinchable)_value, _smallProperties);
                    break; 
                    
                case ChoiceStructureKind.RequiredDecimal:
                    encoder.PrepareEncodeRequiredStructure((IPinchable)_value, _requiredDecimalProperties);
                    encoder.PrepareContainer();
                    encoder.EncodeRequiredStructure((IPinchable)_value, _requiredDecimalProperties);
                    break; 
                    
                case ChoiceStructureKind.OptionalDecimal:
                    encoder.PrepareEncodeRequiredStructure((IPinchable)_value, _optionalDecimalProperties);
                    encoder.PrepareContainer();
                    encoder.EncodeRequiredStructure((IPinchable)_value, _optionalDecimalProperties);
                    break; 
                    
                case ChoiceStructureKind.Versioning:
                    encoder.PrepareEncodeRequiredStructure((IPinchable)_value, _versioningProperties);
                    encoder.PrepareContainer();
                    encoder.EncodeRequiredStructure((IPinchable)_value, _versioningProperties);
                    break; 
                    
            }
            
            encoder.CloseContainer();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            _valueKind = (ChoiceStructureKind)decoder.OpenCountedContainer();
            
            switch (_valueKind)
            {
                case ChoiceStructureKind.Small:
                    decoder.PrepareDecodeRequiredStructure(_smallProperties);
                    decoder.PrepareContainer();
                    _value = decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _smallProperties);
                    break; 
                    
                case ChoiceStructureKind.RequiredDecimal:
                    decoder.PrepareDecodeRequiredStructure(_requiredDecimalProperties);
                    decoder.PrepareContainer();
                    _value = decoder.DecodeRequiredStructure(RequiredDecimalStructureFactory.Instance, _requiredDecimalProperties);
                    break; 
                    
                case ChoiceStructureKind.OptionalDecimal:
                    decoder.PrepareDecodeRequiredStructure(_optionalDecimalProperties);
                    decoder.PrepareContainer();
                    _value = decoder.DecodeRequiredStructure(OptionalDecimalStructureFactory.Instance, _optionalDecimalProperties);
                    break; 
                    
                case ChoiceStructureKind.Versioning:
                    decoder.PrepareDecodeRequiredStructure(_versioningProperties);
                    decoder.PrepareContainer();
                    _value = decoder.DecodeRequiredStructure(VersioningStructureFactory.Instance, _versioningProperties);
                    break; 
                    
                default:
                    throw new PinchInvalidCodingException();
            }
            
            decoder.CloseContainer();
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class SmallStructureFactory : IPinchableFactory
    {
        static SmallStructureFactory _instance = new SmallStructureFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new SmallStructure(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class SmallStructure : IPinchable, INotifyPropertyChanged
    {
        byte _test; 

        static PinchFieldProperties _testProperties = new PinchFieldProperties(1, 1, null); 
        
        public SmallStructure()
        {
        }
    
        public SmallStructure(IPinchDecodingContext context)
        {
        }
        
        public byte Test
        {
            get { return _test; }
            set 
            { 
                _test = value; 
                
                FirePropertyChanged("Test");
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 3;
            }
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenUncountedContainer();
            
            encoder.PrepareEncodeRequiredInt8(_test, _testProperties);
            
            encoder.PrepareContainer();
            
            encoder.EncodeRequiredInt8(_test, _testProperties);
            
            encoder.CloseContainer();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            decoder.OpenUncountedContainer();
            
            decoder.PrepareDecodeRequiredInt8(_testProperties);
            
            decoder.PrepareContainer();
            
            _test = (byte)decoder.DecodeRequiredInt8(_testProperties);
            
            decoder.CloseContainer();
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class RequiredDecimalStructureFactory : IPinchableFactory
    {
        static RequiredDecimalStructureFactory _instance = new RequiredDecimalStructureFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new RequiredDecimalStructure(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class RequiredDecimalStructure : IPinchable, INotifyPropertyChanged
    {
        decimal _value; 

        static PinchFieldProperties _valueProperties = new PinchFieldProperties(1, 1, null); 
        
        public RequiredDecimalStructure()
        {
        }
    
        public RequiredDecimalStructure(IPinchDecodingContext context)
        {
        }
        
        public decimal Value
        {
            get { return _value; }
            set 
            { 
                _value = value; 
                
                FirePropertyChanged("Value");
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 3;
            }
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenUncountedContainer();
            
            encoder.PrepareEncodeRequiredDecimal(_value, _valueProperties);
            
            encoder.PrepareContainer();
            
            encoder.EncodeRequiredDecimal(_value, _valueProperties);
            
            encoder.CloseContainer();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            decoder.OpenUncountedContainer();
            
            decoder.PrepareDecodeRequiredDecimal(_valueProperties);
            
            decoder.PrepareContainer();
            
            _value = (decimal)decoder.DecodeRequiredDecimal(_valueProperties);
            
            decoder.CloseContainer();
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class OptionalDecimalStructureFactory : IPinchableFactory
    {
        static OptionalDecimalStructureFactory _instance = new OptionalDecimalStructureFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new OptionalDecimalStructure(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class OptionalDecimalStructure : IPinchable, INotifyPropertyChanged
    {
        decimal? _value; 

        static PinchFieldProperties _valueProperties = new PinchFieldProperties(1, 1, null); 
        
        public OptionalDecimalStructure()
        {
        }
    
        public OptionalDecimalStructure(IPinchDecodingContext context)
        {
        }
        
        public decimal? Value
        {
            get { return _value; }
            set 
            { 
                _value = value; 
                
                FirePropertyChanged("Value");
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 3;
            }
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenUncountedContainer();
            
            encoder.PrepareEncodeOptionalDecimal(_value, _valueProperties);
            
            encoder.PrepareContainer();
            
            encoder.EncodeOptionalDecimal(_value, _valueProperties);
            
            encoder.CloseContainer();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            decoder.OpenUncountedContainer();
            
            decoder.PrepareDecodeOptionalDecimal(_valueProperties);
            
            decoder.PrepareContainer();
            
            _value = (decimal?)decoder.DecodeOptionalDecimal(_valueProperties);
            
            decoder.CloseContainer();
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class VersioningStructureFactory : IPinchableFactory
    {
        static VersioningStructureFactory _instance = new VersioningStructureFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new VersioningStructure(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class VersioningStructure : IPinchable, INotifyPropertyChanged
    {
        byte _reqScalar; 
        string _reqPointer; 
        SmallStructure _reqStructure; 
        byte? _optScalar; 
        string _optPointer; 
        SmallStructure _optStructure; 
        byte _removedReqScalar; 
        string _removedReqPointer; 
        SmallStructure _removedReqStructure; 
        byte? _removedOptScalar; 
        string _removedOptPointer; 
        SmallStructure _removedOptStructure; 
        byte _addedReqScalar; 
        string _addedReqPointer; 
        SmallStructure _addedReqStructure; 
        byte? _addedOptScalar; 
        string _addedOptPointer; 
        SmallStructure _addedOptStructure; 

        static PinchFieldProperties _reqScalarProperties = new PinchFieldProperties(11, 1, null); 
        static PinchFieldProperties _reqPointerProperties = new PinchFieldProperties(10, 1, null); 
        static PinchFieldProperties _reqStructureProperties = new PinchFieldProperties(12, 1, null); 
        static PinchFieldProperties _optScalarProperties = new PinchFieldProperties(2, 1, null); 
        static PinchFieldProperties _optPointerProperties = new PinchFieldProperties(1, 1, null); 
        static PinchFieldProperties _optStructureProperties = new PinchFieldProperties(3, 1, null); 
        static PinchFieldProperties _removedReqScalarProperties = new PinchFieldProperties(8, 1, 2); 
        static PinchFieldProperties _removedReqPointerProperties = new PinchFieldProperties(7, 1, 2); 
        static PinchFieldProperties _removedReqStructureProperties = new PinchFieldProperties(9, 1, 2); 
        static PinchFieldProperties _removedOptScalarProperties = new PinchFieldProperties(5, 1, 2); 
        static PinchFieldProperties _removedOptPointerProperties = new PinchFieldProperties(4, 1, 2); 
        static PinchFieldProperties _removedOptStructureProperties = new PinchFieldProperties(6, 1, 2); 
        static PinchFieldProperties _addedReqScalarProperties = new PinchFieldProperties(17, 2, null); 
        static PinchFieldProperties _addedReqPointerProperties = new PinchFieldProperties(16, 2, null); 
        static PinchFieldProperties _addedReqStructureProperties = new PinchFieldProperties(18, 2, null); 
        static PinchFieldProperties _addedOptScalarProperties = new PinchFieldProperties(14, 2, null); 
        static PinchFieldProperties _addedOptPointerProperties = new PinchFieldProperties(13, 2, null); 
        static PinchFieldProperties _addedOptStructureProperties = new PinchFieldProperties(15, 2, null); 
        
        public VersioningStructure()
        {
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            

        }
    
        public VersioningStructure(IPinchDecodingContext context)
        {
        }
        
        public byte ReqScalar
        {
            get { return _reqScalar; }
            set 
            { 
                _reqScalar = value; 
                
                FirePropertyChanged("ReqScalar");
            }
        }
        
        public string ReqPointer
        {
            get { return _reqPointer; }
            set 
            { 
                _reqPointer = value; 
                
                FirePropertyChanged("ReqPointer");
            }
        }
        
        public SmallStructure ReqStructure
        {
            get { return _reqStructure; }
            set 
            { 
                _reqStructure = value; 
                
                FirePropertyChanged("ReqStructure");
            }
        }
        
        public byte? OptScalar
        {
            get { return _optScalar; }
            set 
            { 
                _optScalar = value; 
                
                FirePropertyChanged("OptScalar");
            }
        }
        
        public string OptPointer
        {
            get { return _optPointer; }
            set 
            { 
                _optPointer = value; 
                
                FirePropertyChanged("OptPointer");
            }
        }
        
        public SmallStructure OptStructure
        {
            get { return _optStructure; }
            set 
            { 
                _optStructure = value; 
                
                FirePropertyChanged("OptStructure");
            }
        }
        
        public byte RemovedReqScalar
        {
            get { return _removedReqScalar; }
            set 
            { 
                _removedReqScalar = value; 
                
                FirePropertyChanged("RemovedReqScalar");
            }
        }
        
        public string RemovedReqPointer
        {
            get { return _removedReqPointer; }
            set 
            { 
                _removedReqPointer = value; 
                
                FirePropertyChanged("RemovedReqPointer");
            }
        }
        
        public SmallStructure RemovedReqStructure
        {
            get { return _removedReqStructure; }
            set 
            { 
                _removedReqStructure = value; 
                
                FirePropertyChanged("RemovedReqStructure");
            }
        }
        
        public byte? RemovedOptScalar
        {
            get { return _removedOptScalar; }
            set 
            { 
                _removedOptScalar = value; 
                
                FirePropertyChanged("RemovedOptScalar");
            }
        }
        
        public string RemovedOptPointer
        {
            get { return _removedOptPointer; }
            set 
            { 
                _removedOptPointer = value; 
                
                FirePropertyChanged("RemovedOptPointer");
            }
        }
        
        public SmallStructure RemovedOptStructure
        {
            get { return _removedOptStructure; }
            set 
            { 
                _removedOptStructure = value; 
                
                FirePropertyChanged("RemovedOptStructure");
            }
        }
        
        public byte AddedReqScalar
        {
            get { return _addedReqScalar; }
            set 
            { 
                _addedReqScalar = value; 
                
                FirePropertyChanged("AddedReqScalar");
            }
        }
        
        public string AddedReqPointer
        {
            get { return _addedReqPointer; }
            set 
            { 
                _addedReqPointer = value; 
                
                FirePropertyChanged("AddedReqPointer");
            }
        }
        
        public SmallStructure AddedReqStructure
        {
            get { return _addedReqStructure; }
            set 
            { 
                _addedReqStructure = value; 
                
                FirePropertyChanged("AddedReqStructure");
            }
        }
        
        public byte? AddedOptScalar
        {
            get { return _addedOptScalar; }
            set 
            { 
                _addedOptScalar = value; 
                
                FirePropertyChanged("AddedOptScalar");
            }
        }
        
        public string AddedOptPointer
        {
            get { return _addedOptPointer; }
            set 
            { 
                _addedOptPointer = value; 
                
                FirePropertyChanged("AddedOptPointer");
            }
        }
        
        public SmallStructure AddedOptStructure
        {
            get { return _addedOptStructure; }
            set 
            { 
                _addedOptStructure = value; 
                
                FirePropertyChanged("AddedOptStructure");
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 3;
            }
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenUncountedContainer();
            
            encoder.PrepareEncodeOptionalString(_optPointer, _optPointerProperties);
            encoder.PrepareEncodeOptionalInt8(_optScalar, _optScalarProperties);
            encoder.PrepareEncodeOptionalStructure(_optStructure, _optStructureProperties);
            encoder.PrepareEncodeOptionalString(_removedOptPointer, _removedOptPointerProperties);
            encoder.PrepareEncodeOptionalInt8(_removedOptScalar, _removedOptScalarProperties);
            encoder.PrepareEncodeOptionalStructure(_removedOptStructure, _removedOptStructureProperties);
            encoder.PrepareEncodeRequiredString(_removedReqPointer, _removedReqPointerProperties);
            encoder.PrepareEncodeRequiredInt8(_removedReqScalar, _removedReqScalarProperties);
            encoder.PrepareEncodeRequiredStructure(_removedReqStructure, _removedReqStructureProperties);
            encoder.PrepareEncodeRequiredString(_reqPointer, _reqPointerProperties);
            encoder.PrepareEncodeRequiredInt8(_reqScalar, _reqScalarProperties);
            encoder.PrepareEncodeRequiredStructure(_reqStructure, _reqStructureProperties);
            encoder.PrepareEncodeOptionalString(_addedOptPointer, _addedOptPointerProperties);
            encoder.PrepareEncodeOptionalInt8(_addedOptScalar, _addedOptScalarProperties);
            encoder.PrepareEncodeOptionalStructure(_addedOptStructure, _addedOptStructureProperties);
            encoder.PrepareEncodeRequiredString(_addedReqPointer, _addedReqPointerProperties);
            encoder.PrepareEncodeRequiredInt8(_addedReqScalar, _addedReqScalarProperties);
            encoder.PrepareEncodeRequiredStructure(_addedReqStructure, _addedReqStructureProperties);
            
            encoder.PrepareContainer();
            
            encoder.EncodeOptionalString(_optPointer, _optPointerProperties);
            encoder.EncodeOptionalInt8(_optScalar, _optScalarProperties);
            encoder.EncodeOptionalStructure(_optStructure, _optStructureProperties);
            encoder.EncodeOptionalString(_removedOptPointer, _removedOptPointerProperties);
            encoder.EncodeOptionalInt8(_removedOptScalar, _removedOptScalarProperties);
            encoder.EncodeOptionalStructure(_removedOptStructure, _removedOptStructureProperties);
            encoder.EncodeRequiredString(_removedReqPointer, _removedReqPointerProperties);
            encoder.EncodeRequiredInt8(_removedReqScalar, _removedReqScalarProperties);
            encoder.EncodeRequiredStructure(_removedReqStructure, _removedReqStructureProperties);
            encoder.EncodeRequiredString(_reqPointer, _reqPointerProperties);
            encoder.EncodeRequiredInt8(_reqScalar, _reqScalarProperties);
            encoder.EncodeRequiredStructure(_reqStructure, _reqStructureProperties);
            encoder.EncodeOptionalString(_addedOptPointer, _addedOptPointerProperties);
            encoder.EncodeOptionalInt8(_addedOptScalar, _addedOptScalarProperties);
            encoder.EncodeOptionalStructure(_addedOptStructure, _addedOptStructureProperties);
            encoder.EncodeRequiredString(_addedReqPointer, _addedReqPointerProperties);
            encoder.EncodeRequiredInt8(_addedReqScalar, _addedReqScalarProperties);
            encoder.EncodeRequiredStructure(_addedReqStructure, _addedReqStructureProperties);
            
            encoder.CloseContainer();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            decoder.OpenUncountedContainer();
            
            decoder.PrepareDecodeOptionalString(_optPointerProperties);
            decoder.PrepareDecodeOptionalInt8(_optScalarProperties);
            decoder.PrepareDecodeOptionalStructure(_optStructureProperties);
            decoder.PrepareDecodeOptionalString(_removedOptPointerProperties);
            decoder.PrepareDecodeOptionalInt8(_removedOptScalarProperties);
            decoder.PrepareDecodeOptionalStructure(_removedOptStructureProperties);
            decoder.PrepareDecodeRequiredString(_removedReqPointerProperties);
            decoder.PrepareDecodeRequiredInt8(_removedReqScalarProperties);
            decoder.PrepareDecodeRequiredStructure(_removedReqStructureProperties);
            decoder.PrepareDecodeRequiredString(_reqPointerProperties);
            decoder.PrepareDecodeRequiredInt8(_reqScalarProperties);
            decoder.PrepareDecodeRequiredStructure(_reqStructureProperties);
            decoder.PrepareDecodeOptionalString(_addedOptPointerProperties);
            decoder.PrepareDecodeOptionalInt8(_addedOptScalarProperties);
            decoder.PrepareDecodeOptionalStructure(_addedOptStructureProperties);
            decoder.PrepareDecodeRequiredString(_addedReqPointerProperties);
            decoder.PrepareDecodeRequiredInt8(_addedReqScalarProperties);
            decoder.PrepareDecodeRequiredStructure(_addedReqStructureProperties);
            
            decoder.PrepareContainer();
            
            _optPointer = (string)decoder.DecodeOptionalString(_optPointerProperties);
            _optScalar = (byte?)decoder.DecodeOptionalInt8(_optScalarProperties);
            _optStructure = (SmallStructure)decoder.DecodeOptionalStructure(SmallStructureFactory.Instance, _optStructureProperties);
            _removedOptPointer = (string)decoder.DecodeOptionalString(_removedOptPointerProperties);
            _removedOptScalar = (byte?)decoder.DecodeOptionalInt8(_removedOptScalarProperties);
            _removedOptStructure = (SmallStructure)decoder.DecodeOptionalStructure(SmallStructureFactory.Instance, _removedOptStructureProperties);
            _removedReqPointer = (string)decoder.DecodeRequiredString(_removedReqPointerProperties);
            _removedReqScalar = (byte)decoder.DecodeRequiredInt8(_removedReqScalarProperties);
            _removedReqStructure = (SmallStructure)decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _removedReqStructureProperties);
            _reqPointer = (string)decoder.DecodeRequiredString(_reqPointerProperties);
            _reqScalar = (byte)decoder.DecodeRequiredInt8(_reqScalarProperties);
            _reqStructure = (SmallStructure)decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _reqStructureProperties);
            _addedOptPointer = (string)decoder.DecodeOptionalString(_addedOptPointerProperties);
            _addedOptScalar = (byte?)decoder.DecodeOptionalInt8(_addedOptScalarProperties);
            _addedOptStructure = (SmallStructure)decoder.DecodeOptionalStructure(SmallStructureFactory.Instance, _addedOptStructureProperties);
            _addedReqPointer = (string)decoder.DecodeRequiredString(_addedReqPointerProperties);
            _addedReqScalar = (byte)decoder.DecodeRequiredInt8(_addedReqScalarProperties);
            _addedReqStructure = (SmallStructure)decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _addedReqStructureProperties);
            
            decoder.CloseContainer();
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public enum TypesEnumeration
    {
        A = 1,
        B = 2,
        C = 3
    }
    
    public class TypesStructureFactory : IPinchableFactory
    {
        static TypesStructureFactory _instance = new TypesStructureFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new TypesStructure(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class TypesStructure : IPinchable, INotifyPropertyChanged
    {
        float _reqFloat32; 
        double _reqFloat64; 
        byte _reqInt8; 
        short _reqInt16; 
        int _reqInt32; 
        long _reqInt64; 
        decimal _reqDecimal; 
        bool _reqBool; 
        string _reqString; 
        byte[] _reqBytes; 
        TypesEnumeration _reqEnumeration; 
        SmallStructure _reqStructure; 
        List<SmallStructure> _reqListOfEnum; 
        float? _optFloat32; 
        double? _optFloat64; 
        byte? _optInt8; 
        short? _optInt16; 
        int? _optInt32; 
        long? _optInt64; 
        decimal? _optDecimal; 
        bool? _optBool; 
        string _optString; 
        byte[] _optBytes; 
        TypesEnumeration? _optEnumeration; 
        SmallStructure _optStructure; 
        List<SmallStructure> _optListOfEnum; 

        static PinchFieldProperties _reqFloat32Properties = new PinchFieldProperties(18, 1, null); 
        static PinchFieldProperties _reqFloat64Properties = new PinchFieldProperties(19, 1, null); 
        static PinchFieldProperties _reqInt8Properties = new PinchFieldProperties(23, 1, null); 
        static PinchFieldProperties _reqInt16Properties = new PinchFieldProperties(20, 1, null); 
        static PinchFieldProperties _reqInt32Properties = new PinchFieldProperties(21, 1, null); 
        static PinchFieldProperties _reqInt64Properties = new PinchFieldProperties(22, 1, null); 
        static PinchFieldProperties _reqDecimalProperties = new PinchFieldProperties(16, 1, null); 
        static PinchFieldProperties _reqBoolProperties = new PinchFieldProperties(14, 1, null); 
        static PinchFieldProperties _reqStringProperties = new PinchFieldProperties(25, 1, null); 
        static PinchFieldProperties _reqBytesProperties = new PinchFieldProperties(15, 1, null); 
        static PinchFieldProperties _reqEnumerationProperties = new PinchFieldProperties(17, 1, null); 
        static PinchFieldProperties _reqStructureProperties = new PinchFieldProperties(26, 1, null); 
        static PinchFieldProperties _reqListOfEnumProperties = new PinchFieldProperties(24, 1, null); 
        static PinchFieldProperties _optFloat32Properties = new PinchFieldProperties(5, 1, null); 
        static PinchFieldProperties _optFloat64Properties = new PinchFieldProperties(6, 1, null); 
        static PinchFieldProperties _optInt8Properties = new PinchFieldProperties(10, 1, null); 
        static PinchFieldProperties _optInt16Properties = new PinchFieldProperties(7, 1, null); 
        static PinchFieldProperties _optInt32Properties = new PinchFieldProperties(8, 1, null); 
        static PinchFieldProperties _optInt64Properties = new PinchFieldProperties(9, 1, null); 
        static PinchFieldProperties _optDecimalProperties = new PinchFieldProperties(3, 1, null); 
        static PinchFieldProperties _optBoolProperties = new PinchFieldProperties(1, 1, null); 
        static PinchFieldProperties _optStringProperties = new PinchFieldProperties(12, 1, null); 
        static PinchFieldProperties _optBytesProperties = new PinchFieldProperties(2, 1, null); 
        static PinchFieldProperties _optEnumerationProperties = new PinchFieldProperties(4, 1, null); 
        static PinchFieldProperties _optStructureProperties = new PinchFieldProperties(13, 1, null); 
        static PinchFieldProperties _optListOfEnumProperties = new PinchFieldProperties(11, 1, null); 
        
        public TypesStructure()
        {
            
            
            
            
            
            
            
            
            
            
            
            
            _reqListOfEnum = new List<SmallStructure>();
            
            
            
            
            
            
            
            
            
            
            
            
            _optListOfEnum = new List<SmallStructure>();
        }
    
        public TypesStructure(IPinchDecodingContext context)
        {
        }
        
        public float ReqFloat32
        {
            get { return _reqFloat32; }
            set 
            { 
                _reqFloat32 = value; 
                
                FirePropertyChanged("ReqFloat32");
            }
        }
        
        public double ReqFloat64
        {
            get { return _reqFloat64; }
            set 
            { 
                _reqFloat64 = value; 
                
                FirePropertyChanged("ReqFloat64");
            }
        }
        
        public byte ReqInt8
        {
            get { return _reqInt8; }
            set 
            { 
                _reqInt8 = value; 
                
                FirePropertyChanged("ReqInt8");
            }
        }
        
        public short ReqInt16
        {
            get { return _reqInt16; }
            set 
            { 
                _reqInt16 = value; 
                
                FirePropertyChanged("ReqInt16");
            }
        }
        
        public int ReqInt32
        {
            get { return _reqInt32; }
            set 
            { 
                _reqInt32 = value; 
                
                FirePropertyChanged("ReqInt32");
            }
        }
        
        public long ReqInt64
        {
            get { return _reqInt64; }
            set 
            { 
                _reqInt64 = value; 
                
                FirePropertyChanged("ReqInt64");
            }
        }
        
        public decimal ReqDecimal
        {
            get { return _reqDecimal; }
            set 
            { 
                _reqDecimal = value; 
                
                FirePropertyChanged("ReqDecimal");
            }
        }
        
        public bool ReqBool
        {
            get { return _reqBool; }
            set 
            { 
                _reqBool = value; 
                
                FirePropertyChanged("ReqBool");
            }
        }
        
        public string ReqString
        {
            get { return _reqString; }
            set 
            { 
                _reqString = value; 
                
                FirePropertyChanged("ReqString");
            }
        }
        
        public byte[] ReqBytes
        {
            get { return _reqBytes; }
            set 
            { 
                _reqBytes = value; 
                
                FirePropertyChanged("ReqBytes");
            }
        }
        
        public TypesEnumeration ReqEnumeration
        {
            get { return _reqEnumeration; }
            set 
            { 
                _reqEnumeration = value; 
                
                FirePropertyChanged("ReqEnumeration");
            }
        }
        
        public SmallStructure ReqStructure
        {
            get { return _reqStructure; }
            set 
            { 
                _reqStructure = value; 
                
                FirePropertyChanged("ReqStructure");
            }
        }
        
        public List<SmallStructure> ReqListOfEnum
        {
            get { return _reqListOfEnum; }
            set 
            { 
                _reqListOfEnum = value; 
                
                FirePropertyChanged("ReqListOfEnum");
            }
        }
        
        public float? OptFloat32
        {
            get { return _optFloat32; }
            set 
            { 
                _optFloat32 = value; 
                
                FirePropertyChanged("OptFloat32");
            }
        }
        
        public double? OptFloat64
        {
            get { return _optFloat64; }
            set 
            { 
                _optFloat64 = value; 
                
                FirePropertyChanged("OptFloat64");
            }
        }
        
        public byte? OptInt8
        {
            get { return _optInt8; }
            set 
            { 
                _optInt8 = value; 
                
                FirePropertyChanged("OptInt8");
            }
        }
        
        public short? OptInt16
        {
            get { return _optInt16; }
            set 
            { 
                _optInt16 = value; 
                
                FirePropertyChanged("OptInt16");
            }
        }
        
        public int? OptInt32
        {
            get { return _optInt32; }
            set 
            { 
                _optInt32 = value; 
                
                FirePropertyChanged("OptInt32");
            }
        }
        
        public long? OptInt64
        {
            get { return _optInt64; }
            set 
            { 
                _optInt64 = value; 
                
                FirePropertyChanged("OptInt64");
            }
        }
        
        public decimal? OptDecimal
        {
            get { return _optDecimal; }
            set 
            { 
                _optDecimal = value; 
                
                FirePropertyChanged("OptDecimal");
            }
        }
        
        public bool? OptBool
        {
            get { return _optBool; }
            set 
            { 
                _optBool = value; 
                
                FirePropertyChanged("OptBool");
            }
        }
        
        public string OptString
        {
            get { return _optString; }
            set 
            { 
                _optString = value; 
                
                FirePropertyChanged("OptString");
            }
        }
        
        public byte[] OptBytes
        {
            get { return _optBytes; }
            set 
            { 
                _optBytes = value; 
                
                FirePropertyChanged("OptBytes");
            }
        }
        
        public TypesEnumeration? OptEnumeration
        {
            get { return _optEnumeration; }
            set 
            { 
                _optEnumeration = value; 
                
                FirePropertyChanged("OptEnumeration");
            }
        }
        
        public SmallStructure OptStructure
        {
            get { return _optStructure; }
            set 
            { 
                _optStructure = value; 
                
                FirePropertyChanged("OptStructure");
            }
        }
        
        public List<SmallStructure> OptListOfEnum
        {
            get { return _optListOfEnum; }
            set 
            { 
                _optListOfEnum = value; 
                
                FirePropertyChanged("OptListOfEnum");
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 3;
            }
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenUncountedContainer();
            
            encoder.PrepareEncodeOptionalBool(_optBool, _optBoolProperties);
            encoder.PrepareEncodeOptionalBytes(_optBytes, _optBytesProperties);
            encoder.PrepareEncodeOptionalDecimal(_optDecimal, _optDecimalProperties);
            encoder.PrepareEncodeOptionalEnumeration(_optEnumeration, _optEnumerationProperties);
            encoder.PrepareEncodeOptionalFloat32(_optFloat32, _optFloat32Properties);
            encoder.PrepareEncodeOptionalFloat64(_optFloat64, _optFloat64Properties);
            encoder.PrepareEncodeOptionalInt16(_optInt16, _optInt16Properties);
            encoder.PrepareEncodeOptionalInt32(_optInt32, _optInt32Properties);
            encoder.PrepareEncodeOptionalInt64(_optInt64, _optInt64Properties);
            encoder.PrepareEncodeOptionalInt8(_optInt8, _optInt8Properties);
            
            encoder.PrepareEncodeOptionalString(_optString, _optStringProperties);
            encoder.PrepareEncodeOptionalStructure(_optStructure, _optStructureProperties);
            encoder.PrepareEncodeRequiredBool(_reqBool, _reqBoolProperties);
            encoder.PrepareEncodeRequiredBytes(_reqBytes, _reqBytesProperties);
            encoder.PrepareEncodeRequiredDecimal(_reqDecimal, _reqDecimalProperties);
            encoder.PrepareEncodeRequiredEnumeration(_reqEnumeration, _reqEnumerationProperties);
            encoder.PrepareEncodeRequiredFloat32(_reqFloat32, _reqFloat32Properties);
            encoder.PrepareEncodeRequiredFloat64(_reqFloat64, _reqFloat64Properties);
            encoder.PrepareEncodeRequiredInt16(_reqInt16, _reqInt16Properties);
            encoder.PrepareEncodeRequiredInt32(_reqInt32, _reqInt32Properties);
            encoder.PrepareEncodeRequiredInt64(_reqInt64, _reqInt64Properties);
            encoder.PrepareEncodeRequiredInt8(_reqInt8, _reqInt8Properties);
            
            encoder.PrepareEncodeRequiredString(_reqString, _reqStringProperties);
            encoder.PrepareEncodeRequiredStructure(_reqStructure, _reqStructureProperties);
            
            encoder.PrepareContainer();
            
            encoder.EncodeOptionalBool(_optBool, _optBoolProperties);
            encoder.EncodeOptionalBytes(_optBytes, _optBytesProperties);
            encoder.EncodeOptionalDecimal(_optDecimal, _optDecimalProperties);
            encoder.EncodeOptionalEnumeration(_optEnumeration, _optEnumerationProperties);
            encoder.EncodeOptionalFloat32(_optFloat32, _optFloat32Properties);
            encoder.EncodeOptionalFloat64(_optFloat64, _optFloat64Properties);
            encoder.EncodeOptionalInt16(_optInt16, _optInt16Properties);
            encoder.EncodeOptionalInt32(_optInt32, _optInt32Properties);
            encoder.EncodeOptionalInt64(_optInt64, _optInt64Properties);
            encoder.EncodeOptionalInt8(_optInt8, _optInt8Properties);
            
            encoder.OpenCountedContainer(_optListOfEnum.Count);
            
            foreach (SmallStructure value in _optListOfEnum)
            {
                encoder.PrepareEncodeOptionalStructure(value, _optListOfEnumProperties);
            }
            
            encoder.PrepareContainer();
            
            foreach (SmallStructure value in _optListOfEnum)
            {
                encoder.EncodeOptionalStructure(value, _optListOfEnumProperties);
            }
            
            encoder.CloseContainer();
            
            encoder.EncodeOptionalString(_optString, _optStringProperties);
            encoder.EncodeOptionalStructure(_optStructure, _optStructureProperties);
            encoder.EncodeRequiredBool(_reqBool, _reqBoolProperties);
            encoder.EncodeRequiredBytes(_reqBytes, _reqBytesProperties);
            encoder.EncodeRequiredDecimal(_reqDecimal, _reqDecimalProperties);
            encoder.EncodeRequiredEnumeration(_reqEnumeration, _reqEnumerationProperties);
            encoder.EncodeRequiredFloat32(_reqFloat32, _reqFloat32Properties);
            encoder.EncodeRequiredFloat64(_reqFloat64, _reqFloat64Properties);
            encoder.EncodeRequiredInt16(_reqInt16, _reqInt16Properties);
            encoder.EncodeRequiredInt32(_reqInt32, _reqInt32Properties);
            encoder.EncodeRequiredInt64(_reqInt64, _reqInt64Properties);
            encoder.EncodeRequiredInt8(_reqInt8, _reqInt8Properties);
            
            encoder.OpenCountedContainer(_reqListOfEnum.Count);
            
            foreach (SmallStructure value in _reqListOfEnum)
            {
                encoder.PrepareEncodeRequiredStructure(value, _reqListOfEnumProperties);
            }
            
            encoder.PrepareContainer();
            
            foreach (SmallStructure value in _reqListOfEnum)
            {
                encoder.EncodeRequiredStructure(value, _reqListOfEnumProperties);
            }
            
            encoder.CloseContainer();
            
            encoder.EncodeRequiredString(_reqString, _reqStringProperties);
            encoder.EncodeRequiredStructure(_reqStructure, _reqStructureProperties);
            
            encoder.CloseContainer();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            decoder.OpenUncountedContainer();
            
            decoder.PrepareDecodeOptionalBool(_optBoolProperties);
            decoder.PrepareDecodeOptionalBytes(_optBytesProperties);
            decoder.PrepareDecodeOptionalDecimal(_optDecimalProperties);
            decoder.PrepareDecodeOptionalEnumeration(_optEnumerationProperties);
            decoder.PrepareDecodeOptionalFloat32(_optFloat32Properties);
            decoder.PrepareDecodeOptionalFloat64(_optFloat64Properties);
            decoder.PrepareDecodeOptionalInt16(_optInt16Properties);
            decoder.PrepareDecodeOptionalInt32(_optInt32Properties);
            decoder.PrepareDecodeOptionalInt64(_optInt64Properties);
            decoder.PrepareDecodeOptionalInt8(_optInt8Properties);
            
            decoder.PrepareDecodeOptionalString(_optStringProperties);
            decoder.PrepareDecodeOptionalStructure(_optStructureProperties);
            decoder.PrepareDecodeRequiredBool(_reqBoolProperties);
            decoder.PrepareDecodeRequiredBytes(_reqBytesProperties);
            decoder.PrepareDecodeRequiredDecimal(_reqDecimalProperties);
            decoder.PrepareDecodeRequiredEnumeration(_reqEnumerationProperties);
            decoder.PrepareDecodeRequiredFloat32(_reqFloat32Properties);
            decoder.PrepareDecodeRequiredFloat64(_reqFloat64Properties);
            decoder.PrepareDecodeRequiredInt16(_reqInt16Properties);
            decoder.PrepareDecodeRequiredInt32(_reqInt32Properties);
            decoder.PrepareDecodeRequiredInt64(_reqInt64Properties);
            decoder.PrepareDecodeRequiredInt8(_reqInt8Properties);
            
            decoder.PrepareDecodeRequiredString(_reqStringProperties);
            decoder.PrepareDecodeRequiredStructure(_reqStructureProperties);
            
            decoder.PrepareContainer();
            
            _optBool = (bool?)decoder.DecodeOptionalBool(_optBoolProperties);
            _optBytes = (byte[])decoder.DecodeOptionalBytes(_optBytesProperties);
            _optDecimal = (decimal?)decoder.DecodeOptionalDecimal(_optDecimalProperties);
            _optEnumeration = (TypesEnumeration?)decoder.DecodeOptionalEnumeration(_optEnumerationProperties);
            _optFloat32 = (float?)decoder.DecodeOptionalFloat32(_optFloat32Properties);
            _optFloat64 = (double?)decoder.DecodeOptionalFloat64(_optFloat64Properties);
            _optInt16 = (short?)decoder.DecodeOptionalInt16(_optInt16Properties);
            _optInt32 = (int?)decoder.DecodeOptionalInt32(_optInt32Properties);
            _optInt64 = (long?)decoder.DecodeOptionalInt64(_optInt64Properties);
            _optInt8 = (byte?)decoder.DecodeOptionalInt8(_optInt8Properties);
            int optListOfEnumCount = decoder.OpenCountedContainer();
            
            for (int i = 0; i < optListOfEnumCount; i++)
            {
                decoder.PrepareDecodeOptionalStructure(_optListOfEnumProperties);
            }
            
            decoder.PrepareContainer();
            
            _optListOfEnum = new List<SmallStructure>();
            
            for (int i = 0; i < optListOfEnumCount; i++)
            {
                _optListOfEnum.Add((SmallStructure)decoder.DecodeOptionalStructure(SmallStructureFactory.Instance, _optListOfEnumProperties));
            }
            
            decoder.CloseContainer();
            _optString = (string)decoder.DecodeOptionalString(_optStringProperties);
            _optStructure = (SmallStructure)decoder.DecodeOptionalStructure(SmallStructureFactory.Instance, _optStructureProperties);
            _reqBool = (bool)decoder.DecodeRequiredBool(_reqBoolProperties);
            _reqBytes = (byte[])decoder.DecodeRequiredBytes(_reqBytesProperties);
            _reqDecimal = (decimal)decoder.DecodeRequiredDecimal(_reqDecimalProperties);
            _reqEnumeration = (TypesEnumeration)decoder.DecodeRequiredEnumeration(_reqEnumerationProperties);
            _reqFloat32 = (float)decoder.DecodeRequiredFloat32(_reqFloat32Properties);
            _reqFloat64 = (double)decoder.DecodeRequiredFloat64(_reqFloat64Properties);
            _reqInt16 = (short)decoder.DecodeRequiredInt16(_reqInt16Properties);
            _reqInt32 = (int)decoder.DecodeRequiredInt32(_reqInt32Properties);
            _reqInt64 = (long)decoder.DecodeRequiredInt64(_reqInt64Properties);
            _reqInt8 = (byte)decoder.DecodeRequiredInt8(_reqInt8Properties);
            int reqListOfEnumCount = decoder.OpenCountedContainer();
            
            for (int i = 0; i < reqListOfEnumCount; i++)
            {
                decoder.PrepareDecodeRequiredStructure(_reqListOfEnumProperties);
            }
            
            decoder.PrepareContainer();
            
            _reqListOfEnum = new List<SmallStructure>();
            
            for (int i = 0; i < reqListOfEnumCount; i++)
            {
                _reqListOfEnum.Add((SmallStructure)decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _reqListOfEnumProperties));
            }
            
            decoder.CloseContainer();
            _reqString = (string)decoder.DecodeRequiredString(_reqStringProperties);
            _reqStructure = (SmallStructure)decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _reqStructureProperties);
            
            decoder.CloseContainer();
        }
        
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }


}
