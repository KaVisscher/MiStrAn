﻿/*
    Matrix class in C#
    Written by Ivan Kuckir (ivan.kuckir@gmail.com, http://blog.ivank.net)
    Faculty of Mathematics and Physics
    Charles University in Prague
    (C) 2010
    - updated on 1. 6.2014 - Trimming the string before parsing
    - updated on 14.6.2012 - parsing improved. Thanks to Andy!
    - updated on 3.10.2012 - there was a terrible bug in LU, SoLE and Inversion. Thanks to Danilo Neves Cruz for reporting that!
	
    This code is distributed under MIT licence.
	
	
		Permission is hereby granted, free of charge, to any person
		obtaining a copy of this software and associated documentation
		files (the "Software"), to deal in the Software without
		restriction, including without limitation the rights to use,
		copy, modify, merge, publish, distribute, sublicense, and/or sell
		copies of the Software, and to permit persons to whom the
		Software is furnished to do so, subject to the following
		conditions:

		The above copyright notice and this permission notice shall be
		included in all copies or substantial portions of the Software.

		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
		EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
		OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
		NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
		HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
		WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
		FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
		OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace MiStrAnEngine
{
    [Serializable]
    public class Matrix
    {
        public int rows;
        public int cols;
        public double[,] mat;

        public Matrix L;
        public Matrix U;
        public Matrix D;
        private int[] pi;
        private double detOfP = 1;

        public Matrix(int iRows, int iCols)         // Matrix Class constructor
        {
            rows = iRows;
            cols = iCols;
            mat = new double[rows, cols];
        }

        // Added by Gustav 2017-01-30
        // Constructor for predefined mat values
        public Matrix(double[,] _mat) : this(_mat.GetLength(0), _mat.GetLength(1))       // Matrix Class constructor
        {
            mat = _mat;
        }

        public Matrix(double[] _mat) : this(_mat.Length,1)
        {
            for (int i = 0; i < _mat.Length; i++)
            {
                mat[i, 0] = _mat[i];
            }

        }

        public Matrix(Matrix copy) : this(copy.rows,copy.cols)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    mat[i, j] = copy[i, j];
                }
            }


        }

        public double Norm
        {
            get
            {

                double sum = 0;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        sum += this[i, j] * this[i, j];
                    }
                }

                return Math.Sqrt(sum);

            }
        }

        public Boolean IsSquare()
        {
            return (rows == cols);
        }


        public Matrix this[int[] iRows, int[] iCols]      // Access sub array
        {
            get
            {
                Matrix subMat = new Matrix(iRows.Length, iCols.Length);

                for (int i = 0; i < iRows.Length; i++)
                {
                    for (int j = 0; j < iCols.Length; j++)
                    {
                        subMat[i, j] = mat[iRows[i], iCols[j]];
                    }
                }

                return subMat;

            }
            set
            {
                for (int i = 0; i < iRows.Length; i++)
                {
                    for (int j = 0; j < iCols.Length; j++)
                    {
                        mat[iRows[i], iCols[j]] = value[i, j];
                    }
                }
            }
        }

        public Vector3D ToVector3D()
        {
            return new Vector3D(this[0], this[1], this[2]);

        }

        public Vector ToVector()
        {
            if (cols != 1) throw new MException("Matrix cannot be converted to vector. Has several columns");
            Vector ret = new Vector(rows);
            for (int i = 0; i < rows; i++)
            {
                ret[i] = mat[i, 0];
            }

            return ret;

        }

        public double this[int i]
        {
            get
            {
                if (cols == 1)
                    return mat[i, 0];
                else if (rows == 1)
                    return mat[0, i];
                else
                    throw new MException("Operator for 1D matrix used. Matrix is multidimensional");
            }

            set
            {
                if (cols == 1)
                    mat[i, 0] = value;
                else if (rows == 1)
                    mat[0, i] = value;
                else
                    throw new MException("Operator for 1D matrix used. Matrix is multidimensional");
            }
        }


        public Matrix this[int iRow, int[] iCols]      // Access sub array
        {
            get
            {
                return this[new int[] { iRow }, iCols];
            }
            set
            {
                this[new int[] { iRow }, iCols] = value;
            }
        }

        public Matrix this[int[] iRows, int iCol]      // Access sub array
        {
            get
            {
                return this[iRows, new int[] { iCol }];
            }
            set
            {
                this[iRows, new int[] { iCol }] = value;
            }
        }



        public double this[int iRow, int iCol]      // Access this matrix as a 2D array
        {
            get { return mat[iRow, iCol]; }
            set { mat[iRow, iCol] = value; }
        }

        public Matrix GetCol(int k)
        {
            Matrix m = new Matrix(rows, 1);
            for (int i = 0; i < rows; i++) m[i, 0] = mat[i, k];
            return m;
        }

        public Matrix GetRow(int k)
        {
            Matrix m = new Matrix(1, cols);
            for (int i = 0; i < cols; i++)
                m[0, i] = mat[k, i];
            return m;
        }

        public void SetCol(Matrix v, int k)
        {
            for (int i = 0; i < rows; i++) mat[i, k] = v[i, 0];
        }

        public void SetRow(Matrix v, int k)
        {
            for (int i = 0; i < cols; i++) mat[k, i] = v[0, i];
        }

        public void MakeLU()                        // Function for LU decomposition
        {
            if (!IsSquare()) throw new MException("The matrix is not square!");
            L = IdentityMatrix(rows, cols);
            U = Duplicate();

            pi = new int[rows];
            for (int i = 0; i < rows; i++) pi[i] = i;

            double p = 0;
            double pom2;
            int k0 = 0;
            int pom1 = 0;

            for (int k = 0; k < cols - 1; k++)
            {
                p = 0;
                for (int i = k; i < rows; i++)      // find the row with the biggest pivot
                {
                    if (Math.Abs(U[i, k]) > p)
                    {
                        p = Math.Abs(U[i, k]);
                        k0 = i;
                    }
                }
                if (p == 0) // samé nuly ve sloupci
                    throw new MException("The matrix is singular!");

                pom1 = pi[k]; pi[k] = pi[k0]; pi[k0] = pom1;    // switch two rows in permutation matrix

                for (int i = 0; i < k; i++)
                {
                    pom2 = L[k, i]; L[k, i] = L[k0, i]; L[k0, i] = pom2;
                }

                if (k != k0) detOfP *= -1;

                for (int i = 0; i < cols; i++)                  // Switch rows in U
                {
                    pom2 = U[k, i]; U[k, i] = U[k0, i]; U[k0, i] = pom2;
                }

                for (int i = k + 1; i < rows; i++)
                {
                    L[i, k] = U[i, k] / U[k, k];
                    for (int j = k; j < cols; j++)
                        U[i, j] = U[i, j] - L[i, k] * U[k, j];
                }
            }
        }



        public void MakeLL()
        {
            double[,] a = mat;

            int n = (int)Math.Sqrt(a.Length);

            double[,] ret = new double[n, n];
            for (int r = 0; r < n; r++)
                for (int c = 0; c <= r; c++)
                {
                    if (c == r)
                    {
                        double sum = 0;
                        for (int j = 0; j < c; j++)
                        {
                            sum += ret[c, j] * ret[c, j];
                        }
                        ret[c, c] = Math.Sqrt(a[c, c] - sum);
                    }
                    else
                    {
                        double sum = 0;
                        for (int j = 0; j < c; j++)
                            sum += ret[r, j] * ret[c, j];
                        ret[r, c] = 1.0 / ret[c, c] * (a[r, c] - sum);
                    }
                }

            L = new Matrix(ret);
        }

        public void MakeIncompleteLL()
        {
            double[,] a = mat;

            int n = (int)Math.Sqrt(a.Length);

            double[,] ret = new double[n, n];
            for (int r = 0; r < n; r++)
                for (int c = 0; c <= r; c++)
                {
                    if (a[r, c] == 0)
                        continue;

                    if (c == r)
                    {
                        double sum = 0;
                        for (int j = 0; j < c; j++)
                        {
                            sum += ret[c, j] * ret[c, j];
                        }
                        ret[c, c] = Math.Sqrt(a[c, c] - sum);
                    }
                    else
                    {
                        double sum = 0;
                        for (int j = 0; j < c; j++)
                            sum += ret[r, j] * ret[c, j];
                        ret[r, c] = 1.0 / ret[c, c] * (a[r, c] - sum);
                    }
                }

            L = new Matrix(ret);
        }

        public void MakeLL_ALGLIB()
        {
            double[,] a = mat;
            alglib.smp_spdmatrixcholesky(ref a, a.GetLength(1), false);

            L = new Matrix(a);
        }

        // See https://en.wikipedia.org/wiki/Cholesky_decomposition#The_Cholesky_algorithm
        //Using the The LDL-algorithm
        public void MakeLDL()
        {
            Matrix A = this;
            L = new Matrix(rows, cols);
            D = new Matrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {

                for (int j = 0; j <= i; j++)
                {
                    if(i==j)
                    {
                        L[i, j] = 1;
                        double sum = 0;
                        for (int k = 0; k < j; k++)
                        {
                            sum += L[j, k] * L[j, k] * D[k, k];
                        }
                        D[j, j] = A[j, j] - sum;
                    }
                    else
                    {
                        double sum = 0;
                        for (int k = 0; k < j; k++)
                        {
                            sum += L[i, k] * L[j, k] * D[k, k];
                        }

                        if (D[j, j] == 0)
                            L[i, j] = 0;
                        else
                            L[i, j] = (1 / D[j, j]) * (A[i, j] - sum);
                    }
                }
            }

        }


        private static double CholeskySumDiagonal(Matrix L,int j)
        {
            double sum = 0;

            for (int k = 0; k < j; k++)
                sum += Math.Pow(L[j,k], 2);

            return sum;
        }

        private static double CholeskySum(Matrix L, int i, int j)
        {
            double sum = 0;

            for (int k = 0; k < j; k++)
                sum += L[i,k]*L[j,k];

            return sum;
        }

        public Matrix Transpose()
        {
            Matrix M = Matrix.Transpose(this);

            return M;
        }

        public Matrix SolveWith(Matrix v)                        // Function solves Ax = v in confirmity with solution vector "v"
        {
            if (rows != cols) throw new MException("The matrix is not square!");
            if (rows != v.rows) throw new MException("Wrong number of results in solution vector!");
            if (L == null) MakeLU();

            Matrix b = new Matrix(rows, 1);
            for (int i = 0; i < rows; i++) b[i, 0] = v[pi[i], 0];   // switch two items in "v" due to permutation matrix

            Matrix z = SubsForth(L, b);
            Matrix x = SubsBack(U, z);

            return x;
        }

        // Assumes symmetric non definite matrix.
        // see https://se.mathworks.com/help/dsp/ref/ldlsolver.html
        public Matrix SolveWith_LDL(Matrix v)                        // Function solves Ax = v in confirmity with solution vector "v"
        {
            if (rows != cols) throw new MException("The matrix is not square!");
            if (rows != v.rows) throw new MException("Wrong number of results in solution vector!");
            if (D == null) MakeLDL();

            Matrix Y = SolveLowerTriangular(L, v);
            Matrix Z = SolveDiagonal(D, Y);
            Matrix X = SolveUpperTriangular(L.Transpose(), Z);
            

            return X;
        }

        public void ToCSRFormat(out int[] row, out int[] col, out double[] val, out int nnz, out int N)
        {
            if (rows != cols) throw new MException("The matrix is not square!");
            N = rows;

            row = new int[N + 1];
            


            List<int> _col = new List<int>();
            List<double> _val = new List<double>();

            nnz = 0;
            int count = 0;
            for (int i = 0; i < rows; i++)
            {
                bool foundFirst = false;
                for (int j = 0; j < cols; j++)
                {
                    if (mat[i, j] == 0)
                        continue;
                    if (!foundFirst)
                    {
                        row[i] = count;
                        foundFirst = true;
                    }
                    _val.Add(mat[i, j]);
                    _col.Add(j);
                    count++;
                    nnz++;
                }
            }

            row[N] = nnz;
            col = _col.ToArray();
            val = _val.ToArray();


        }

        [DllImport("SolverWrapperNativeCode.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double CPUsolveCSRDouble(int[] row_offset, int[] col, double[] val,
int nnz, int N, double[] _rhs, double[] _x);

        [DllImport("SolverWrapperNativeCode.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double solveCSRDouble(int[] row_offset, int[] col, double[] val,
    int nnz, int N, double[] _rhs, double[] _x);




        // Assumes symmetric positive definite matrix.
        // Uses cholesky decomposition
        public Matrix SolveWith_LL(Matrix v)                        // Function solves Ax = v in confirmity with solution vector "v"
        {
            if (rows != cols) throw new MException("The matrix is not square!");
            if (rows != v.rows) throw new MException("Wrong number of results in solution vector!");
            MakeLL_ALGLIB();


            Matrix Y = SolveLowerTriangular(L, v);
            Matrix X = SolveUpperTriangular(L.Transpose(), Y);


            return X;
        }


        // see https://en.wikipedia.org/wiki/Conjugate_gradient_method
        public Matrix SolveWith_CG(Matrix b)
        {
            Matrix A = this;

            Matrix x = new Matrix(b.rows, b.cols);

            Matrix r = b - A * x;
            Matrix old_r;

            Matrix p = new Matrix(r);

            int maxIterations = 1000;

            double tol = 1e-3;

            for (int i = 0; i < maxIterations; i++)
            {
                double alpha_k = (r.Transpose() * r)[0, 0] / (p.Transpose() * A * p)[0, 0];
                x = x + alpha_k * p;

                old_r = new Matrix(r);
                r = r - alpha_k * A * p;


                if (r.Norm < tol)
                    break;
                double beta_k = (r.Transpose() * r)[0, 0] / (old_r.Transpose() * old_r)[0, 0];
                p = r + beta_k * p;
            }


            return x;
        }

        public Matrix SolveWith_CGPrecon(Matrix b)
        {
            Matrix A = this;
            A.MakeIncompleteLL();

            Matrix M = A.L * A.L.Transpose();
            M = M.Invert();

            Matrix x = new Matrix(b.rows, b.cols);

            Matrix r = b - A * x;
            Matrix z = M * r;
            Matrix old_r,old_z;

            Matrix p = new Matrix(z);

            int maxIterations = 1000;

            double tol = 1e-3;

            for (int i = 0; i < maxIterations; i++)
            {
                double alpha_k = (r.Transpose() * z)[0, 0] / (p.Transpose() * A * p)[0, 0];
                x = x + alpha_k * p;

                old_r = new Matrix(r);
                r = r - alpha_k * A * p;


                if (r.Norm < tol)
                    break;
                old_z = new Matrix(z);
                z = M * r;
                double beta_k = (z.Transpose() * r)[0, 0] / (old_z.Transpose() * old_r)[0, 0];
                p = z + beta_k * p;
            }


            return x;
        }








        private static Matrix SolveLowerTriangular(Matrix A, Matrix v)
        {
            Matrix X = new Matrix(v.rows, 1);

            for (int i = 0; i < A.rows; i++)
            {
                double sum = 0;
                for (int k = 0; k < i; k++)
                    sum += A[i, k] * X[k, 0];

                X[i, 0] = (v[i, 0] - sum) / A[i, i];
            }

            return X;

        }

        private static Matrix SolveDiagonal(Matrix D, Matrix v)
        {
            Matrix X = new Matrix(D.rows, 1);

            for (int i = 0; i < D.rows; i++)
            {
                X[i, 0] =  v[i, 0]/ D[i, i];
            }

            return X;
        }

        private static Matrix SolveUpperTriangular(Matrix A, Matrix v)
        {
            Matrix X = new Matrix(v.rows, 1);


            for (int i = A.rows-1; i >=0; i--)
            {
                double sum = 0;
                for (int k = i+1; k < A.rows; k++)
                    sum += A[i, k] * X[k, 0];

                X[i, 0] = (v[i, 0] - sum)/A[i,i];
            }

            return X;

        }

        public Matrix Invert()                                   // Function returns the inverted matrix
        {
            if (L == null) MakeLU();

            Matrix inv = new Matrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                Matrix Ei = Matrix.ZeroMatrix(rows, 1);
                Ei[i, 0] = 1;
                Matrix col = SolveWith(Ei);
                inv.SetCol(col, i);
            }
            return inv;
        }

        public double Max()
        {
            return mat.Cast<double>().Max();
        }

        public double Det()                         // Function for determinant
        {
            if (L == null) MakeLU();
            double det = detOfP;
            for (int i = 0; i < rows; i++) det *= U[i, i];
            return det;
        }

        public Matrix GetP()                        // Function returns permutation matrix "P" due to permutation vector "pi"
        {
            if (L == null) MakeLU();

            Matrix matrix = ZeroMatrix(rows, cols);
            for (int i = 0; i < rows; i++) matrix[pi[i], i] = 1;
            return matrix;
        }

        public Matrix Duplicate()                   // Function returns the copy of this matrix
        {
            Matrix matrix = new Matrix(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    matrix[i, j] = mat[i, j];
            return matrix;
        }

        public static Matrix SubsForth(Matrix A, Matrix b)          // Function solves Ax = b for A as a lower triangular matrix
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            Matrix x = new Matrix(n, 1);

            for (int i = 0; i < n; i++)
            {
                x[i, 0] = b[i, 0];
                for (int j = 0; j < i; j++) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static Matrix SubsBack(Matrix A, Matrix b)           // Function solves Ax = b for A as an upper triangular matrix
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            Matrix x = new Matrix(n, 1);

            for (int i = n - 1; i > -1; i--)
            {
                x[i, 0] = b[i, 0];
                for (int j = n - 1; j > i; j--) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static Matrix ZeroMatrix(int iRows, int iCols)       // Function generates the zero matrix
        {
            Matrix matrix = new Matrix(iRows, iCols);

            return matrix;
        }

        public static Matrix Ones(int iRows, int iCols)
        {
            Matrix M = new Matrix(iRows, iCols);

            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    M[i, j] = 1;

            return M;

        }

        // Replicating MATLABS diag function
        //
        //  /Gustav
        public static Matrix Diag(Matrix M)
        {
            if (M.rows == 1 && M.cols == 1)
                return M;

            if (M.IsSquare())
            {
                Matrix A = new Matrix(M.rows, 1);

                for (int i = 0; i < M.rows; i++)
                    A[i, 0] = M[i, i];

                return A;
            }

            Matrix B = Matrix.ZeroMatrix(M.rows, M.rows);
            for (int i = 0; i < M.rows; i++)
                B[i, i] = M[i, 0];

            return B;

        }

        public SparseMatrix ToSparse()
        {
            SparseMatrix M = new SparseMatrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if(mat[i,j] != 0)
                       M[i, j] = mat[i, j];
                }
            }

            return M;
        }

        public alglib.sparsematrix ToAlglibSparse()
        {
            alglib.sparsematrix M;

            alglib.sparsecreate(this.rows, this.cols, out M);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (mat[i, j] != 0)
                        alglib.sparseset(M, i, j, mat[i, j]); 
                }
            }

            return M;
        }

        public static Matrix IdentityMatrix(int iRows, int iCols)   // Function generates the identity matrix
        {
            Matrix matrix = ZeroMatrix(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                matrix[i, i] = 1;
            return matrix;
        }

        public static Matrix RandomMatrix(int iRows, int iCols, int dispersion)       // Function generates the random matrix
        {
            Random random = new Random();
            Matrix matrix = new Matrix(iRows, iCols);
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    matrix[i, j] = random.Next(-dispersion, dispersion);
            return matrix;
        }

        public static Matrix Parse(string ps)                        // Function parses the matrix from string
        {
            string s = NormalizeMatrixString(ps);
            string[] rows = Regex.Split(s, "\r\n");
            string[] nums = rows[0].Split(' ');
            Matrix matrix = new Matrix(rows.Length, nums.Length);
            try
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    nums = rows[i].Split(' ');
                    for (int j = 0; j < nums.Length; j++) matrix[i, j] = double.Parse(nums[j]);
                }
            }
            catch (FormatException exc) { throw new MException("Wrong input format!"); }
            return matrix;
        }

        public override string ToString()                           // Function returns matrix as a string
        {
            string s = "";
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++) s += String.Format("{0,5:0.0000}", mat[i, j]) + " ";
                s += "\r\n";
            }
            return s;
        }

        public static Matrix Transpose(Matrix m)              // Matrix transpose, for any rectangular matrix
        {
            Matrix t = new Matrix(m.cols, m.rows);
            for (int i = 0; i < m.rows; i++)
                for (int j = 0; j < m.cols; j++)
                    t[j, i] = m[i, j];
            return t;
        }

        public static Matrix Power(Matrix m, int pow)           // Power matrix to exponent
        {
            if (pow == 0) return IdentityMatrix(m.rows, m.cols);
            if (pow == 1) return m.Duplicate();
            if (pow == -1) return m.Invert();

            Matrix x;
            if (pow < 0) { x = m.Invert(); pow *= -1; }
            else x = m.Duplicate();

            Matrix ret = IdentityMatrix(m.rows, m.cols);
            while (pow != 0)
            {
                if ((pow & 1) == 1) ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        private static void SafeAplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                    if (xb + j < B.cols && yb + i < B.rows) C[i, j] += B[yb + i, xb + j];
                }
        }

        private static void SafeAminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                    if (xb + j < B.cols && yb + i < B.rows) C[i, j] -= B[yb + i, xb + j];
                }
        }

        private static void SafeACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                }
        }

        private static void AplusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
        }

        private static void AminusBintoC(Matrix A, int xa, int ya, Matrix B, int xb, int yb, Matrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
        }

        private static void ACopytoC(Matrix A, int xa, int ya, Matrix C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j];
        }

        // den här suuuuuuuuuuuuuuuuuuuuuuuuuger
        private static Matrix StrassenMultiply(Matrix A, Matrix B)                // Smart matrix multiplication
        {
            if (A.cols != B.rows) throw new MException("Wrong dimension of matrix!");

            Matrix R;

            int msize = Math.Max(Math.Max(A.rows, A.cols), Math.Max(B.rows, B.cols));

            if (msize < 32)
            {
                R = ZeroMatrix(A.rows, B.cols);
                for (int i = 0; i < R.rows; i++)
                    for (int j = 0; j < R.cols; j++)
                        for (int k = 0; k < A.cols; k++)
                            R[i, j] += A[i, k] * B[k, j];
                return R;
            }

            int size = 1; int n = 0;
            while (msize > size) { size *= 2; n++; };
            int h = size / 2;


            Matrix[,] mField = new Matrix[n, 9];

            /*
             *  8x8, 8x8, 8x8, ...
             *  4x4, 4x4, 4x4, ...
             *  2x2, 2x2, 2x2, ...
             *  . . .
             */

            int z;
            for (int i = 0; i < n - 4; i++)          // rows
            {
                z = (int)Math.Pow(2, n - i - 1);
                for (int j = 0; j < 9; j++) mField[i, j] = new Matrix(z, z);
            }

            SafeAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

            SafeAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
            SafeACopytoC(B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

            SafeACopytoC(A, 0, 0, mField[0, 0], h);
            SafeAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

            SafeACopytoC(A, h, h, mField[0, 0], h);
            SafeAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

            SafeAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
            SafeACopytoC(B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

            SafeAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

            SafeAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

            R = new Matrix(A.rows, B.cols);                  // result

            /// C11
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] + mField[0, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] + mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];

            return R;
        }

        // function for square matrix 2^N x 2^N

        private static void StrassenMultiplyRun(Matrix A, Matrix B, Matrix C, int l, Matrix[,] f)    // A * B into C, level of recursion, matrix field
        {
            int size = A.rows;
            int h = size / 2;

            if (size < 32)
            {
                for (int i = 0; i < C.rows; i++)
                    for (int j = 0; j < C.cols; j++)
                    {
                        C[i, j] = 0;
                        for (int k = 0; k < A.cols; k++) C[i, j] += A[i, k] * B[k, j];
                    }
                return;
            }

            AplusBintoC(A, 0, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 1], l + 1, f); // (A11 + A22) * (B11 + B22);

            AplusBintoC(A, 0, h, A, h, h, f[l, 0], h);
            ACopytoC(B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 2], l + 1, f); // (A21 + A22) * B11;

            ACopytoC(A, 0, 0, f[l, 0], h);
            AminusBintoC(B, h, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 3], l + 1, f); //A11 * (B12 - B22);

            ACopytoC(A, h, h, f[l, 0], h);
            AminusBintoC(B, 0, h, B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 4], l + 1, f); //A22 * (B21 - B11);

            AplusBintoC(A, 0, 0, A, h, 0, f[l, 0], h);
            ACopytoC(B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 5], l + 1, f); //(A11 + A12) * B22;

            AminusBintoC(A, 0, h, A, 0, 0, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 6], l + 1, f); //(A21 - A11) * (B11 + B12);

            AminusBintoC(A, h, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, h, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 7], l + 1, f); // (A12 - A22) * (B21 + B22);

            /// C11
            for (int i = 0; i < h; i++)          // rows
                for (int j = 0; j < h; j++)     // cols
                    C[i, j] = f[l, 1 + 1][i, j] + f[l, 1 + 4][i, j] - f[l, 1 + 5][i, j] + f[l, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < h; i++)          // rows
                for (int j = h; j < size; j++)     // cols
                    C[i, j] = f[l, 1 + 3][i, j - h] + f[l, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < size; i++)          // rows
                for (int j = 0; j < h; j++)     // cols
                    C[i, j] = f[l, 1 + 2][i - h, j] + f[l, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < size; i++)          // rows
                for (int j = h; j < size; j++)     // cols
                    C[i, j] = f[l, 1 + 1][i - h, j - h] - f[l, 1 + 2][i - h, j - h] + f[l, 1 + 3][i - h, j - h] + f[l, 1 + 6][i - h, j - h];
        }

        public static Matrix StupidMultiply(Matrix m1, Matrix m2)                  // Stupid matrix multiplication
        {
            if (m1.cols != m2.rows) throw new MException("Wrong dimensions of matrix!");

            Matrix result = ZeroMatrix(m1.rows, m2.cols);
            for (int i = 0; i < result.rows; i++)
                for (int j = 0; j < result.cols; j++)
                    for (int k = 0; k < m1.cols; k++)
                        result[i, j] += m1[i, k] * m2[k, j];
            return result;
        }
        private static Matrix Multiply(double n, Matrix m)                          // Multiplication by constant n
        {
            Matrix r = new Matrix(m.rows, m.cols);
            for (int i = 0; i < m.rows; i++)
                for (int j = 0; j < m.cols; j++)
                    r[i, j] = m[i, j] * n;
            return r;
        }
        private static Matrix Add(Matrix m1, Matrix m2)         // Sčítání matic
        {
            if (m1.rows != m2.rows || m1.cols != m2.cols) throw new MException("Matrices must have the same dimensions!");
            Matrix r = new Matrix(m1.rows, m1.cols);
            for (int i = 0; i < r.rows; i++)
                for (int j = 0; j < r.cols; j++)
                    r[i, j] = m1[i, j] + m2[i, j];
            return r;
        }

        // Gustavs addition
        public static Matrix MergeHorizontal(Matrix A, Matrix B)
        {


            if (A.rows != B.rows)
                throw new System.ArgumentException("Matrices must have same number of rows", "original");

            Matrix C = new Matrix(A.rows, A.cols + B.cols);

            for (int i = 0; i < A.cols; i++)
                C.SetCol(A.GetCol(i), i);

            for (int i = 0; i < B.cols; i++)
                C.SetCol(B.GetCol(i), A.cols + i);

            return C;
        }

        public static Matrix MergeHorizontal(Matrix A, Matrix B, Matrix C)
        {
            Matrix D = MergeHorizontal(A, B);
            Matrix E = MergeHorizontal(D, C);

            return E;
        }

        public static Matrix MergeHorizontal(Matrix A, Matrix B, Matrix C, Matrix D)
        {
            Matrix E = MergeHorizontal(A, B);
            Matrix F = MergeHorizontal(C, D);
            Matrix G = MergeHorizontal(E, F);

            return G;
        }

        public static Matrix MergeHorizontal(Matrix A, Matrix B, Matrix C, Matrix D, Matrix E)
        {
            Matrix F = MergeHorizontal(A, B, C, D);
            Matrix G = MergeHorizontal(F, E);

            return G;
        }

        public static Matrix MergeHorizontal(Matrix A, Matrix B, Matrix C, Matrix D, Matrix E, Matrix F)
        {
            Matrix G = MergeHorizontal(A, B, C, D, E);
            Matrix H = MergeHorizontal(G, F);

            return H;
        }

        public static Matrix MergeHorizontal(Matrix A, Matrix B, Matrix C, Matrix D, Matrix E, Matrix F, Matrix G,
            Matrix H, Matrix I, Matrix J, Matrix K, Matrix L)
        {
            Matrix M = MergeHorizontal(A, B, C, D, E, F);
            Matrix N = MergeHorizontal(G, H, I, J, K, L);
            Matrix O = MergeHorizontal(M, N);

            return O;
        }

        public double[] VectorToDoubleArray()
        {
            if (cols != 1) throw new MException("Matrix is not vector");

            double[] a = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                a[i] = mat[i, 0];
            }

            return a;
        }

        public static string NormalizeMatrixString(string matStr)   // From Andy - thank you! :)
        {
            // Remove any multiple spaces
            while (matStr.IndexOf("  ") != -1)
                matStr = matStr.Replace("  ", " ");

            // Remove any spaces before or after newlines
            matStr = matStr.Replace(" \r\n", "\r\n");
            matStr = matStr.Replace("\r\n ", "\r\n");

            // If the data ends in a newline, remove the trailing newline.
            // Make it easier by first replacing \r\n’s with |’s then
            // restore the |’s with \r\n’s
            matStr = matStr.Replace("\r\n", "|");
            while (matStr.LastIndexOf("|") == (matStr.Length - 1))
                matStr = matStr.Substring(0, matStr.Length - 1);

            matStr = matStr.Replace("|", "\r\n");
            return matStr.Trim();
        }

        //   O P E R A T O R S

        public static Matrix operator -(Matrix m)
        { return Matrix.Multiply(-1, m); }

        public static Matrix operator +(Matrix m1, Matrix m2)
        { return Matrix.Add(m1, m2); }

        public static Matrix operator -(Matrix m1, Matrix m2)
        { return Matrix.Add(m1, -m2); }

        public static Matrix operator *(Matrix m1, Matrix m2)
        { return Matrix.StupidMultiply(m1, m2); }

        public static Matrix operator *(double n, Matrix m)
        { return Matrix.Multiply(n, m); }
    }

    //  The class for exceptions

    public class MException : Exception
    {
        public MException(string Message)
            : base(Message)
        { }
    }

}