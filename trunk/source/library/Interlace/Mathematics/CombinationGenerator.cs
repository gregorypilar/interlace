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

namespace Interlace.Mathematics
{
    /// <summary>
    /// Generates combinations in the specified range.
    /// The combination length will start from one then it will
    /// increase until reaches the passed specified range e.g. 
    /// if you pass 10 for the combination range, the combination length
    /// will first be one, when all the combinations are generated, the 
    /// combination length will be increased to two and so on.
    /// Also note that the range always starts from zero
    /// so if you pass 10 for the combination range,
    /// the range will be 0-10 (what is meant by the range here
    /// is the values inside the combinations array)
    /// </summary>
    public class CombinationGenerator
    {
        private readonly int _combinationRange;
        private int[] _currentCombination;

        public CombinationGenerator(int combinationRange)
        {
            // check
            if (combinationRange < 0)
            {
                throw new ApplicationException("Invalid value for the combination range parameter (value cannot be less than zero)");
            }

            _combinationRange = combinationRange;
        }

        /// <summary>
        /// Generates the combinations. When the last possible combination is generated null is returned.
        /// </summary>
        /// <returns></returns>
        public int[] GetNextCombination()
        {
            if (_currentCombination == null)
            {
                _currentCombination = new int[1];
                _currentCombination[0] = 0;
            }
            else
            {
                bool combinationGenerated = false;
                for (int i = _currentCombination.Length - 1; i >= 0; i--)
                {
                    if (_currentCombination[i] < _combinationRange - (_currentCombination.Length - i - 1))
                    {
                        _currentCombination[i]++;
                        for (int k = i + 1; k < _currentCombination.Length; k++)
                        {
                            _currentCombination[k] = _currentCombination[i] + (k - i);
                        }
                        combinationGenerated = true;
                        break;
                    }
                }

                if (!combinationGenerated)
                {
                    if (_currentCombination.Length >= _combinationRange + 1)
                    {
                        return null;
                    }
                    else
                    {
                        int newCombinationLength = _currentCombination.Length + 1;
                        _currentCombination = new int[newCombinationLength];
                        for (int i = 0; i < _currentCombination.Length; i++)
                        {
                            _currentCombination[i] = i;
                        }
                    }
                }
            }

            // Create a copy of the combination array to make sure the internal state of the class
            // won't be changed by the caller (Note that Array.Clone() creates a shallow copy of 
            // an array, i.e. if you have references in your array it will only copy the references 
            // and won't clone the objects they refer to. This is not a problem in this case as we 
            // have a value type array, int[], so we don't have references in our array)
            return (int[])_currentCombination.Clone();
        }

        public void Reset()
        {
            _currentCombination = null;
        }

        public IEnumerable<int[]> AllCombinations
        {
            get
            {
                int[] combination = GetNextCombination();

                while (combination != null)
                {
                    yield return combination;

                    combination = GetNextCombination();
                }
            }
        }
    }
}
