#Region "Microsoft.VisualBasic::95822ea8aca69ecc6ea31923dd9e74f4, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\BasicMathematics.vb"

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

    '   Total Lines: 72
    '    Code Lines: 62
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 2.17 KB


    ' Class BasicMathematics
    ' 
    '     Function: ErrorOfSquareVs2, InnerProduct, (+2 Overloads) RootSumOfSquare, Stdev, SumOfSquare
    '               Var
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Public Class BasicMathematics

    Public Shared Function ErrorOfSquareVs2(x As Double(), y As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To x.Length - 1
            sum += std.Pow(x(i) - y(i), 2)
        Next
        Return sum
    End Function

    Public Shared Function Stdev(array As Double()) As Double
        Dim sum As Double = 0, mean As Double = 0
        For i = 0 To array.Length - 1
            sum += array(i)
        Next
        mean = sum / array.Length

        sum = 0
        For i = 0 To array.Length - 1
            sum += std.Pow(array(i) - mean, 2)
        Next
        Return std.Sqrt(sum / (array.Length - 1))
    End Function

    Public Shared Function SumOfSquare(array As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To array.Length - 1
            sum += std.Pow(array(i), 2)
        Next
        Return sum
    End Function

    Public Shared Function RootSumOfSquare(array1 As Double(), array2 As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To array1.Length - 1
            sum += std.Pow(array1(i) - array2(i), 2)
        Next
        Return std.Sqrt(sum)
    End Function

    Public Shared Function RootSumOfSquare(array As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To array.Length - 1
            sum += std.Pow(array(i), 2)
        Next
        Return std.Sqrt(sum)
    End Function

    Public Shared Function Var(array As Double()) As Double
        Dim sum As Double = 0, mean As Double = 0
        For i = 0 To array.Length - 1
            sum += array(i)
        Next
        mean = sum / array.Length

        sum = 0
        For i = 0 To array.Length - 1
            sum += std.Pow(array(i) - mean, 2)
        Next
        Return sum / (array.Length - 1)
    End Function

    Public Shared Function InnerProduct(array1 As Double(), array2 As Double()) As Double
        Dim sum As Double = 0
        For i = 0 To array1.Length - 1
            sum += array1(i) * array2(i)
        Next
        Return sum
    End Function
End Class
