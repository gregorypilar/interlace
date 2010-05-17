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

// This class is a conversion of portions of the Python 2.4 difflib class.
//
// PSF LICENSE AGREEMENT FOR PYTHON 2.4
// ------------------------------------
// 
// 1. This LICENSE AGREEMENT is between the Python Software Foundation
// ("PSF"), and the Individual or Organization ("Licensee") accessing and
// otherwise using Python 2.4 software in source or binary form and its
// associated documentation.
// 
// 2. Subject to the terms and conditions of this License Agreement, PSF
// hereby grants Licensee a nonexclusive, royalty-free, world-wide
// license to reproduce, analyze, test, perform and/or display publicly,
// prepare derivative works, distribute, and otherwise use Python 2.4
// alone or in any derivative version, provided, however, that PSF's
// License Agreement and PSF's notice of copyright, i.e., "Copyright (c)
// 2001, 2002, 2003, 2004 Python Software Foundation; All Rights Reserved"
// are retained in Python 2.4 alone or in any derivative version prepared
// by Licensee.
// 
// 3. In the event Licensee prepares a derivative work that is based on
// or incorporates Python 2.4 or any part thereof, and wants to make
// the derivative work available to others as provided herein, then
// Licensee hereby agrees to include in any such work a brief summary of
// the changes made to Python 2.4.
// 
// 4. PSF is making Python 2.4 available to Licensee on an "AS IS"
// basis.  PSF MAKES NO REPRESENTATIONS OR WARRANTIES, EXPRESS OR
// IMPLIED.  BY WAY OF EXAMPLE, BUT NOT LIMITATION, PSF MAKES NO AND
// DISCLAIMS ANY REPRESENTATION OR WARRANTY OF MERCHANTABILITY OR FITNESS
// FOR ANY PARTICULAR PURPOSE OR THAT THE USE OF PYTHON 2.4 WILL NOT
// INFRINGE ANY THIRD PARTY RIGHTS.
// 
// 5. PSF SHALL NOT BE LIABLE TO LICENSEE OR ANY OTHER USERS OF PYTHON
// 2.4 FOR ANY INCIDENTAL, SPECIAL, OR CONSEQUENTIAL DAMAGES OR LOSS AS
// A RESULT OF MODIFYING, DISTRIBUTING, OR OTHERWISE USING PYTHON 2.4,
// OR ANY DERIVATIVE THEREOF, EVEN IF ADVISED OF THE POSSIBILITY THEREOF.
// 
// 6. This License Agreement will automatically terminate upon a material
// breach of its terms and conditions.
// 
// 7. Nothing in this License Agreement shall be deemed to create any
// relationship of agency, partnership, or joint venture between PSF and
// Licensee.  This License Agreement does not grant permission to use PSF
// trademarks or trade name in a trademark sense to endorse or promote
// products or services of Licensee, or any third party.
// 
// 8. By copying, installing or otherwise using Python 2.4, Licensee
// agrees to be bound by the terms and conditions of this License
// Agreement.

namespace Interlace.Differencing
{
    public delegate bool IsJunk<T>(T element);

    /// <summary>
    /// SequenceMatcher is a flexible class for comparing pairs of sequences of
    /// any type, so long as the sequence elements are hashable.
    /// </summary>
    public class SequenceMatcher<T>
    {
        IsJunk<T> _isJunk;
        IList<T> _left;
        IList<T> _right;

        List<MatchingBlock> _matchingBlocks;
        List<OpCode> _opCodes;

        Dictionary<T, List<int>> _rightToIndices;
        Dictionary<T, bool> _popularElements;
        Dictionary<T, bool> _rightJunk;

        public SequenceMatcher()
            : this(IsNeverJunk)
        {
        }

        public SequenceMatcher(IsJunk<T> isJunk)
        {
            _isJunk = isJunk;
        }

        private static bool IsNeverJunk(T element)
        {
            return false;
        }

        public IList<T> Left
        {
            get { return _left; }
            set 
            {
                if (object.ReferenceEquals(_left, value)) return;

                _left = value;

                _matchingBlocks = null;
                _opCodes = null;
            }
        }

