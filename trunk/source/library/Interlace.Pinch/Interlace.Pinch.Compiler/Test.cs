using System.Collections.Generic;
using System.ComponentModel;

using Interlace.Pinch.Implementation;

namespace Interlace.Pinch.Test
{
    public enum FixQuality
    {
        None = 3,
        Bad = 1,
        Good = 2,
        Excellent = 4
    }
    
    public class MessageFactory : IPinchableFactory
    {
        static MessageFactory _instance = new MessageFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new Message(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class Message : IPinchable, INotifyPropertyChanged
    {
        Content _content; 

        static PinchFieldProperties _contentProperties = new PinchFieldProperties(1, 1, null); 
        
        public Message()
        {
        }
    
        public Message(IPinchDecodingContext context)
        {
        }
        
        public Content Content
        {
            get { return _content; }
            set 
            { 
                _content = value; 
                
                FirePropertyChanged("Content");
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 2;
            }
        }
        
        protected virtual void OnAdditionalFutureFields(IPinchDecoder decoder)
        {
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenSequence(1);
            
            // Encode fields for version 1:
            encoder.EncodeRequiredStructure(_content, _contentProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 1)
            {
                _content = (Content)decoder.DecodeRequiredStructure(ContentFactory.Instance, _contentProperties);
            
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

    public class ContentFactory : IPinchableFactory
    {
        static ContentFactory _instance = new ContentFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new Content(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public enum ContentKind
    {
        None = 0,
        
        Fixes = 3, 
        Exception = 1, 
        Exception2 = 2, 
    }
    
    public partial class Content : IPinchable, INotifyPropertyChanged
    {
        object _value;
        ContentKind _valueKind;
        
        static PinchFieldProperties _fixesProperties = new PinchFieldProperties(3, 1, null); 
        static PinchFieldProperties _exceptionProperties = new PinchFieldProperties(1, 1, null); 
        static PinchFieldProperties _exception2Properties = new PinchFieldProperties(2, 1, null); 
        
        public Content(Fixes value)
        {
            _value = value;
            _valueKind = ContentKind.Fixes;
        }
        
        public static implicit operator Content(Fixes value)
        {
            return new Content(value);
        }
        
        
        public Content(Exception value)
        {
            _value = value;
            _valueKind = ContentKind.Exception;
        }
        
        public static implicit operator Content(Exception value)
        {
            return new Content(value);
        }
        
        
        public Content(Exception value)
        {
            _value = value;
            _valueKind = ContentKind.Exception2;
        }
        
        public static implicit operator Content(Exception value)
        {
            return new Content(value);
        }
        

    
        public Content()
        {
            _value = null;
            _valueKind = ContentKind.None;
        }
    
        public Content(IPinchDecodingContext context)
        {
        }
        
        public object Value 
        {
            get { return _value; }
        }
        
        public ContentKind ValueKind
        {
            get { return _valueKind; }
        }
        
        public Fixes Fixes
        {
            get { return _valueKind == ContentKind.Fixes ? (Fixes)_value : null; }
            set 
            { 
                ContentKind existingKind = _valueKind;
                
                _value = value; 
                _valueKind = ContentKind.Fixes;
                
                if (existingKind != _valueKind) FirePropertyChanged(existingKind);
                FirePropertyChanged(_valueKind);
            }
        }
        
        public Exception Exception
        {
            get { return _valueKind == ContentKind.Exception ? (Exception)_value : null; }
            set 
            { 
                ContentKind existingKind = _valueKind;
                
                _value = value; 
                _valueKind = ContentKind.Exception;
                
                if (existingKind != _valueKind) FirePropertyChanged(existingKind);
                FirePropertyChanged(_valueKind);
            }
        }
        
        public Exception Exception2
        {
            get { return _valueKind == ContentKind.Exception2 ? (Exception)_value : null; }
            set 
            { 
                ContentKind existingKind = _valueKind;
                
                _value = value; 
                _valueKind = ContentKind.Exception2;
                
                if (existingKind != _valueKind) FirePropertyChanged(existingKind);
                FirePropertyChanged(_valueKind);
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 2;
            }
        }
        
        public void FirePropertyChanged(ContentKind kind)
        {
            switch (kind)
            {
                case ContentKind.None:
                    break;
                    
                case ContentKind.Fixes:
                    FirePropertyChanged("Fixes");
                    break; 
                    
                case ContentKind.Exception:
                    FirePropertyChanged("Exception");
                    break; 
                    
                case ContentKind.Exception2:
                    FirePropertyChanged("Exception2");
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
                case ContentKind.None:
                    throw new PinchNullRequiredFieldException();
                    
                case ContentKind.Fixes:
                    encoder.EncodeRequiredStructure((IPinchable)_value, _fixesProperties);
                    break; 
                    
                case ContentKind.Exception:
                    encoder.EncodeRequiredStructure((IPinchable)_value, _exceptionProperties);
                    break; 
                    
                case ContentKind.Exception2:
                    encoder.EncodeRequiredStructure((IPinchable)_value, _exception2Properties);
                    break; 
                    
            }
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            _valueKind = (ContentKind)decoder.DecodeChoiceMarker();
            
            switch (_valueKind)
            {
                case ContentKind.Fixes:
                    _value = decoder.DecodeRequiredStructure(FixesFactory.Instance, _fixesProperties);
                    break; 
                    
                case ContentKind.Exception:
                    _value = decoder.DecodeRequiredStructure(ExceptionFactory.Instance, _exceptionProperties);
                    break; 
                    
                case ContentKind.Exception2:
                    _value = decoder.DecodeRequiredStructure(ExceptionFactory.Instance, _exception2Properties);
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

    public class ExceptionFactory : IPinchableFactory
    {
        static ExceptionFactory _instance = new ExceptionFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new Exception(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class Exception : IPinchable, INotifyPropertyChanged
    {
        string _message; 

        static PinchFieldProperties _messageProperties = new PinchFieldProperties(1, 1, null); 
        
        public Exception()
        {
        }
    
        public Exception(IPinchDecodingContext context)
        {
        }
        
        public string Message
        {
            get { return _message; }
            set 
            { 
                _message = value; 
                
                FirePropertyChanged("Message");
            }
        }
        
        int IPinchable.ProtocolVersion
        {
            get 
            {
                return 2;
            }
        }
        
        protected virtual void OnAdditionalFutureFields(IPinchDecoder decoder)
        {
        }
    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.OpenSequence(1);
            
            // Encode fields for version 1:
            encoder.EncodeRequiredString(_message, _messageProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 1)
            {
                _message = (string)decoder.DecodeRequiredString(_messageProperties);
            
                remainingFields -= 1;
            }
            else
            {
                if (remainingFields != 0) throw new PinchInvalidCodingException();
            }
            
            if (remainingFields > 0) 
            {
                OnAdditionalFutur