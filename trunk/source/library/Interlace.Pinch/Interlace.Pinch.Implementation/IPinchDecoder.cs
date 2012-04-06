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
    public interface IPinchDecoder
    {
        int OpenSequence();
        int? OpenOptionalSequence();

        int DecodeChoiceMarker();
        int? DecodeOptionalChoiceMarker();

        void SkipFields(int remainingFields);
        void SkipRemoved();

        float DecodeRequiredFloat32(PinchFieldProperties properties);
        double DecodeRequiredFloat64(PinchFieldProperties properties);
        byte DecodeRequiredInt8(PinchFieldProperties properties);
        short DecodeRequiredInt16(PinchFieldProperties properties);
        int DecodeRequiredInt32(PinchFieldProperties properties);
        long DecodeRequiredInt64(PinchFieldProperties properties);
        decimal DecodeRequiredDecimal(PinchFieldProperties properties);
        bool DecodeRequiredBool(PinchFieldProperties properties);
        string DecodeRequiredString(PinchFieldProperties properties);
        byte[] DecodeRequiredBytes(PinchFieldProperties properties);
        int DecodeRequiredEnumeration(PinchFieldProperties properties);
        object DecodeRequiredStructure(IPinchableFactory factory, PinchFieldProperties properties);
        float? DecodeOptionalFloat32(PinchFieldProperties properties);
        double? DecodeOptionalFloat64(PinchFieldProperties properties);
        byte? DecodeOptionalInt8(PinchFieldProperties properties);
        short? DecodeOptionalInt16(PinchFieldProperties properties);
        int? DecodeOptionalInt32(PinchFieldProperties properties);
        long? DecodeOptionalInt64(PinchFieldProperties properties);
        decimal? DecodeOptionalDecimal(PinchFieldProperties properties);
        bool? DecodeOptionalBool(PinchFieldProperties properties);
        string DecodeOptionalString(PinchFieldProperties properties);
        byte[] DecodeOptionalBytes(PinchFieldProperties properties);
        int? DecodeOptionalEnumeration(PinchFieldProperties properties);
        object DecodeOptionalStructure(IPinchableFactory factory, PinchFieldProperties properties);

        void CloseSequence();
    }
}
