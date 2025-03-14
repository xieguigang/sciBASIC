#Region "Microsoft.VisualBasic::a1dcc0bb14ca4351a8ee89d0f8e256d2, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\MDS\Data.vb"

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

    '   Total Lines: 211
    '    Code Lines: 137 (64.93%)
    ' Comment Lines: 51 (24.17%)
    '    - Xml Docs: 11.76%
    ' 
    '   Blank Lines: 23 (10.90%)
    '     File Size: 7.03 KB


    '     Class Data
    ' 
    '         Function: copyMatrix, landmarkIndices, normalize, prod
    ' 
    '         Sub: doubleCenter, eigen, multiply, normalize, randomize
    '              squareEntries
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports stdf = System.Math

' 
'  Copyright (C) 2014. Daniel Asarnow
' 
'  This program is free software: you can redistribute it and/or modify
'  it under the terms of the GNU General Public License as published by
'  the Free Software Foundation, either version 3 of the License, or
'  (at your option) any later version.
' 
'  This program is distributed in the hope that it will be useful,
'  but WITHOUT ANY WARRANTY; without even the implied warranty of
'  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'  GNU General Public License for more details.
' 
'  You should have received a copy of the GNU General Public License
'  along with this program.  If not, see <http://www.gnu.org/licenses/>.
' 

Namespace LinearAlgebra.Matrix.MDSScale

    ''' <summary>
    ''' Created by IntelliJ IDEA.
    ''' User: da
    ''' Date: 10/8/11
    ''' Time: 2:27 AM
    ''' </summary>
    Public Class Data

        Public Shared Sub doubleCenter(matrix As Double()())
            Dim n = matrix(0).Length
            Dim k = matrix.Length

            For j = 0 To k - 1
                Dim avg = 0.0R
                For i = 0 To n - 1
                    avg += matrix(j)(i)
                Next
                avg /= n
                For i = 0 To n - 1
                    matrix(j)(i) -= avg
                Next
            Next

            For i = 0 To n - 1
                Dim avg = 0.0R
                For j = 0 To k - 1
                    avg += matrix(j)(i)
                Next
                avg /= matrix.Length
                For j = 0 To k - 1
                    matrix(j)(i) -= avg
                Next
            Next
        End Sub

        Public Shared Sub multiply(matrix As Double()(), factor As Double)
            For i = 0 To matrix.Length - 1
                For j = 0 To matrix(0).Length - 1
                    matrix(i)(j) *= factor
                Next
            Next
        End Sub

        Public Shared Sub squareEntries(matrix As Double()())
            Dim n = matrix(0).Length
            Dim k = matrix.Length
            For i = 0 To k - 1
                For j = 0 To n - 1
                    matrix(i)(j) = stdf.Pow(matrix(i)(j), 2.0R)
                Next
            Next
        End Sub

        Public Shared Sub normalize(x As Double()())
            For i = 0 To x.Length - 1
                normalize(x(i))
            Next
        End Sub

        Public Shared Function normalize(x As Double()) As Double
            Dim norm = stdf.Sqrt(prod(x, x))
            For i = 0 To x.Length - 1
                x(i) /= norm
            Next
            Return norm
        End Function

        Public Shared Function prod(x As Double(), y As Double()) As Double
            Dim result = 0.0R
            Dim length = stdf.Min(x.Length, y.Length)
            For i = 0 To length - 1
                result += x(i) * y(i)
            Next
            Return result
        End Function

        ' public static void eigen(double[][] matrix, double[][] evecs, double[] evals) {
        ' double eps = 1.0E-06D;
        ' int maxiter = 100;
        ' int d = evals.length;
        ' int n = matrix.length;
        ' for (int m = 0; m < d; m++) {
        ' if (m > 0)
        ' for (int i = 0; i < n; i++)
        ' for (int j = 0; j < n; j++)
        ' matrix[i][j] -= evals[(m - 1)] * evecs[(m - 1)][i] * evecs[(m - 1)][j];
        ' for (int i = 0; i < n; i++)
        ' evecs[m][i] = Math.random();
        ' Data.normalize(evecs[m]);
        ' 
        ' double r = 0.0D;
        ' 
        ' for (int iter = 0; (Math.abs(1.0D - r) > 1.0E-06D) && (iter < 100); iter++) {
        ' double[] q = new double[n];
        ' for (int i = 0; i < n; i++) {
        ' for (int j = 0; j < n; j++)
        ' q[i] += matrix[i][j] * evecs[m][j];
        ' }
        ' evals[m] = Data.prod(evecs[m], q);
        ' Data.normalize(q);
        ' r = Math.abs(Data.prod(evecs[m], q));
        ' evecs[m] = q;
        ' }
        ' }
        ' }

        Public Shared Sub eigen(matrix As Double()(), evecs As Double()(), evals As Double())
            Dim d = evals.Length
            Dim k = matrix.Length
            Dim r = 0.0R

            For m = 0 To d - 1
                evals(m) = normalize(evecs(m))
            Next
            Dim iterations = 0
            While r < 0.99999R
                Dim tempOld = RectangularArray.Matrix(Of Double)(d, k)

                For m = 0 To d - 1
                    For i = 0 To k - 1
                        tempOld(m)(i) = evecs(m)(i)
                        evecs(m)(i) = 0.0R
                    Next
                Next

                For m = 0 To d - 1
                    For i = 0 To k - 1
                        For j = 0 To k - 1
                            evecs(m)(j) += matrix(i)(j) * tempOld(m)(i)
                        Next
                    Next
                Next
                For m = 0 To d - 1
                    For p = 0 To m - 1
                        Dim fac = prod(evecs(p), evecs(m)) / prod(evecs(p), evecs(p))
                        For i = 0 To k - 1
                            evecs(m)(i) -= fac * evecs(p)(i)
                        Next
                    Next
                Next

                For m = 0 To d - 1
                    evals(m) = normalize(evecs(m))
                Next
                r = 1.0R
                For m = 0 To d - 1
                    r = stdf.Min(stdf.Abs(prod(evecs(m), tempOld(m))), r)
                Next

                iterations += 1
            End While
        End Sub


        Public Shared Sub randomize(matrix As Double()())
            Dim random As Random = New Random()
            For i = 0 To matrix.Length - 1
                For j = 0 To matrix(0).Length - 1
                    matrix(i)(j) = random.NextDouble()
                Next
            Next
        End Sub

        Public Shared Function landmarkIndices(matrix As Double()()) As Integer()
            Dim k = matrix.Length
            Dim n = matrix(0).Length
            Dim result = New Integer(k - 1) {}
            For i = 0 To k - 1
                For j = 0 To n - 1
                    If matrix(i)(j) = 0.0R Then
                        result(i) = j
                    End If
                Next
            Next
            Return result
        End Function

        Public Shared Function copyMatrix(matrix As Double()()) As Double()()
            Dim copy = RectangularArray.Matrix(Of Double)(matrix.Length, matrix(0).Length)
            For i = 0 To matrix.Length - 1
                For j = 0 To matrix(0).Length - 1
                    copy(i)(j) = matrix(i)(j)
                Next
            Next
            Return copy
        End Function
    End Class

End Namespace