        public IList<T> Right
        {
            get { return _right; }
            set
            {
                if (object.ReferenceEquals(_right, value)) return;

                _right = value;

                _matchingBlocks = null;
                _opCodes = null;

                PrepareRightSideTables();
            }
        }

        void PrepareRightSideTables()
        {
            _rightToIndices = new Dictionary<T, List<int>>();
            _popularElements = new Dictionary<T, bool>();

            _rightJunk = new Dictionary<T, bool>();

            for (int i = 0; i < _right.Count; i++)
            {
                T element = _right[i];

                if (_rightToIndices.ContainsKey(element))
                {
                    List<int> indices = _rightToIndices[element];
                    
                    if (_right.Count >= 200 && indices.Count * 100 > _right.Count)
                    {
                        _popularElements[element] = true;
                        indices.Clear();
                    }
                    else
                    {
                        indices.Add(i);
                    }
                }
                else
                {
                    if (!_rightJunk.ContainsKey(element))
                    {
                        if (_isJunk(element))
                        {
                            _rightJunk[element] = true;
                        }
                        else
                        {
                            List<int> indices = new List<int>();
                            indices.Add(i);
                            _rightToIndices[element] = indices;
                        }
                    }
                }
            }

            // Purge leftover indices for popular elements:
            foreach (T element in _popularElements.Keys)
            {
                _rightToIndices.Remove(element);
            }
        }

        /// <summary>
        /// Find longest matching block in two subsequences of the left and right sequences.
        /// </summary>
        /// <param name="leftLow">The index of the first element in the left sequence.</param>
        /// <param name="leftHigh">The index of the first element after the last element in the left sequence.</param>
        /// <param name="rightLow">The index of the first element in the right sequence.</param>
        /// <param name="rightHigh">The index of the first element after the last element in the right sequence.</param>
        private MatchingBlock FindLongestMatch(int leftLow, int leftHigh, int rightLow, int rightHigh)
        {
            int bestLeftLow = leftLow;
            int bestRightLow = rightLow;
            int bestSize = 0;

            // During an iteration of the loop, j2len[j] = length of longest
            // junk-free match ending with a[i-1] and b[j]
            Dictionary<int, int> longestLengths = new Dictionary<int,int>();

            for (int i = leftLow; i < leftHigh; i++)
            {
                Dictionary<int, int> newLongestLengths = new Dictionary<int, int>();

                if (_rightToIndices.ContainsKey(_left[i]))
                {
                    foreach (int j in _rightToIndices[_left[i]])
                    {
                        // _left[i] matches _right[j]:
                        if (j < rightLow) continue;
                        if (j >= rightHigh) break;

                        int newSize;
                        
                        if (!longestLengths.TryGetValue(j - 1, out newSize)) newSize = 0;
                        newSize++;

                        newLongestLengths[j] = newSize;

                        if (newSize > bestSize)
                        {
                            bestLeftLow = i - newSize + 1;
                            bestRightLow = j - newSize + 1;
                            bestSize = newSize;
                        }
                    }
                }

                longestLengths = newLongestLengths;
            }

            // Extend the best by non-junk elements on each end.  In particular,
            // "popular" non-junk elements aren't in b2j, which greatly speeds
            // the inner loop above, but also means "the best" match so far
            // doesn't contain any junk *or* popular non-junk elements.

            while (bestLeftLow > leftLow && bestRightLow > rightLow && 
                !_rightJunk.ContainsKey(_right[bestRightLow -1 ]) &
                object.Equals(_left[bestLeftLow - 1], _right[bestRightLow - 1]))
            {
                bestLeftLow--;
                bestRightLow--;
                bestSize++;
            }

            while (bestLeftLow + bestSize < leftHigh && bestRightLow + bestSize < rightHigh &&
              !_rightJunk.ContainsKey(_right[bestRightLow + bestSize]) &&
              object.Equals(_left[bestLeftLow + bestSize], _right[bestRightLow + bestSize]))
            {
                bestSize++;
            }

            // Now that we have a wholly interesting match (albeit possibly
            // empty!), we may as well suck up the matching junk on each
            // side of it too.  Can't think of a good reason not to, and it
            // saves post-processing the (possibly considerable) expense of
            // figuring out what to do with it.  In the case of an empty
            // interesting match, this is clearly the right thing to do,
            // because no other kind of match is possible in the regions.
            while (bestLeftLow > leftLow && bestRightLow > rightLow && 
                  _rightJunk.ContainsKey(_right[bestRightLow - 1]) && 
                  object.Equals(_left[bestLeftLow - 1], _right[bestRightLow - 1]))
            {
                bestLeftLow--;
                bestRightLow--;
                bestSize++;
            }

            while (bestLeftLow + bestSize < leftHigh && bestRightLow + bestSize < rightHigh && 
                  _rightJunk.ContainsKey(_right[bestRightLow + bestSize]) && 
                  object.Equals(_left[bestLeftLow + bestSize], _right[bestRightLow + bestSize]))
            {
                bestSize++;
            }

            return new MatchingBlock(bestLeftLow, bestRightLow, bestSize);
        }

