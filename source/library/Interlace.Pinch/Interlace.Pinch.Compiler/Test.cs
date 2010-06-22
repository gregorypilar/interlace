using System;
using System.Collections.Generic;
using System.ComponentModel;

using Interlace.Pinch.Implementation;

namespace Talcasoft.Pinch.Test
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
        
        p