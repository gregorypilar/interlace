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

using Interlace.Binding;
using Interlace.Binding.Views;

#endregion

namespace Interlace.Binding.ViewConverters
{
    class BasicAutoConverterFactory : IAutoConverterFactory
    {
        public bool CanProvideConverter(BinderViewBase view, BinderHint hint)
        {
            return hint == BinderHint.Money || hint == BinderHint.Weight || 
                hint == BinderHint.Length || hint == BinderHint.NaturalNumber ||
                hint == BinderHint.Year || hint == BinderHint.Percentage ||
                hint == BinderHint.MinutesDuration || hint == BinderHint.HoursAndMinutesDuration ||
                hint == BinderHint.MoneyString;
        }

        public ViewConverterBase ProvideConverter(BinderViewBase view, BinderHint hint)
        {
            if (hint == BinderHint.Money)
                return new ConvertConverter(typeof(decimal));
                
            if (hint == BinderHint.Weight || hint == BinderHint.Length ||
                hint == BinderHint.Percentage || hint == BinderHint.MinutesDuration ||
                hint == BinderHint.HoursAndMinutesDuration)
                return new ConvertConverter(typeof(double));

            if (hint == BinderHint.NaturalNumber || hint == BinderHint.Year)
                return new ConvertConverter(typeof(int));

            if (hint == BinderHint.MoneyString)
                return new FormatConverter("${0:0.00}");

            throw new InvalidOperationException();
        }
    }
}