        private void FindAndDivideIntoMatchingBlocks(int leftLow, int leftHigh, int rightLow, int rightHigh, List<MatchingBlock> into)
        {
            MatchingBlock block = FindLongestMatch(leftLow, leftHigh, rightLow, rightHigh);

            if (block.length == 0) return;

            if (leftLow < block.leftStart && rightLow < block.rightStart) 
                FindAndDivideIntoMatchingBlocks(leftLow, block.leftStart, rightLow, block.rightStart, into);

            into.Add(block);

            if (block.leftStart + block.length < leftHigh &&
                block.rightStart + block.length < rightHigh)
                FindAndDivideIntoMatchingBlocks(block.leftStart + block.length, leftHigh, 
                    block.rightStart + block.length, rightHigh, into);
        }
        
        /// <summary>
        /// Return list of triples describing matching subsequences.
        /// </summary>
        public List<MatchingBlock> MatchingBlocks
        {
            get
            {
                if (_left == null || _right == null)
                {
                    throw new InvalidOperationException("Both the left and right sequences must be set " +
                        "to non-null sequences.");
                }

                if (_matchingBlocks == null)
                {
                    _matchingBlocks = new List<MatchingBlock>();
                    FindAndDivideIntoMatchingBlocks(0, _left.Count, 0, _right.Count, _matchingBlocks);
                    _matchingBlocks.Add(new MatchingBlock(_left.Count, _right.Count, 0));
                }

                return _matchingBlocks;
            }
        }


        /// <summary>
        /// Returns a list of operations giving instructions to transform the left
        /// sequence into the right sequence.
        /// </summary>
        /// <returns>A list of operation codes giving the instructions.</returns>
        public List<OpCode> OpCodes
        {
            get
            {
                if (_opCodes == null)
                {
                    int leftPosition = 0;
                    int rightPosition = 0;

                    _opCodes = new List<OpCode>();

                    foreach (MatchingBlock block in MatchingBlocks)
                    {
                        // invariant:  we've pumped out correct diffs to change
                        // a[:i] into b[:j], and the next matching block is
                        // a[ai:ai+size] == b[bj:bj+size].  So we need to pump
                        // out a diff to change a[i:ai] into b[j:bj], pump out
                        // the matching block, and move (i,j) beyond the match

                        OpCodeOperation operation = OpCodeOperation.None;

                        if (leftPosition < block.leftStart && rightPosition < block.rightStart)
                        {
                            operation = OpCodeOperation.Replace;
                        }
                        else
                        {
                            if (leftPosition < block.leftStart) operation = OpCodeOperation.Delete;
                            if (rightPosition < block.rightStart) operation = OpCodeOperation.Insert;
                        }

                        if (operation != OpCodeOperation.None)
                        {
                            _opCodes.Add(new OpCode(operation, leftPosition, block.leftStart,
                                rightPosition, block.rightStart));
                        }

                        leftPosition = block.leftStart + block.length;
                        rightPosition = block.rightStart + block.length;

                        if (block.length > 0)
                        {
                            _opCodes.Add(new OpCode(OpCodeOperation.Equal, block.leftStart, leftPosition,
                                block.rightStart, rightPosition));
                        }
                    }
                }

                return _opCodes;
            }
        }
    }
}
