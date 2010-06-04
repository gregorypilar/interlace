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

#endregion

namespace Interlace.Pinch.Test
{
    public enum FixQuality
    {
        None,
        Bad,
        Good,
        Excellent,
        
    }
    
    public class Message
    {
        Content _content;

        public Content Content
        {
            get { return _content; }
            set { _content = value; }
        }
    

    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.PrepareEncodeRequiredStructure(_content);

            
            encoder.BeginEncoding();
            
            encoder.EncodeRequiredStructure(_content);

        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            encoder.PrepareDecodeRequiredStructure();

            
            encoder.BeginDecoding();
            
            encoder.DecodeRequiredStructure(out _content);

        }
    }

    public class Content
    {
        List<Fix> _fix;

        public List<Fix> Fix
        {
            get { return _fix; }
            set { _fix = value; }
        }
    

    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.PrepareEncodeRequiredStructure(_fix);

            
            encoder.BeginEncoding();
            
            encoder.EncodeRequiredStructure(_fix);

        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            encoder.PrepareDecodeRequiredStructure();

            
            encoder.BeginDecoding();
            
            encoder.DecodeRequiredStructure(out _fix);

        }
    }

    public class Fix
    {
        float _latitude;
        float _longitude;
        float _x;
        float _y;
        float? _hDOP;

        public float Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }
    
        public float Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }
    
        public float X
        {
            get { return _x; }
            set { _x = value; }
        }
    
        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }
    
        public float? HDOP
        {
            get { return _hDOP; }
            set { _hDOP = value; }
        }
    

    
        void IPinchable.Encode(IPinchEncoder encoder)
        {
            encoder.PrepareEncodeRequiredFloat32(_latitude);
            encoder.PrepareEncodeRequiredFloat32(_longitude);
            encoder.PrepareEncodeRequiredFloat32(_x);
            encoder.PrepareEncodeRequiredFloat32(_y);
            encoder.PrepareEncodeOptionalFloat32(_hDOP);

            
            encoder.BeginEncoding();
            
            encoder.EncodeRequiredFloat32(_latitude);
            encoder.EncodeRequiredFloat32(_longitude);
            encoder.EncodeRequiredFloat32(_x);
            encoder.EncodeRequiredFloat32(_y);
            encoder.EncodeOptionalFloat32(_hDOP);

        }
        
        void IPinchable.Decode(IPinchDecoder decoder)
        {
            encoder.PrepareDecodeRequiredFloat32();
            encoder.PrepareDecodeRequiredFloat32();
            encoder.PrepareDecodeRequiredFloat32();
            encoder.PrepareDecodeRequiredFloat32();
            encoder.PrepareDecodeOptionalFloat32();

            
            encoder.BeginDecoding();
            
            encoder.DecodeRequiredFloat32(out _latitude);
            encoder.DecodeRequiredFloat32(out _longitude);
            encoder.DecodeRequiredFloat32(out _x);
            encoder.DecodeRequiredFloat32(out _y);
            encoder.DecodeOptionalFloat32(out _hDOP);

        }
    }


}
