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
    // This class is a conversion of portions of the Python 2.4 random class.
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
    public class ExtendedRandom
    {
        Random _random;

        public ExtendedRandom()
        {
            _random = new Random();
        }

        public ExtendedRandom(int seed)
        {
            _random = new Random(seed);
        }

        static readonly double _log4 = Math.Log(4.0);
        static readonly double _sgMagicConst = 1.0 + Math.Log(4.5);

        public double GammaVariate(double alpha, double beta)
        {
            // Gamma distribution.  Not the gamma function!

            // Conditions on the parameters are alpha > 0 and beta > 0.

            // alpha > 0, beta > 0, mean is alpha*beta, variance is alpha*beta**2

            // Warning: a few older sources define the gamma distribution in terms
            // of alpha > -1.0

            if (alpha <= 0.0) throw new ArgumentException("Alpha can not be negative or zero.", "alpha");
            if (beta <= 0.0) throw new ArgumentException("Beta can not be negative or zero.", "beta");

            if (alpha > 1.0)
            {
                // Uses R.C.H. Cheng, "The generation of Gamma
                // variables with non-integral shape parameters",
                // Applied Statistics, (1977), 26, No. 1, p71-74

                double ainv = Math.Sqrt(2.0 * alpha - 1.0);
                double bbb = alpha - _log4;
                double ccc = alpha + ainv;

                while (true)
                {
                    double u1 = _random.NextDouble();

                    if (!(1e-7 < u1 && u1 < .9999999)) continue;

                    double u2 = 1.0 - _random.NextDouble();
                    double v = Math.Log(u1 / (1.0 - u1)) / ainv;
                    double x = alpha * Math.Exp(v);
                    double z = u1 * u1 * u2;
                    double r = bbb + ccc * v - x;

                    if (r + _sgMagicConst - 4.5*z >= 0.0 || r >= Math.Log(z)) return x * beta;
                }
            }
            else if (alpha == 1.0)
            {
                double u = _random.NextDouble();

                while (u <= 1e-7) u = _random.NextDouble();

                return -Math.Log(u) * beta;
            }
            else
            {
                // alpha is between 0 and 1 (exclusive)
                // Uses ALGORITHM GS of Statistical Computing - Kennedy & Gentle

                double x;

                while (true)
                {
                    double u = _random.NextDouble();
                    double b = (Math.E + alpha) / Math.E;
                    double p = b * u;

                    if (p <= 1.0)
                    {
                        x = Math.Pow(p, 1.0 / alpha);
                    }
                    else
                    {
                        x = -Math.Log((b - p) / alpha);
                    }

                    double u1 = _random.NextDouble();

                    if (p > 1.0)
                    {
                        if (u1 <= Math.Pow(x, alpha - 1.0)) break;
                    }
                    else 
                    {
                        if (u1 <= Math.Exp(-x)) break;
                    }
                }

                return x * beta;
            }
        }

        public double BetaVariate(double alpha, double beta)
        {
            double y = GammaVariate(alpha, 1.0);

            if (y == 0.0)
            {
                return 0.0;
            }
            else
            {
                return y / (y + GammaVariate(beta, 1.0));
            }
        }
    }
}
