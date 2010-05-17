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

using Interlace.LinearAlgebra.Utilities;

#endregion

namespace Interlace.LinearAlgebra
{
	
	/// <summary>Jama = Java Matrix class.
	/// <P>
	/// The Java Matrix Class provides the fundamental operations of numerical
	/// linear algebra.  Various constructors create Matrices from two dimensional
	/// arrays of double precision floating point numbers.  Various "gets" and
	/// "sets" provide access to submatrices and matrix elements.  Several methods 
	/// implement basic matrix arithmetic, including matrix addition and
	/// multiplication, matrix norms, and element-by-element array operations.
	/// Methods for reading and printing matrices are also included.  All the
	/// operations in this version of the Matrix Class involve real matrices.
	/// Complex matrices may be handled in a future version.
	/// <P>
	/// Five fundamental matrix decompositions, which consist of pairs or triples
	/// of matrices, permutation vectors, and the like, produce results in five
	/// decomposition classes.  These decompositions are accessed by the Matrix
	/// class to compute solutions of simultaneous linear equations, determinants,
	/// inverses and other matrix functions.  The five decompositions are:
	/// <P><UL>
	/// <LI>Cholesky Decomposition of symmetric, positive definite matrices.
	/// <LI>LU Decomposition of rectangular matrices.
	/// <LI>QR Decomposition of rectangular matrices.
	/// <LI>Singular Value Decomposition of rectangular matrices.
	/// <LI>Eigenvalue Decomposition of both symmetric and nonsymmetric square matrices.
	/// </UL>
	/// <DL>
	/// <DT><B>Example of use:</B></DT>
	/// <P>
	/// <DD>Solve a linear system A x = b and compute the residual norm, ||b - A x||.
	/// <P><PRE>
	/// double[][] vals = {{1.,2.,3},{4.,5.,6.},{7.,8.,10.}};
	/// Matrix A = new Matrix(vals);
	/// Matrix b = Matrix.random(3,1);
	/// Matrix x = A.solve(b);
	/// Matrix r = A.times(x).minus(b);
	/// double rnorm = r.normInf();
	/// </PRE></DD>
	/// </DL>
	/// </summary>
	/// <author>  The MathWorks, Inc. and the National Institute of Standards and Technology.
	/// </author>
	/// <version>  5 August 1998
	/// </version>
	
	[Serializable]
	public class Matrix : System.ICloneable
	{
		/// <summary>Access the internal two-dimensional array.</summary>
		/// <returns>     Pointer to the two-dimensional array of matrix elements.
		/// </returns>
		virtual public double[][] Array
		{
			
			
			get
			{
				return A;
			}
			
		}
		/// <summary>Copy the internal two-dimensional array.</summary>
		/// <returns>     Two-dimensional array copy of matrix elements.
		/// </returns>
		virtual public double[][] ArrayCopy
		{
			
			
			get
			{
				double[][] C = new double[m][];
				for (int i = 0; i < m; i++)
				{
					C[i] = new double[n];
				}
				for (int i = 0; i < m; i++)
				{
					for (int j = 0; j < n; j++)
					{
						C[i][j] = A[i][j];
					}
				}
				return C;
			}
			
		}
		/// <summary>Make a one-dimensional column packed copy of the internal array.</summary>
		/// <returns>     Matrix elements packed in a one-dimensional array by columns.
		/// </returns>
		virtual public double[] ColumnPackedCopy
		{
			
			
			get
			{
				double[] vals = new double[m * n];
				for (int i = 0; i < m; i++)
				{
					for (int j = 0; j < n; j++)
					{
						vals[i + j * m] = A[i][j];
					}
				}
				return vals;
			}
			
		}
		/// <summary>Make a one-dimensional row packed copy of the internal array.</summary>
		/// <returns>     Matrix elements packed in a one-dimensional array by rows.
		/// </returns>
		virtual public double[] RowPackedCopy
		{
			
			
			get
			{
				double[] vals = new double[m * n];
				for (int i = 0; i < m; i++)
				{
					for (int j = 0; j < n; j++)
					{
						vals[i * n + j] = A[i][j];
					}
				}
				return vals;
			}
			
		}
		/// <summary>Get row dimension.</summary>
		/// <returns>     m, the number of rows.
		/// </returns>
		virtual public int RowDimension
		{
			
			
			get
			{
				return m;
			}
			
		}
		/// <summary>Get column dimension.</summary>
		/// <returns>     n, the number of columns.
		/// </returns>
		virtual public int ColumnDimension
		{
			
			
			get
			{
				return n;
			}
			
		}
		
