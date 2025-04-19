#Region "Microsoft.VisualBasic::b53e5a16b90f3215ce3108640938caa8, Data_science\Mathematica\Math\DataFittings\LASSO\MathUtil.vb"

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

    '   Total Lines: 79
    '    Code Lines: 59 (74.68%)
    ' Comment Lines: 7 (8.86%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 13 (16.46%)
    '     File Size: 2.68 KB


    '     Class MathUtil
    ' 
    '         Function: allocateDoubleMatrix, (+2 Overloads) getDotProduct, getFormattedDouble, (+2 Overloads) getStd, (+2 Overloads) getStg
    ' 
    '         Sub: divideInPlace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace LASSO

    ''' <summary>
    ''' Utility Math functions that are used by other classes.
    ''' 
    ''' @author Yasser Ganjisaffar (http://www.ics.uci.edu/~yganjisa/)
    ''' 
    ''' </summary>
    Public Class MathUtil


        Public Shared Function getStg(arr As Double()) As Double
            Return getStd(arr, arr.Average())
        End Function

        Public Shared Function getStg(arr As IList(Of Double)) As Double
            Return getStd(arr, arr.Average())
        End Function

        Public Shared Function getStd(arr As Double(), avg As Double) As Double
            Dim sum As Double = 0
            For Each item In arr
                sum += std.Pow(item - avg, 2)
            Next
            Return std.Sqrt(sum / arr.Length)
        End Function

        Public Shared Function getStd(arr As IList(Of Double), avg As Double) As Double
            Dim sum As Double = 0
            For Each item In arr
                sum += std.Pow(item - avg, 2)
            Next
            Return std.Sqrt(sum / arr.Count)
        End Function

        Public Shared Function getDotProduct(vector1 As Single(), vector2 As Single(), length As Integer) As Double
            Dim product As Double = 0
            For i = 0 To length - 1
                product += vector1(i) * vector2(i)
            Next
            Return product
        End Function

        Public Shared Function getDotProduct(vector1 As Double(), vector2 As Double(), length As Integer) As Double
            Dim product As Double = 0
            For i = 0 To length - 1
                product += vector1(i) * vector2(i)
            Next
            Return product
        End Function

        ' Divides the second vector from the first one (vector1[i] /= val)
        Public Shared Sub divideInPlace(ByRef vector As Double(), val As Single)
            Dim length = vector.Length
            For i = 0 To length - 1
                vector(i) /= val
            Next
        End Sub

        Public Shared Function allocateDoubleMatrix(m As Integer, n As Integer) As Double()()
            Dim mat = New Double(m - 1)() {}
            For i = 0 To m - 1
                mat(i) = New Double(n - 1) {}
            Next
            Return mat
        End Function

        Public Shared Function getFormattedDouble(val As Double, decimalPoints As Integer) As String
            Dim format = "#."
            For i = 0 To decimalPoints - 1
                format += "#"
            Next
            Return val.ToString(format)
        End Function
    End Class

End Namespace

