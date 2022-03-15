#Region "Microsoft.VisualBasic::ad5abd35edd7dd1fb00a13590076af66, sciBASIC#\Data_science\Mathematica\Math\test\Matrix.NET\Example\MagicSquareExample.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 170
    '    Code Lines: 118
    ' Comment Lines: 21
    '   Blank Lines: 31
    '     File Size: 6.95 KB


    '     Class MagicSquareExample
    ' 
    '         Function: fixedWidthDoubletoString, fixedWidthIntegertoString, magic
    ' 
    '         Sub: Main, print
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Matrix

Namespace DotNetMatrix.examples

    ''' <summary>Example of use of GeneralMatrix Class, featuring magic squares. *</summary>

    Public Class MagicSquareExample

        ''' <summary>Generate magic square test matrix. *</summary>

        Public Shared Function magic(n As Integer) As GeneralMatrix

            Dim M As Double()() = New Double(n - 1)() {}
            For i As Integer = 0 To n - 1
                M(i) = New Double(n - 1) {}
            Next

            ' Odd order

            If (n Mod 2) = 1 Then
                Dim a__1 As Integer = (n + 1) \ 2
                Dim b As Integer = (n + 1)
                For j As Integer = 0 To n - 1
                    For i As Integer = 0 To n - 1
                        M(i)(j) = n * ((i + j + a__1) Mod n) + ((i + 2 * j + b) Mod n) + 1
                    Next

                    ' Doubly Even Order
                Next
            ElseIf (n Mod 4) = 0 Then
                For j As Integer = 0 To n - 1
                    For i As Integer = 0 To n - 1
                        If ((i + 1) \ 2) Mod 2 = ((j + 1) \ 2) Mod 2 Then
                            M(i)(j) = n * n - n * i - j
                        Else
                            M(i)(j) = n * i + j + 1
                        End If
                    Next

                    ' Singly Even Order
                Next
            Else
                Dim p As Integer = n \ 2
                Dim k As Integer = (n - 2) \ 4
                Dim A__2 As GeneralMatrix = magic(p)
                For j As Integer = 0 To p - 1
                    For i As Integer = 0 To p - 1
                        Dim aij As Double = A__2(i, j)
                        M(i)(j) = aij
                        M(i)(j + p) = aij + 2 * p * p
                        M(i + p)(j) = aij + 3 * p * p
                        M(i + p)(j + p) = aij + p * p
                    Next
                Next
                For i As Integer = 0 To p - 1
                    For j As Integer = 0 To k - 1
                        Dim t As Double = M(i)(j)
                        M(i)(j) = M(i + p)(j)
                        M(i + p)(j) = t
                    Next
                    For j As Integer = n - k + 1 To n - 1
                        Dim t As Double = M(i)(j)
                        M(i)(j) = M(i + p)(j)
                        M(i + p)(j) = t
                    Next
                Next
                Dim t2 As Double = M(k)(0)
                M(k)(0) = M(k + p)(0)
                M(k + p)(0) = t2
                t2 = M(k)(k)
                M(k)(k) = M(k + p)(k)
                M(k + p)(k) = t2
            End If
            Return New GeneralMatrix(M)
        End Function

        ''' <summary>Shorten spelling of print. *</summary>

        Private Shared Sub print(s As System.String)
            System.Console.Out.Write(s)
        End Sub

        ''' <summary>Format double with Fw.d. *</summary>

        Public Shared Function fixedWidthDoubletoString(x As Double, w As Integer, d As Integer) As System.String
            Dim s As System.String = x.ToString("F" & d.ToString())
            While s.Length < w
                s = " " & s
            End While
            Return s
        End Function

        ''' <summary>Format integer with Iw. *</summary>

        Public Shared Function fixedWidthIntegertoString(n As Integer, w As Integer) As System.String
            Dim s As System.String = System.Convert.ToString(n)
            While s.Length < w
                s = " " & s
            End While
            Return s
        End Function


        <STAThread>
        Public Shared Sub Main(argv As System.String())

            ' 
            '			| Tests LU, QR, SVD and symmetric Eig decompositions.
            '			|
            '			|   n       = order of magic square.
            '			|   trace   = diagonal sum, should be the magic sum, (n^3 + n)/2.
            '			|   max_eig = maximum eigenvalue of (A + A')/2, should equal trace.
            '			|   rank    = linear algebraic rank,
            '			|             should equal n if n is odd, be less than n if n is even.
            '			|   cond    = L_2 condition number, ratio of singular values.
            '			|   lu_res  = test of LU factorization, norm1(L*U-A(p,:))/(n*eps).
            '			|   qr_res  = test of QR factorization, norm1(Q*R-A)/(n*eps).
            '			


            print(vbLf & "    Test of GeneralMatrix Class, using magic squares." & vbLf)
            print("    See MagicSquareExample.main() for an explanation." & vbLf)
            print(vbLf & "      n     trace       max_eig   rank        cond      lu_res      qr_res" & vbLf & vbLf)

            Dim start_time As System.DateTime = System.DateTime.Now
            Dim eps As Double = System.Math.Pow(2.0, -52.0)
            For n As Integer = 3 To 32
                print(fixedWidthIntegertoString(n, 7))

                Dim M As GeneralMatrix = magic(n)

                'UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
                Dim t As Integer = CInt(M.Trace())
                print(fixedWidthIntegertoString(t, 10))

                Dim E As New EigenvalueDecomposition(M.Add(M.Transpose()).Multiply(0.5))
                Dim d As Double() = E.RealEigenvalues
                print(fixedWidthDoubletoString(d(n - 1), 14, 3))

                Dim r__1 As Integer = M.Rank()
                print(fixedWidthIntegertoString(r__1, 7))

                Dim c As Double = M.Condition()
                print(If(c < 1 / eps, fixedWidthDoubletoString(c, 12, 3), "         Inf"))

                Dim LU As New LUDecomposition(M)
                Dim L As GeneralMatrix = LU.L
                Dim U As GeneralMatrix = LU.U
                Dim p As Integer() = LU.Pivot
                Dim R__2 As GeneralMatrix = L.Multiply(U).Subtract(M.GetMatrix(p, 0, n - 1))
                Dim res As Double = R__2.Norm1() / (n * eps)
                print(fixedWidthDoubletoString(res, 12, 3))

                Dim QR As New QRDecomposition(M)
                Dim Q As GeneralMatrix = QR.Q
                R__2 = QR.R
                R__2 = Q.Multiply(R__2).Subtract(M)
                res = R__2.Norm1() / (n * eps)
                print(fixedWidthDoubletoString(res, 12, 3))

                print(vbLf)
            Next

            Dim stop_time As System.DateTime = System.DateTime.Now
            Dim etime As Double = (stop_time.Ticks - start_time.Ticks) / 1000.0
            print(vbLf & "Elapsed Time = " & fixedWidthDoubletoString(etime, 12, 3) & " seconds" & vbLf)
            print("Adios" & vbLf)
        End Sub
    End Class
End Namespace
