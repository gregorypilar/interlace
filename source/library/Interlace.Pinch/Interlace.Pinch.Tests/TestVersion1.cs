using System.Collections.Generic;
using System.ComponentModel;

using Interlace.Pinch.Implementation;

namespace Interlace.Pinch.TestsVersion1
{
    public enum Enumeration
    {
        A = 1,
        C = 2
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
                return 1;
            }
        }
        
        protected virtual void OnAdditionalFutureFields(IPinchDecoder decoder)
        {
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenSequence(2);
            
            // Encode fields for version 1:
            encoder.EncodeRequiredStructure(_choice, _choiceProperties);
            encoder.EncodeRequiredInt32(_test, _testProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 2)
            {
                _choice = (ChoiceStructure)decoder.DecodeRequiredStructure(ChoiceStructureFactory.Instance, _choiceProperties);
                _test = (int)decoder.DecodeRequiredInt32(_testProperties);
            
                remainingFields -= 2;
            }
            else
            {
                if (remainingFields != 0) throw new PinchInvalidCodingException();
            }
            
            if (remainingFields > 0) 
            {
                OnAdditionalFutureFields(decoder);
                
                decoder.SkipFields(remainingFields);
            }
            
            decoder.CloseSequence();
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
                return 1;
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
            encoder.EncodeChoiceMarker((int)_valueKind);
            
            switch (_valueKind)
            {
                case ChoiceStructureKind.None:
                    throw new PinchNullRequiredFieldException();
                    
                case ChoiceStructureKind.Small:
                    encoder.EncodeRequiredStructure((IPinchable)_value, _smallProperties);
                    break; 
                    
                case ChoiceStructureKind.RequiredDecimal:
                    encoder.EncodeRequiredStructure((IPinchable)_value, _requiredDecimalProperties);
                    break; 
                    
                case ChoiceStructureKind.OptionalDecimal:
                    encoder.EncodeRequiredStructure((IPinchable)_value, _optionalDecimalProperties);
                    break; 
                    
                case ChoiceStructureKind.Versioning:
                    encoder.EncodeRequiredStructure((IPinchable)_value, _versioningProperties);
                    break; 
                    
            }
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            _valueKind = (ChoiceStructureKind)decoder.DecodeChoiceMarker();
            
            switch (_valueKind)
            {
                case ChoiceStructureKind.Small:
                    _value = decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _smallProperties);
                    break; 
                    
                case ChoiceStructureKind.RequiredDecimal:
                    _value = decoder.DecodeRequiredStructure(RequiredDecimalStructureFactory.Instance, _requiredDecimalProperties);
                    break; 
                    
                case ChoiceStructureKind.OptionalDecimal:
                    _value = decoder.DecodeRequiredStructure(OptionalDecimalStructureFactory.Instance, _optionalDecimalProperties);
                    break; 
                    
                case ChoiceStructureKind.Versioning:
                    _value = decoder.DecodeRequiredStructure(VersioningStructureFactory.Instance, _versioningProperties);
                    break; 
                    
                default:
                    throw new PinchInvalidCodingException();
            }
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
                return 1;
            }
        }
        
        protected virtual void OnAdditionalFutureFields(IPinchDecoder decoder)
        {
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenSequence(1);
            
            // Encode fields for version 1:
            encoder.EncodeRequiredInt8(_test, _testProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 1)
            {
                _test = (byte)decoder.DecodeRequiredInt8(_testProperties);
            
                remainingFields -= 1;
            }
            else
            {
                if (remainingFields != 0) throw new PinchInvalidCodingException();
            }
            
            if (remainingFields > 0) 
            {
                OnAdditionalFutureFields(decoder);
                
                decoder.SkipFields(remainingFields);
            }
            
            decoder.CloseSequence();
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
                return 1;
            }
        }
        
        protected virtual void OnAdditionalFutureFields(IPinchDecoder decoder)
        {
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenSequence(1);
            
            // Encode fields for version 1:
            encoder.EncodeRequiredDecimal(_value, _valueProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 1)
            {
                _value = (decimal)decoder.DecodeRequiredDecimal(_valueProperties);
            
                remainingFields -= 1;
            }
            else
            {
                if (remainingFields != 0) throw new PinchInvalidCodingException();
            }
            
            if (remainingFields > 0) 
            {
                OnAdditionalFutureFields(decoder);
                
                decoder.SkipFields(remainingFields);
            }
            
            decoder.CloseSequence();
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
                return 1;
            }
        }
        
        protected virtual void OnAdditionalFutureFields(IPinchDecoder decoder)
        {
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenSequence(1);
            
            // Encode fields for version 1:
            encoder.EncodeOptionalDecimal(_value, _valueProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 1)
            {
                _value = (decimal?)decoder.DecodeOptionalDecimal(_valueProperties);
            
                remainingFields -= 1;
            }
            else
            {
                if (remainingFields != 0) throw new PinchInvalidCodingException();
            }
            
            if (remainingFields > 0) 
            {
                OnAdditionalFutureFields(decoder);
                
                decoder.SkipFields(remainingFields);
            }
            
            decoder.CloseSequence();
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
        byte? _removedOptScalar; 
        string _removedOptPointer; 
        SmallStructure _removedOptStructure; 

        static PinchFieldProperties _reqScalarProperties = new PinchFieldProperties(8, 1, null); 
        static PinchFieldProperties _reqPointerProperties = new PinchFieldProperties(7, 1, null); 
        static PinchFieldProperties _reqStructureProperties = new PinchFieldProperties(9, 1, null); 
        static PinchFieldProperties _optScalarProperties = new PinchFieldProperties(2, 1, null); 
        static PinchFieldProperties _optPointerProperties = new PinchFieldProperties(1, 1, null); 
        static PinchFieldProperties _optStructureProperties = new PinchFieldProperties(3, 1, null); 
        static PinchFieldProperties _removedOptScalarProperties = new PinchFieldProperties(5, 1, null); 
        static PinchFieldProperties _removedOptPointerProperties = new PinchFieldProperties(4, 1, null); 
        static PinchFieldProperties _removedOptStructureProperties = new PinchFieldProperties(6, 1, null); 
        
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
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 1;
            }
        }
        
        protected virtual void OnAdditionalFutureFields(IPinchDecoder decoder)
        {
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenSequence(9);
            
            // Encode fields for version 1:
            encoder.EncodeOptionalString(_optPointer, _optPointerProperties);
            encoder.EncodeOptionalInt8(_optScalar, _optScalarProperties);
            encoder.EncodeOptionalStructure(_optStructure, _optStructureProperties);
            encoder.EncodeOptionalString(_removedOptPointer, _removedOptPointerProperties);
            encoder.EncodeOptionalInt8(_removedOptScalar, _removedOptScalarProperties);
            encoder.EncodeOptionalStructure(_removedOptStructure, _removedOptStructureProperties);
            encoder.EncodeRequiredString(_reqPointer, _reqPointerProperties);
            encoder.EncodeRequiredInt8(_reqScalar, _reqScalarProperties);
            encoder.EncodeRequiredStructure(_reqStructure, _reqStructureProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 9)
            {
                _optPointer = (string)decoder.DecodeOptionalString(_optPointerProperties);
                _optScalar = (byte?)decoder.DecodeOptionalInt8(_optScalarProperties);
                _optStructure = (SmallStructure)decoder.DecodeOptionalStructure(SmallStructureFactory.Instance, _optStructureProperties);
                _removedOptPointer = (string)decoder.DecodeOptionalString(_removedOptPointerProperties);
                _removedOptScalar = (byte?)decoder.DecodeOptionalInt8(_removedOptScalarProperties);
                _removedOptStructure = (SmallStructure)decoder.DecodeOptionalStructure(SmallStructureFactory.Instance, _removedOptStructureProperties);
                _reqPointer = (string)decoder.DecodeRequiredString(_reqPointerProperties);
                _reqScalar = (byte)decoder.DecodeRequiredInt8(_reqScalarProperties);
                _reqStructure = (SmallStructure)decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _reqStructureProperties);
            
                remainingFields -= 9;
            }
            else
            {
                if (remainingFields != 0) throw new PinchInvalidCodingException();
            }
            
            if (remainingFields > 0) 
            {
                OnAdditionalFutureFields(decoder);
                
                decoder.SkipFields(remainingFields);
            }
            
            decoder.CloseSequence();
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
        A = 1
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
                return 1;
            }
        }
        
        protected virtual void OnAdditionalFutureFields(IPinchDecoder decoder)
        {
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenSequence(26);
            
            // Encode fields for version 1:
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
            
            encoder.OpenSequence(_optListOfEnum.Count);
            
            foreach (SmallStructure value in _optListOfEnum)
            {
            	encoder.EncodeOptionalStructure(value, _optListOfEnumProperties);
            }
            
            encoder.CloseSequence();
            
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
            
            encoder.OpenSequence(_reqListOfEnum.Count);
            
            foreach (SmallStructure value in _reqListOfEnum)
            {
            	encoder.EncodeRequiredStructure(value, _reqListOfEnumProperties);
            }
            
            encoder.CloseSequence();
            
            encoder.EncodeRequiredString(_reqString, _reqStringProperties);
            encoder.EncodeRequiredStructure(_reqStructure, _reqStructureProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 26)
            {
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
                int optListOfEnumCount = decoder.OpenSequence();
                
                _optListOfEnum = new List<SmallStructure>();
                
                for (int i = 0; i < optListOfEnumCount; i++)
                {
                    _optListOfEnum.Add((SmallStructure)decoder.DecodeOptionalStructure(SmallStructureFactory.Instance, _optListOfEnumProperties));
                }
                
                decoder.CloseSequence();
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
                int reqListOfEnumCount = decoder.OpenSequence();
                
                _reqListOfEnum = new List<SmallStructure>();
                
                for (int i = 0; i < reqListOfEnumCount; i++)
                {
                    _reqListOfEnum.Add((SmallStructure)decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _reqListOfEnumProperties));
                }
                
                decoder.CloseSequence();
                _reqString = (string)decoder.DecodeRequiredString(_reqStringProperties);
                _reqStructure = (SmallStructure)decoder.DecodeRequiredStructure(SmallStructureFactory.Instance, _reqStructureProperties);
            
                remainingFields -= 26;
            }
            else
            {
                if (remainingFields != 0) throw new PinchInvalidCodingException();
            }
            
            if (remainingFields > 0) 
            {
                OnAdditionalFutureFields(decoder);
                
                decoder.SkipFields(remainingFields);
            }
            
            decoder.CloseSequence();
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