		/* ------------------------
		Class variables
		* ------------------------ */
		
		/// <summary>Array for internal storage of elements.</summary>
		/// <serial> internal array storage.
		/// </serial>
		private double[][] A;
		
		/// <summary>Row and column dimensions.</summary>
		/// <serial> row dimension.
		/// </serial>
		/// <serial> column dimension.
		/// </serial>
		private int m, n;
		
		/* ------------------------
		Constructors
		* ------------------------ */
		
		/// <summary>Construct an m-by-n matrix of zeros. </summary>
		/// <param name="m">   Number of rows.
		/// </param>
		/// <param name="n">   Number of colums.
		/// </param>
		
		public Matrix(int m, int n)
		{
			this.m = m;
			this.n = n;
			A = new double[m][];
			for (int i = 0; i < m; i++)
			{
				A[i] = new double[n];
			}
		}
		
		/// <summary>Construct an m-by-n constant matrix.</summary>
		/// <param name="m">   Number of rows.
		/// </param>
		/// <param name="n">   Number of colums.
		/// </param>
		/// <param name="s">   Fill the matrix with this scalar value.
		/// </param>
		
		public Matrix(int m, int n, double s)
		{
			this.m = m;
			this.n = n;
			A = new double[m][];
			for (int i = 0; i < m; i++)
			{
				A[i] = new double[n];
			}
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = s;
				}
			}
		}
		
		/// <summary>Construct a matrix from a 2-D array.</summary>
		/// <param name="A">   Two-dimensional array of doubles.
		/// </param>
		/// <exception cref="IllegalArgumentException">All rows must have the same length
		/// </exception>
		/// <seealso cref="constructWithCopy">
		/// </seealso>
		
		public Matrix(double[][] A)
		{
			m = A.Length;
			n = A[0].Length;
			for (int i = 0; i < m; i++)
			{
				if (A[i].Length != n)
				{
					throw new System.ArgumentException("All rows must have the same length.");
				}
			}
			this.A = A;
		}
		
		/// <summary>Construct a matrix quickly without checking arguments.</summary>
		/// <param name="A">   Two-dimensional array of doubles.
		/// </param>
		/// <param name="m">   Number of rows.
		/// </param>
		/// <param name="n">   Number of colums.
		/// </param>
		
		public Matrix(double[][] A, int m, int n)
		{
			this.A = A;
			this.m = m;
			this.n = n;
		}
		
		/// <summary>Construct a matrix from a one-dimensional packed array</summary>
		/// <param name="vals">One-dimensional array of doubles, packed by columns (ala Fortran).
		/// </param>
		/// <param name="m">   Number of rows.
		/// </param>
		/// <exception cref="IllegalArgumentException">Array length must be a multiple of m.
		/// </exception>
		
		public Matrix(double[] vals, int m)
		{
			this.m = m;
			n = (m != 0?vals.Length / m:0);
			if (m * n != vals.Length)
			{
				throw new System.ArgumentException("Array length must be a multiple of m.");
			}
			A = new double[m][];
			for (int i = 0; i < m; i++)
			{
				A[i] = new double[n];
			}
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = vals[i + j * m];
				}
			}
		}
		
		/* ------------------------
		Public Methods
		* ------------------------ */
		
		/// <summary>Construct a matrix from a copy of a 2-D array.</summary>
		/// <param name="A">   Two-dimensional array of doubles.
		/// </param>
		/// <exception cref="IllegalArgumentException">All rows must have the same length
		/// </exception>
		
		public static Matrix constructWithCopy(double[][] A)
		{
			int m = A.Length;
			int n = A[0].Length;
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				if (A[i].Length != n)
				{
					throw new System.ArgumentException("All rows must have the same length.");
				}
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>Make a deep copy of a matrix</summary>
		
		public virtual Matrix copy()
		{
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>Clone the Matrix object.</summary>
		
		public virtual System.Object Clone()
		{
			return this.copy();
		}
		
		/// <summary>Get a single element.</summary>
		/// <param name="i">   Row index.
		/// </param>
		/// <param name="j">   Column index.
		/// </param>
		/// <returns>     A(i,j)
		/// </returns>
		/// <exception cref="ArrayIndexOutOfBoundsException">
		/// </exception>
		
		public virtual double get_Renamed(int i, int j)
		{
			return A[i][j];
		}
		
		/// <summary>Get a submatrix.</summary>
		/// <param name="i0">  Initial row index
		/// </param>
		/// <param name="i1">  Final row index
		/// </param>
		/// <param name="j0">  Initial column index
		/// </param>
		/// <param name="j1">  Final column index
		/// </param>
		/// <returns>     A(i0:i1,j0:j1)
		/// </returns>
		/// <exception cref="ArrayIndexOutOfBoundsException">Submatrix indices
		/// </exception>
		
		public virtual Matrix getMatrix(int i0, int i1, int j0, int j1)
		{
			Matrix X = new Matrix(i1 - i0 + 1, j1 - j0 + 1);
			double[][] B = X.Array;
			try
			{
				for (int i = i0; i <= i1; i++)
				{
					for (int j = j0; j <= j1; j++)
					{
						B[i - i0][j - j0] = A[i][j];
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException("Submatrix indices");
			}
			return X;
		}
		
		/// <summary>Get a submatrix.</summary>
		/// <param name="r">   Array of row indices.
		/// </param>
		/// <param name="c">   Array of column indices.
		/// </param>
		/// <returns>     A(r(:),c(:))
		/// </returns>
		/// <exception cref="ArrayIndexOutOfBoundsException">Submatrix indices
		/// </exception>
		
		public virtual Matrix getMatrix(int[] r, int[] c)
		{
			Matrix X = new Matrix(r.Length, c.Length);
			double[][] B = X.Array;
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
						B[i][j] = A[r[i]][c[j]];
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException("Submatrix indices");
			}
			return X;
		}
		
		/// <summary>Get a submatrix.</summary>
		/// <param name="i0">  Initial row index
		/// </param>
		/// <param name="i1">  Final row index
		/// </param>
		/// <param name="c">   Array of column indices.
		/// </param>
		/// <returns>     A(i0:i1,c(:))
		/// </returns>
		/// <exception cref="ArrayIndexOutOfBoundsException">Submatrix indices
		/// </exception>
		
		public virtual Matrix getMatrix(int i0, int i1, int[] c)
		{
			Matrix X = new Matrix(i1 - i0 + 1, c.Length);
			double[][] B = X.Array;
			try
			{
				for (int i = i0; i <= i1; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
						B[i - i0][j] = A[i][c[j]];
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException("Submatrix indices");
			}
			return X;
		}
		
		/// <summary>Get a submatrix.</summary>
		/// <param name="r">   Array of row indices.
		/// </param>
		/// <param name="i0">  Initial column index
		/// </param>
		/// <param name="i1">  Final column index
		/// </param>
		/// <returns>     A(r(:),j0:j1)
		/// </returns>
		/// <exception cref="ArrayIndexOutOfBoundsException">Submatrix indices
		/// </exception>
		
		public virtual Matrix getMatrix(int[] r, int j0, int j1)
		{
			Matrix X = new Matrix(r.Length, j1 - j0 + 1);
			double[][] B = X.Array;
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = j0; j <= j1; j++)
					{
						B[i][j - j0] = A[r[i]][j];
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException("Submatrix indices");
			}
			return X;
		}
		
		/// <summary>Set a single element.</summary>
		/// <param name="i">   Row index.
		/// </param>
		/// <param name="j">   Column index.
		/// </param>
		/// <param name="s">   A(i,j).
		/// </param>
		/// <exception cref="ArrayIndexOutOfBoundsException">
		/// </exception>
		
		public virtual void  set_Renamed(int i, int j, double s)
		{
			A[i][j] = s;
		}
		
		/// <summary>Set a submatrix.</summary>
		/// <param name="i0">  Initial row index
		/// </param>
		/// <param name="i1">  Final row index
		/// </param>
		/// <param name="j0">  Initial column index
		/// </param>
		/// <param name="j1">  Final column index
		/// </param>
		/// <param name="X">   A(i0:i1,j0:j1)
		/// </param>
		/// <exception cref="ArrayIndexOutOfBoundsException">Submatrix indices
		/// </exception>
		
		public virtual void  setMatrix(int i0, int i1, int j0, int j1, Matrix X)
		{
			try
			{
				for (int i = i0; i <= i1; i++)
				{
					for (int j = j0; j <= j1; j++)
					{
						A[i][j] = X.get_Renamed(i - i0, j - j0);
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException("Submatrix indices");
			}
		}
		
		/// <summary>Set a submatrix.</summary>
		/// <param name="r">   Array of row indices.
		/// </param>
		/// <param name="c">   Array of column indices.
		/// </param>
		/// <param name="X">   A(r(:),c(:))
		/// </param>
		/// <exception cref="ArrayIndexOutOfBoundsException">Submatrix indices
		/// </exception>
		
		public virtual void  setMatrix(int[] r, int[] c, Matrix X)
		{
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
						A[r[i]][c[j]] = X.get_Renamed(i, j);
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException("Submatrix indices");
			}
		}
		
		/// <summary>Set a submatrix.</summary>
		/// <param name="r">   Array of row indices.
		/// </param>
		/// <param name="j0">  Initial column index
		/// </param>
		/// <param name="j1">  Final column index
		/// </param>
		/// <param name="X">   A(r(:),j0:j1)
		/// </param>
		/// <exception cref="ArrayIndexOutOfBoundsException">Submatrix indices
		/// </exception>
		
		public virtual void  setMatrix(int[] r, int j0, int j1, Matrix X)
		{
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = j0; j <= j1; j++)
					{
						A[r[i]][j] = X.get_Renamed(i, j - j0);
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException("Submatrix indices");
			}
		}
		
		/// <summary>Set a submatrix.</summary>
		/// <param name="i0">  Initial row index
		/// </param>
		/// <param name="i1">  Final row index
		/// </param>
		/// <param name="c">   Array of column indices.
		/// </param>
		/// <param name="X">   A(i0:i1,c(:))
		/// </param>
		/// <exception cref="ArrayIndexOutOfBoundsException">Submatrix indices
		/// </exception>
		
		public virtual void  setMatrix(int i0, int i1, int[] c, Matrix X)
		{
			try
			{
				for (int i = i0; i <= i1; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
						A[i][c[j]] = X.get_Renamed(i - i0, j);
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				throw new System.IndexOutOfRangeException("Submatrix indices");
			}
		}
		
		/// <summary>Matrix transpose.</summary>
		/// <returns>    A'
		/// </returns>
		
		public virtual Matrix transpose()
		{
			Matrix X = new Matrix(n, m);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[j][i] = A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>One norm</summary>
		/// <returns>    maximum column sum.
		/// </returns>
		
		public virtual double norm1()
		{
			double f = 0;
			for (int j = 0; j < n; j++)
			{
				double s = 0;
				for (int i = 0; i < m; i++)
				{
					s += System.Math.Abs(A[i][j]);
				}
				f = System.Math.Max(f, s);
			}
			return f;
		}
		
		/// <summary>Two norm</summary>
		/// <returns>    maximum singular value.
		/// </returns>
		
		public virtual double norm2()
		{
			return (new SingularValueDecomposition(this).norm2());
		}
		
		/// <summary>Infinity norm</summary>
		/// <returns>    maximum row sum.
		/// </returns>
		
		public virtual double normInf()
		{
			double f = 0;
			for (int i = 0; i < m; i++)
			{
				double s = 0;
				for (int j = 0; j < n; j++)
				{
					s += System.Math.Abs(A[i][j]);
				}
				f = System.Math.Max(f, s);
			}
			return f;
		}
		
		/// <summary>Frobenius norm</summary>
		/// <returns>    sqrt of sum of squares of all elements.
		/// </returns>
		
		public virtual double normF()
		{
			double f = 0;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					f = Maths.hypot(f, A[i][j]);
				}
			}
			return f;
		}
		
		/// <summary>Unary minus</summary>
		/// <returns>    -A
		/// </returns>
		
		public virtual Matrix uminus()
		{
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = - A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>C = A + B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A + B
		/// </returns>
		
		public virtual Matrix plus(Matrix B)
		{
			checkMatrixDimensions(B);
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j] + B.A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>A = A + B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A + B
		/// </returns>
		
		public virtual Matrix plusEquals(Matrix B)
		{
			checkMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = A[i][j] + B.A[i][j];
				}
			}
			return this;
		}
		
		/// <summary>C = A - B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A - B
		/// </returns>
		
		public virtual Matrix minus(Matrix B)
		{
			checkMatrixDimensions(B);
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j] - B.A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>A = A - B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A - B
		/// </returns>
		
		public virtual Matrix minusEquals(Matrix B)
		{
			checkMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = A[i][j] - B.A[i][j];
				}
			}
			return this;
		}
		
		/// <summary>Element-by-element multiplication, C = A.*B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A.*B
		/// </returns>
		
		public virtual Matrix arrayTimes(Matrix B)
		{
			checkMatrixDimensions(B);
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j] * B.A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>Element-by-element multiplication in place, A = A.*B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A.*B
		/// </returns>
		
		public virtual Matrix arrayTimesEquals(Matrix B)
		{
			checkMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = A[i][j] * B.A[i][j];
				}
			}
			return this;
		}
		
		/// <summary>Element-by-element right division, C = A./B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A./B
		/// </returns>
		
		public virtual Matrix arrayRightDivide(Matrix B)
		{
			checkMatrixDimensions(B);
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j] / B.A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>Element-by-element right division in place, A = A./B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A./B
		/// </returns>
		
		public virtual Matrix arrayRightDivideEquals(Matrix B)
		{
			checkMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = A[i][j] / B.A[i][j];
				}
			}
			return this;
		}
		
		/// <summary>Element-by-element left division, C = A.\B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A.\B
		/// </returns>
		
		public virtual Matrix arrayLeftDivide(Matrix B)
		{
			checkMatrixDimensions(B);
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = B.A[i][j] / A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>Element-by-element left division in place, A = A.\B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     A.\B
		/// </returns>
		
		public virtual Matrix arrayLeftDivideEquals(Matrix B)
		{
			checkMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = B.A[i][j] / A[i][j];
				}
			}
			return this;
		}
		
		/// <summary>Multiply a matrix by a scalar, C = s*A</summary>
		/// <param name="s">   scalar
		/// </param>
		/// <returns>     s*A
		/// </returns>
		
		public virtual Matrix times(double s)
		{
			Matrix X = new Matrix(m, n);
			double[][] C = X.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = s * A[i][j];
				}
			}
			return X;
		}
		
		/// <summary>Multiply a matrix by a scalar in place, A = s*A</summary>
		/// <param name="s">   scalar
		/// </param>
		/// <returns>     replace A by s*A
		/// </returns>
		
		public virtual Matrix timesEquals(double s)
		{
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = s * A[i][j];
				}
			}
			return this;
		}
		
		/// <summary>Linear algebraic matrix multiplication, A * B</summary>
		/// <param name="B">   another matrix
		/// </param>
		/// <returns>     Matrix product, A * B
		/// </returns>
		/// <exception cref="IllegalArgumentException">Matrix inner dimensions must agree.
		/// </exception>
		
		public virtual Matrix times(Matrix B)
		{
			if (B.m != n)
			{
				throw new System.ArgumentException("Matrix inner dimensions must agree.");
			}
			Matrix X = new Matrix(m, B.n);
			double[][] C = X.Array;
			double[] Bcolj = new double[n];
			for (int j = 0; j < B.n; j++)
			{
				for (int k = 0; k < n; k++)
				{
					Bcolj[k] = B.A[k][j];
				}
				for (int i = 0; i < m; i++)
				{
					double[] Arowi = A[i];
					double s = 0;
					for (int k = 0; k < n; k++)
					{
						s += Arowi[k] * Bcolj[k];
					}
					C[i][j] = s;
				}
			}
			return X;
		}
		
		/// <summary>LU Decomposition</summary>
		/// <returns>     LUDecomposition
		/// </returns>
		/// <seealso cref="LUDecomposition">
		/// </seealso>
		
		public virtual LUDecomposition lu()
		{
			return new LUDecomposition(this);
		}
		
		/// <summary>QR Decomposition</summary>
		/// <returns>     QRDecomposition
		/// </returns>
		/// <seealso cref="QRDecomposition">
		/// </seealso>
		
		public virtual QRDecomposition qr()
		{
			return new QRDecomposition(this);
		}
		
		/// <summary>Cholesky Decomposition</summary>
		/// <returns>     CholeskyDecomposition
		/// </returns>
		/// <seealso cref="CholeskyDecomposition">
		/// </seealso>
		
		public virtual CholeskyDecomposition chol()
		{
			return new CholeskyDecomposition(this);
		}
		
		/// <summary>Singular Value Decomposition</summary>
		/// <returns>     SingularValueDecomposition
		/// </returns>
		/// <seealso cref="SingularValueDecomposition">
		/// </seealso>
		
		public virtual SingularValueDecomposition svd()
		{
			return new SingularValueDecomposition(this);
		}
		
		/// <summary>Eigenvalue Decomposition</summary>
		/// <returns>     EigenvalueDecomposition
		/// </returns>
		/// <seealso cref="EigenvalueDecomposition">
		/// </seealso>
		
		public virtual EigenvalueDecomposition eig()
		{
			return new EigenvalueDecomposition(this);
		}
		
		/// <summary>Solve A*X = B</summary>
		/// <param name="B">   right hand side
		/// </param>
		/// <returns>     solution if A is square, least squares solution otherwise
		/// </returns>
		
		public virtual Matrix solve(Matrix B)
		{
			return (m == n?(new LUDecomposition(this)).solve(B):(new QRDecomposition(this)).solve(B));
		}
		
		/// <summary>Solve X*A = B, which is also A'*X' = B'</summary>
		/// <param name="B">   right hand side
		/// </param>
		/// <returns>     solution if A is square, least squares solution otherwise.
		/// </returns>
		
		public virtual Matrix solveTranspose(Matrix B)
		{
			return transpose().solve(B.transpose());
		}
		
		/// <summary>Matrix inverse or pseudoinverse</summary>
		/// <returns>     inverse(A) if A is square, pseudoinverse otherwise.
		/// </returns>
		
		public virtual Matrix inverse()
		{
			return solve(identity(m, m));
		}
		
		/// <summary>Matrix determinant</summary>
		/// <returns>     determinant
		/// </returns>
		
		public virtual double det()
		{
			return new LUDecomposition(this).det();
		}
		
		/// <summary>Matrix rank</summary>
		/// <returns>     effective numerical rank, obtained from SVD.
		/// </returns>
		
		public virtual int rank()
		{
			return new SingularValueDecomposition(this).rank();
		}
		
		/// <summary>Matrix condition (2 norm)</summary>
		/// <returns>     ratio of largest to smallest singular value.
		/// </returns>
		
		public virtual double cond()
		{
			return new SingularValueDecomposition(this).cond();
		}
		
		/// <summary>Matrix trace.</summary>
		/// <returns>     sum of the diagonal elements.
		/// </returns>
		
		public virtual double trace()
		{
			double t = 0;
			for (int i = 0; i < System.Math.Min(m, n); i++)
			{
				t += A[i][i];
			}
			return t;
		}
		
		/// <summary>Generate matrix with random elements</summary>
		/// <param name="m">   Number of rows.
		/// </param>
		/// <param name="n">   Number of colums.
		/// </param>
		/// <returns>     An m-by-n matrix with uniformly distributed random elements.
		/// </returns>
		
		public static Matrix random(int m, int n)
		{
        	Random random = new Random();

			Matrix A = new Matrix(m, n);
			double[][] X = A.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					X[i][j] = random.NextDouble();
				}
			}
			return A;
		}
		
		/// <summary>Generate identity matrix</summary>
		/// <param name="m">   Number of rows.
		/// </param>
		/// <param name="n">   Number of colums.
		/// </param>
		/// <returns>     An m-by-n matrix with ones on the diagonal and zeros elsewhere.
		/// </returns>
		
		public static Matrix identity(int m, int n)
		{
			Matrix A = new Matrix(m, n);
			double[][] X = A.Array;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					X[i][j] = (i == j?1.0:0.0);
				}
			}
			return A;
		}
		
		/// <summary>Print the matrix to the output stream.   Line the elements up in
		/// columns with a Fortran-like 'Fw.d' style format.
		/// </summary>
		/// <param name="output">Output stream.
		/// </param>
		/// <param name="w">     Column width.
		/// </param>
		/// <param name="d">     Number of digits after the decimal.
		/// </param>
		
		/// <summary>Read a matrix from a stream.  The format is the same the print method,
		/// so printed matrices can be read back in (provided they were printed using
		/// US Locale).  Elements are separated by
		/// whitespace, all the elements for each row appear on a single line,
		/// the last row is followed by a blank line.
		/// </summary>
		/// <param name="input">the input stream.
		/// </param>
		
		/* ------------------------
		Private Methods
		* ------------------------ */
		
		/// <summary>Check if size(A) == size(B) *</summary>
		
		private void  checkMatrixDimensions(Matrix B)
		{
			if (B.m != m || B.n != n)
			{
				throw new System.ArgumentException("Matrix dimensions must agree.");
			}
		}
	}
}
