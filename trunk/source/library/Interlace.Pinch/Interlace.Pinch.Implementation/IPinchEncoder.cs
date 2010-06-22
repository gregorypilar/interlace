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

#endregion

namespace Interlace.Pinch.Implementation
{
    public interface IPinchEncoder
    {
        void OpenSequence(int count);

        void EncodeChoiceMarker(int valueKind);
        void EncodeRemoved();

        void EncodeRequiredFloat32(float value, PinchFieldProperties properties);
        void EncodeRequiredFloat64(double value, PinchFieldProperties properties);
        void EncodeRequiredInt8(byte value, PinchFieldProperties properties);
        void EncodeRequiredInt16(short value, PinchFieldProperties properties);
        void EncodeRequiredInt32(int value, PinchFieldProperties properties);
        void EncodeRequiredInt64(long value, PinchFieldProperties properties);
        void EncodeRequiredDecimal(decimal value, PinchFieldProperties properties);
        void EncodeRequiredBool(bool value, PinchFieldProperties properties);
        void EncodeRequiredString(string value, PinchFieldProperties properties);
        void EncodeRequiredBytes(byte[] value, PinchFieldProperties properties);
        void EncodeRequiredEnumeration(object value, PinchFieldProperties properties);
        void EncodeRequiredStructure(object value, PinchFieldProperties properties);
        void EncodeOptionalFloat32(float? value, PinchFieldProperties properties);
        void EncodeOptionalFloat64(double? value, PinchFieldProperties properties);
        void EncodeOptionalInt8(byte? value, PinchFieldProperties properties);
        void EncodeOptionalInt16(short? value, PinchFieldProperties properties);
        void EncodeOptionalInt32(int? value, PinchFieldProperties properties);
        void EncodeOptionalInt64(long? value, PinchFieldProperties properties);
        void EncodeOptionalDecimal(decimal? value, PinchFieldProperties properties);
        void EncodeOptionalBool(bool? value, PinchFieldProperties properties);
        void EncodeOptionalString(string value, PinchFieldProperties properties);
        void EncodeOptionalBytes(byte[] value, PinchFieldProperties properties);
        void EncodeOptionalEnumeration(object value, PinchFieldProperties properties);
        void EncodeOptionalStructure(object value, PinchFieldProperties properties);

        void CloseSequence();
    }
}
