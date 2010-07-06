using System.Collections.Generic;
using System.ComponentModel;

using Interlace.Pinch.Implementation;

namespace Interlace.Pinch.Test
{
    public class MyDateTimeSurrogateFactory : IPinchableFactory
    {
        static MyDateTimeSurrogateFactory _instance = new MyDateTimeSurrogateFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new MyDateTimeSurrogate(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class MyDateTimeSurrogate : IPinchable, INotifyPropertyChanged
    {
        long _ticks; 

        static PinchFieldProperties _ticksProperties = new PinchFieldProperties(1, 1, null); 
        
        public MyDateTimeSurrogate()
        {
        }
    
        public MyDateTimeSurrogate(IPinchDecodingContext context)
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
		
		public static MyDateTimeSurrogate ValueToSurrogateOptional(System.DateTime? value)
		{
			if (value != null)
			{
				MyDateTimeSurrogate surrogate = MyDateTimeSurrogate.ValueToSurrogate((System.DateTime)value);

				if (surrogate == null) throw new PinchNullRequiredFieldException();

				return surrogate;
			}
			else
			{
				return null;
			}
		}

		public static MyDateTimeSurrogate ValueToSurrogateRequired(System.DateTime value)
		{
			MyDateTimeSurrogate surrogate = MyDateTimeSurrogate.ValueToSurrogate(value);

			if (surrogate == null) throw new PinchNullRequiredFieldException();

			return surrogate;
		}
		
		public static System.DateTime? SurrogateToValueOptional(MyDateTimeSurrogate surrogate)
		{
			if (surrogate != null)
			{
				return MyDateTimeSurrogate.SurrogateToValue(surrogate);
			}
			else
			{
				return null;
			}
		}

		public static System.DateTime SurrogateToValueRequired(MyDateTimeSurrogate surrogate)
		{
			if (surrogate != null)
			{
				return MyDateTimeSurrogate.SurrogateToValue(surrogate);
			}
			else
			{
				throw new PinchNullRequiredFieldException();
			}
		}
		#endregion
    }

    public class MyUriSurrogateFactory : IPinchableFactory
    {
        static MyUriSurrogateFactory _instance = new MyUriSurrogateFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new MyUriSurrogate(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class MyUriSurrogate : IPinchable, INotifyPropertyChanged
    {
        string _address; 

        static PinchFieldProperties _addressProperties = new PinchFieldProperties(1, 1, null); 
        
        public MyUriSurrogate()
        {
        }
    
        public MyUriSurrogate(IPinchDecodingContext context)
        {
        }
        
        public string Address
        {
            get { return _address; }
            set 
            { 
                _address = value; 
                
                FirePropertyChanged("Address");
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
            encoder.EncodeOptionalString(_address, _addressProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 1)
            {
                _address = (string)decoder.DecodeOptionalString(_addressProperties);
            
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
		
		public static MyUriSurrogate ValueToSurrogateOptional(System.Uri value)
		{
			if (value != null)
			{
				MyUriSurrogate surrogate = MyUriSurrogate.ValueToSurrogate((System.Uri)value);

				if (surrogate == null) throw new PinchNullRequiredFieldException();

				return surrogate;
			}
			else
			{
				return null;
			}
		}

		public static MyUriSurrogate ValueToSurrogateRequired(System.Uri value)
		{
			MyUriSurrogate surrogate = MyUriSurrogate.ValueToSurrogate(value);

			if (surrogate == null) throw new PinchNullRequiredFieldException();

			return surrogate;
		}
		
		public static System.Uri SurrogateToValueOptional(MyUriSurrogate surrogate)
		{
			if (surrogate != null)
			{
				return MyUriSurrogate.SurrogateToValue(surrogate);
			}
			else
			{
				return null;
			}
		}

		public static System.Uri SurrogateToValueRequired(MyUriSurrogate surrogate)
		{
			if (surrogate != null)
			{
				return MyUriSurrogate.SurrogateToValue(surrogate);
			}
			else
			{
				throw new PinchNullRequiredFieldException();
			}
		}
		#endregion
    }

    public class BucketOfSurrogatesFactory : IPinchableFactory
    {
        static BucketOfSurrogatesFactory _instance = new BucketOfSurrogatesFactory();
        
        public object Create(IPinchDecodingContext context)
        {
            return new BucketOfSurrogates(context);
        }
        
        public static IPinchableFactory Instance
        {
            get
            {
                return _instance;
            }
        }
    }
    
    public partial class BucketOfSurrogates : IPinchable, INotifyPropertyChanged
    {
        System.DateTime _requiredDateTime; 
        System.DateTime? _optionalDateTime; 
        List<System.DateTime> _listOfRequiredDateTime; 
        List<System.DateTime?> _listOfOptionalDateTime; 
        System.Uri _requiredUri; 
        System.Uri _optionalUri; 
        List<System.Uri> _listOfRequiredUri; 
        List<System.Uri> _listOfOptionalUri; 

        static PinchFieldProperties _requiredDateTimeProperties = new PinchFieldProperties(7, 1, null); 
        static PinchFieldProperties _optionalDateTimeProperties = new PinchFieldProperties(5, 1, null); 
        static PinchFieldProperties _listOfRequiredDateTimeProperties = new PinchFieldProperties(3, 1, null); 
        static PinchFieldProperties _listOfOptionalDateTimeProperties = new PinchFieldProperties(1, 1, null); 
        static PinchFieldProperties _requiredUriProperties = new PinchFieldProperties(8, 1, null); 
        static PinchFieldProperties _optionalUriProperties = new PinchFieldProperties(6, 1, null); 
        static PinchFieldProperties _listOfRequiredUriProperties = new PinchFieldProperties(4, 1, null); 
        static PinchFieldProperties _listOfOptionalUriProperties = new PinchFieldProperties(2, 1, null); 
        
        public BucketOfSurrogates()
        {
            
            
            _listOfRequiredDateTime = new List<System.DateTime>();
            _listOfOptionalDateTime = new List<System.DateTime?>();
            
            
            _listOfRequiredUri = new List<System.Uri>();
            _listOfOptionalUri = new List<System.Uri>();
        }
    
        public BucketOfSurrogates(IPinchDecodingContext context)
        {
        }
        
        public System.DateTime RequiredDateTime
        {
            get { return _requiredDateTime; }
            set 
            { 
                _requiredDateTime = value; 
                
                FirePropertyChanged("RequiredDateTime");
            }
        }
        
        public System.DateTime? OptionalDateTime
        {
            get { return _optionalDateTime; }
            set 
            { 
                _optionalDateTime = value; 
                
                FirePropertyChanged("OptionalDateTime");
            }
        }
        
        public List<System.DateTime> ListOfRequiredDateTime
        {
            get { return _listOfRequiredDateTime; }
            set 
            { 
                _listOfRequiredDateTime = value; 
                
                FirePropertyChanged("ListOfRequiredDateTime");
            }
        }
        
        public List<System.DateTime?> ListOfOptionalDateTime
        {
            get { return _listOfOptionalDateTime; }
            set 
            { 
                _listOfOptionalDateTime = value; 
                
                FirePropertyChanged("ListOfOptionalDateTime");
            }
        }
        
        public System.Uri RequiredUri
        {
            get { return _requiredUri; }
            set 
            { 
                _requiredUri = value; 
                
                FirePropertyChanged("RequiredUri");
            }
        }
        
        public System.Uri OptionalUri
        {
            get { return _optionalUri; }
            set 
            { 
                _optionalUri = value; 
                
                FirePropertyChanged("OptionalUri");
            }
        }
        
        public List<System.Uri> ListOfRequiredUri
        {
            get { return _listOfRequiredUri; }
            set 
            { 
                _listOfRequiredUri = value; 
                
                FirePropertyChanged("ListOfRequiredUri");
            }
        }
        
        public List<System.Uri> ListOfOptionalUri
        {
            get { return _listOfOptionalUri; }
            set 
            { 
                _listOfOptionalUri = value; 
                
                FirePropertyChanged("ListOfOptionalUri");
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
            encoder.OpenSequence(8);
            
            // Encode fields for version 1:
            
            encoder.OpenSequence(_listOfOptionalDateTime.Count);
            
            foreach (System.DateTime? value in _listOfOptionalDateTime)
            {
            	encoder.EncodeOptionalStructure(MyDateTimeSurrogate.ValueToSurrogateOptional(value), _listOfOptionalDateTimeProperties);
            }
            
            encoder.CloseSequence();
            
            
            encoder.OpenSequence(_listOfOptionalUri.Count);
            
            foreach (System.Uri value in _listOfOptionalUri)
            {
            	encoder.EncodeOptionalStructure(MyUriSurrogate.ValueToSurrogateOptional(value), _listOfOptionalUriProperties);
            }
            
            encoder.CloseSequence();
            
            
            encoder.OpenSequence(_listOfRequiredDateTime.Count);
            
            foreach (System.DateTime value in _listOfRequiredDateTime)
            {
            	encoder.EncodeRequiredStructure(MyDateTimeSurrogate.ValueToSurrogateRequired(value), _listOfRequiredDateTimeProperties);
            }
            
            encoder.CloseSequence();
            
            
            encoder.OpenSequence(_listOfRequiredUri.Count);
            
            foreach (System.Uri value in _listOfRequiredUri)
            {
            	encoder.EncodeRequiredStructure(MyUriSurrogate.ValueToSurrogateRequired(value), _listOfRequiredUriProperties);
            }
            
            encoder.CloseSequence();
            
            encoder.EncodeOptionalStructure(MyDateTimeSurrogate.ValueToSurrogateOptional(_optionalDateTime), _optionalDateTimeProperties);
            encoder.EncodeOptionalStructure(MyUriSurrogate.ValueToSurrogateOptional(_optionalUri), _optionalUriProperties);
            encoder.EncodeRequiredStructure(MyDateTimeSurrogate.ValueToSurrogateRequired(_requiredDateTime), _requiredDateTimeProperties);
            encoder.EncodeRequiredStructure(MyUriSurrogate.ValueToSurrogateRequired(_requiredUri), _requiredUriProperties);
            
            encoder.CloseSequence();
        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            int remainingFields = decoder.OpenSequence();
            
            // Decode members for version 1:
            if (remainingFields >= 8)
            {
                int listOfOptionalDateTimeCount = decoder.OpenSequence();
                
                _listOfOptionalDateTime = new List<System.DateTime?>();
                
                for (int i = 0; i < listOfOptionalDateTimeCount; i++)
                {
                    _listOfOptionalDateTime.Add(MyDateTimeSurrogate.SurrogateToValueOptional((MyDateTimeSurrogate)decoder.DecodeOptionalStructure(MyDateTimeSurrogateFactory.Instance, _listOfOptionalDateTimeProperties)));
                }
                
                decoder.CloseSequence();
                int listOfOptionalUriCount = decoder.OpenSequence();
                
                _listOfOptionalUri = new List<System.Uri>();
                
                for (int i = 0; i < listOfOptionalUriCount; i++)
                {
                    _listOfOptionalUri.Add(MyUriSurrogate.SurrogateToValueOptional((MyUriSurrogate)decoder.DecodeOptionalStructure(MyUriSurrogateFactory.Instance, _listOfOptionalUriProperties)));
                }
                
                decoder.CloseSequence();
                int listOfRequiredDateTimeCount = decoder.OpenSequence();
                
                _listOfRequiredDateTime = new List<System.DateTime>();
                
                for (int i = 0; i < listOfRequiredDateTimeCount; i++)
                {
                    _listOfRequiredDateTime.Add(MyDateTimeSurrogate.SurrogateToValueRequired((MyDateTimeSurrogate)decoder.DecodeRequiredStructure(MyDateTimeSurrogateFactory.Instance, _listOfRequiredDateTimeProperties)));
                }
                
                decoder.CloseSequence();
                int listOfRequiredUriCount = decoder.OpenSequence();
                
                _listOfRequiredUri = new List<System.Uri>();
                
                for (int i = 0; i < listOfRequiredUriCount; i++)
                {
                    _listOfRequiredUri.Add(MyUriSurrogate.SurrogateToValueRequired((MyUriSurrogate)decoder.DecodeRequiredStructure(MyUriSurrogateFactory.Instance, _listOfRequiredUriProperties)));
                }
                
                decoder.CloseSequence();
                _optionalDateTime = MyDateTimeSurrogate.SurrogateToValueOptional((MyDateTimeSurrogate)decoder.DecodeOptionalStructure(MyDateTimeSurrogateFactory.Instance, _optionalDateTimeProperties));
                _optionalUri = MyUriSurrogate.SurrogateToValueOptional((MyUriSurrogate)decoder.DecodeOptionalStructure(MyUriSurrogateFactory.Instance, _optionalUriProperties));
                _requiredDateTime = MyDateTimeSurrogate.SurrogateToValueRequired((MyDateTimeSurrogate)decoder.DecodeRequiredStructure(MyDateTimeSurrogateFactory.Instance, _requiredDateTimeProperties));
                _requiredUri = MyUriSurrogate.SurrogateToValueRequired((MyUriSurrogate)decoder.DecodeRequiredStructure(MyUriSurrogateFactory.Instance, _requiredUriProperties));
            
                remainingFields -= 8;
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
