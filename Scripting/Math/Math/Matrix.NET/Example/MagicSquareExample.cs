using System;
using DotNetMatrix;


namespace DotNetMatrix.examples
{
	
	/// <summary>Example of use of GeneralMatrix Class, featuring magic squares. *</summary>
	
	public class MagicSquareExample
	{
		
		/// <summary>Generate magic square test matrix. *</summary>

        public static DotNetMatrix.GeneralMatrix magic(int n)
		{
			
			double[][] M = new double[n][];
			for (int i = 0; i < n; i++)
			{
				M[i] = new double[n];
			}
			
			// Odd order
			
			if ((n % 2) == 1)
			{
				int a = (n + 1) / 2;
				int b = (n + 1);
				for (int j = 0; j < n; j++)
				{
					for (int i = 0; i < n; i++)
					{
						M[i][j] = n * ((i + j + a) % n) + ((i + 2 * j + b) % n) + 1;
					}
				}
				
				// Doubly Even Order
			}
			else if ((n % 4) == 0)
			{
				for (int j = 0; j < n; j++)
				{
					for (int i = 0; i < n; i++)
					{
						if (((i + 1) / 2) % 2 == ((j + 1) / 2) % 2)
						{
							M[i][j] = n * n - n * i - j;
						}
						else
						{
							M[i][j] = n * i + j + 1;
						}
					}
				}
				
				// Singly Even Order
			}
			else
			{
				int p = n / 2;
				int k = (n - 2) / 4;
				GeneralMatrix A = magic(p);
				for (int j = 0; j < p; j++)
				{
					for (int i = 0; i < p; i++)
					{
						double aij = A.GetElement(i, j);
						M[i][j] = aij;
						M[i][j + p] = aij + 2 * p * p;
						M[i + p][j] = aij + 3 * p * p;
						M[i + p][j + p] = aij + p * p;
					}
				}
				for (int i = 0; i < p; i++)
				{
					for (int j = 0; j < k; j++)
					{
						double t = M[i][j]; M[i][j] = M[i + p][j]; M[i + p][j] = t;
					}
					for (int j = n - k + 1; j < n; j++)
					{
						double t = M[i][j]; M[i][j] = M[i + p][j]; M[i + p][j] = t;
					}
				}
				double t2 = M[k][0]; M[k][0] = M[k + p][0]; M[k + p][0] = t2;
				t2 = M[k][k]; M[k][k] = M[k + p][k]; M[k + p][k] = t2;
			}
			return new GeneralMatrix(M);
		}
		
		/// <summary>Shorten spelling of print. *</summary>
		
		private static void  print(System.String s)
		{
			System.Console.Out.Write(s);
		}
		
		/// <summary>Format double with Fw.d. *</summary>
		
		public static System.String fixedWidthDoubletoString(double x, int w, int d)
		{
			System.String s = x.ToString("F" + d.ToString());
			while (s.Length < w)
			{
				s = " " + s;
			}
			return s;
		}
		
		/// <summary>Format integer with Iw. *</summary>
		
		public static System.String fixedWidthIntegertoString(int n, int w)
		{
			System.String s = System.Convert.ToString(n);
			while (s.Length < w)
			{
				s = " " + s;
			}
			return s;
		}
		
		
		[STAThread]
		public static void  Main(System.String[] argv)
		{
			
			/* 
			| Tests LU, QR, SVD and symmetric Eig decompositions.
			|
			|   n       = order of magic square.
			|   trace   = diagonal sum, should be the magic sum, (n^3 + n)/2.
			|   max_eig = maximum eigenvalue of (A + A')/2, should equal trace.
			|   rank    = linear algebraic rank,
			|             should equal n if n is odd, be less than n if n is even.
			|   cond    = L_2 condition number, ratio of singular values.
			|   lu_res  = test of LU factorization, norm1(L*U-A(p,:))/(n*eps).
			|   qr_res  = test of QR factorization, norm1(Q*R-A)/(n*eps).
			*/
			
			print("\n    Test of GeneralMatrix Class, using magic squares.\n");
			print("    See MagicSquareExample.main() for an explanation.\n");
			print("\n      n     trace       max_eig   rank        cond      lu_res      qr_res\n\n");
			
			System.DateTime start_time = System.DateTime.Now;
			double eps = System.Math.Pow(2.0, - 52.0);
			for (int n = 3; n <= 32; n++)
			{
				print(fixedWidthIntegertoString(n, 7));
				
				GeneralMatrix M = magic(n);
				
				//UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
				int t = (int) M.Trace();
				print(fixedWidthIntegertoString(t, 10));
				
				EigenvalueDecomposition E = new EigenvalueDecomposition(M.Add(M.Transpose()).Multiply(0.5));
				double[] d = E.RealEigenvalues;
				print(fixedWidthDoubletoString(d[n - 1], 14, 3));
				
				int r = M.Rank();
				print(fixedWidthIntegertoString(r, 7));
				
				double c = M.Condition();
				print(c < 1 / eps ? fixedWidthDoubletoString(c, 12, 3):"         Inf");
				
				LUDecomposition LU = new LUDecomposition(M);
				GeneralMatrix L = LU.L;
				GeneralMatrix U = LU.U;
				int[] p = LU.Pivot;
				GeneralMatrix R = L.Multiply(U).Subtract(M.GetMatrix(p, 0, n - 1));
				double res = R.Norm1() / (n * eps);
				print(fixedWidthDoubletoString(res, 12, 3));
				
				QRDecomposition QR = new QRDecomposition(M);
				GeneralMatrix Q = QR.Q;
				R = QR.R;
				R = Q.Multiply(R).Subtract(M);
				res = R.Norm1() / (n * eps);
				print(fixedWidthDoubletoString(res, 12, 3));
				
				print("\n");
			}

			System.DateTime stop_time = System.DateTime.Now;
			double etime = (stop_time.Ticks - start_time.Ticks) / 1000.0;
			print("\nElapsed Time = " + fixedWidthDoubletoString(etime, 12, 3) + " seconds\n");
			print("Adios\n");
		}
	}
}