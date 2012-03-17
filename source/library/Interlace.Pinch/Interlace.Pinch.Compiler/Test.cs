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
        
        Fixes = 2, 
        Exception = 1, 
    }
    
    public partial class Content : IPinchable, INotifyPropertyChanged
    {
        object _value;
        ContentKind _valueKind;
        
        static PinchFieldProperties _fixesProperties = new PinchFieldProperties(2, 1, null); 
        static PinchFieldProperties _exceptionProperties = new PinchFieldProperties(1, 1, null); 
        
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

    public class FixesFactory : IPinchableFactory
    {
        static FixesFactory _instance = new FixesFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new Fixes(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class Fixes : IPinchable, INotifyPropertyChanged
    {
        List<Fix> _fix; 

        static PinchFieldProperties _fixProperties = new PinchFieldProperties(1, 1, null); 
        
        public Fixes()
        {
            _fix = new List<Fix>();
        }
    
        public Fixes(IPinchDecodingContext context)
        {
        }
        
        public List<Fix> Fix
        {
            get { return _fix; }
            set 
            { 
                _fix = value; 
                
                FirePropertyChanged("Fix");
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
            
            encoder.OpenSequence(_fix.Count);
            
            foreach (Fix value in _fix)
            {
            	encoder.EncodeRequiredStructure(value, _fixProperties);
            }
            
            encoder.CloseSequence();

            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 1)
            {
                int fixCount = decoder.OpenSequence();
                
                _fix = new List<Fix>();
                
                for (int i = 0; i < fixCount; i++)
                {
                    _fix.Add((Fix)decoder.DecodeRequiredStructure(FixFactory.Instance, _fixProperties));
                }
                
                decoder.CloseSequence();
            
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

    public class TimestampSurrogateFactory : IPinchableFactory
    {
        static TimestampSurrogateFactory _instance = new TimestampSurrogateFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new TimestampSurrogate(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class TimestampSurrogate : IPinchable, INotifyPropertyChanged
    {
        long _ticks; 

        static PinchFieldProperties _ticksProperties = new PinchFieldProperties(1, 1, null); 
        
        public TimestampSurrogate()
        {
        }
    
        public TimestampSurrogate(IPinchDecodingContext context)
        {
        }
        
        public long Ticks
        {
            get { return _ticks; }
            set 
            { 
                _ticks = value; 
                
                FirePropertyChanged("Ticks");
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
            encoder.EncodeRequiredInt64(_ticks, _ticksProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 1)
            {
                _ticks = (long)decoder.DecodeRequiredInt64(_ticksProperties);
            
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

		#region Surrogate Methods
		
		public static TimestampSurrogate ValueToSurrogateOptional(System.DateTime? value)
		{
			if (value != null)
			{
				TimestampSurrogate surrogate = TimestampSurrogate.ValueToSurrogate((System.DateTime)value);

				if (surrogate == null) throw new PinchNullRequiredFieldException();

				return surrogate;
			}
			else
			{
				return null;
			}
		}

		public static TimestampSurrogate ValueToSurrogateRequired(System.DateTime value)
		{
			TimestampSurrogate surrogate = TimestampSurrogate.ValueToSurrogate(value);

			if (surrogate == null) throw new PinchNullRequiredFieldException();

			return surrogate;
		}
		
		public static System.DateTime? SurrogateToValueOptional(TimestampSurrogate surrogate)
		{
			if (surrogate != null)
			{
				return TimestampSurrogate.SurrogateToValue(surrogate);
			}
			else
			{
				return null;
			}
		}

		public static System.DateTime SurrogateToValueRequired(TimestampSurrogate surrogate)
		{
			if (surrogate != null)
			{
				return TimestampSurrogate.SurrogateToValue(surrogate);
			}
			else
			{
				throw new PinchNullRequiredFieldException();
			}
		}
		#endregion
    }

    public class FixFactory : IPinchableFactory
    {
        static FixFactory _instance = new FixFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new Fix(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class Fix : IPinchable, INotifyPropertyChanged
    {
        System.DateTime _when; 
        System.DateTime? _received; 
        List<System.DateTime?> _receivedTimes; 
        float _latitude; 
        float _longitude; 

        static PinchFieldProperties _whenProperties = new PinchFieldProperties(4, 1, null); 
        static PinchFieldProperties _receivedProperties = new PinchFieldProperties(2, 1, null); 
        static PinchFieldProperties _receivedTimesProperties = new PinchFieldProperties(3, 1, null); 
        static PinchFieldProperties _latitudeProperties = new PinchFieldProperties(7, 2, null); 
        static PinchFieldProperties _longitudeProperties = new PinchFieldProperties(8, 2, null); 
        static PinchFieldProperties _xProperties = new PinchFieldProperties(5, 1, 2); 
        static PinchFieldProperties _yProperties = new PinchFieldProperties(6, 1, 2); 
        static PinchFieldProperties _hDOPProperties = new PinchFieldProperties(1, 1, 2); 
        
        public Fix()
        {
            
            
            _receivedTimes = new List<System.DateTime?>();
            

        }
    
        public Fix(IPinchDecodingContext context)
        {
        }
        
        public System.DateTime When
        {
            get { return _when; }
            set 
            { 
                _when = value; 
                
                FirePropertyChanged("When");
            }
        }
        
        public System.DateTime? Received
        {
            get { return _received; }
            set 
            { 
                _received = value; 
                
                FirePropertyChanged("Received");
            }
        }
        
        public List<System.DateTime?> ReceivedTimes
        {
            get { return _receivedTimes; }
            set 
            { 
                _receivedTimes = value; 
                
                FirePropertyChanged("ReceivedTimes");
            }
        }
        
        public float Latitude
        {
            get { return _latitude; }
            set 
            { 
                _latitude = value; 
                
                FirePropertyChanged("Latitude");
            }
        }
        
        public float Longitude
        {
            get { return _longitude; }
            set 
            { 
                _longitude = value; 
                
                FirePropertyChanged("Longitude");
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
            encoder.OpenSequence(8);
            
            // Encode fields for version 1:
            encoder.EncodeRemoved();
            encoder.EncodeOptionalStructure(TimestampSurrogate.ValueToSurrogateOptional(_received), _receivedProperties);
            
            encoder.OpenSequence(_receivedTimes.Count);
            
            foreach (System.DateTime? value in _receivedTimes)
            {
            	encoder.EncodeOptionalStructure(TimestampSurrogate.ValueToSurrogateOptional(value), _receivedTimesProperties);
            }
            
            encoder.CloseSequence();
            
            encoder.EncodeRequiredStructure(TimestampSurrogate.ValueToSurrogateRequired(_when), _whenProperties);
            encoder.EncodeRemoved();
            encoder.EncodeRemoved();
            
            // Encode fields for version 2:
            encoder.EncodeRequiredFloat32(_latitude, _latitudeProperties);
            encoder.EncodeRequiredFloat32(_longitude, _longitudeProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 6)
            {
                decoder.SkipRemoved();
                _received = TimestampSurrogate.SurrogateToValueOptional((TimestampSurrogate)decoder.DecodeOptionalStructure(TimestampSurrogateFactory.Instance, _receivedProperties));
                int receivedTimesCount = decoder.OpenSequence();
                
                _receivedTimes = new List<System.DateTime?>();
                
                for (int i = 0; i < receivedTimesCount; i++)
                {
                    _receivedTimes.Add(TimestampSurrogate.SurrogateToValueOptional((TimestampSurrogate)decoder.DecodeOptionalStructure(TimestampSurrogateFactory.Instance, _receivedTimesProperties)));
                }
                
                decoder.CloseSequence();
                _when = TimestampSurrogate.SurrogateToValueRequired((TimestampSurrogate)decoder.DecodeRequiredStructure(TimestampSurrogateFactory.Instance, _whenProperties));
                decoder.SkipRemoved();
                decoder.SkipRemoved();
            
                remainingFields -= 6;
            }
            else
            {
                if (remainingFields != 0) throw new PinchInvalidCodingException();
            }
            
            // Decode members for version 2:
            if (remainingFields >= 2)
            {
                _latitude = (float)decoder.DecodeRequiredFloat32(_latitudeProperties);
                _longitude = (float)decoder.DecodeRequiredFloat32(_longitudeProperties);
            
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


}
