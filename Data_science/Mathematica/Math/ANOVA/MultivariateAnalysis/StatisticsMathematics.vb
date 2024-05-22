#Region "Microsoft.VisualBasic::706d006b1529215048ca5ff41870f7f3, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\StatisticsMathematics.vb"

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

    '   Total Lines: 136
    '    Code Lines: 111 (81.62%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 25 (18.38%)
    '     File Size: 4.50 KB


    ' Class StatisticsMathematics
    ' 
    '     Function: AutoScaling, LogTransform, MeanCentering, ParetoScaling, QuadRootTransform
    ' 
    '     Sub: (+2 Overloads) StatisticsProperties
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports std = System.Math

Public Class StatisticsMathematics

    Public Shared Function LogTransform(dataArray As Double(,)) As Double(,)
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To rowSize - 1
            For j = 0 To columnSize - 1
                If dataArray(i, j) > 0 Then
                    resultArray(i, j) = std.Log10(dataArray(i, j))
                Else
                    resultArray(i, j) = 0
                End If
            Next
        Next
        Return resultArray
    End Function

    Public Shared Function QuadRootTransform(dataArray As Double(,)) As Double(,)
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To rowSize - 1
            For j = 0 To columnSize - 1
                If dataArray(i, j) > 0 Then
                    resultArray(i, j) = std.Sqrt(std.Sqrt(dataArray(i, j)))
                Else
                    resultArray(i, j) = 0
                End If
            Next
        Next
        Return resultArray
    End Function

    Public Shared Function MeanCentering(dataArray As Double(,), mean As Double()) As Double(,)

        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}

        For i = 0 To columnSize - 1
            For j = 0 To rowSize - 1
                resultArray(j, i) = dataArray(j, i) - mean(i)
            Next
        Next

        Return resultArray
    End Function

    Public Shared Function ParetoScaling(dataArray As Double(,), mean As Double(), stdev As Double()) As Double(,)
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To columnSize - 1
            For j = 0 To rowSize - 1
                If stdev(i) <> 0 Then
                    resultArray(j, i) = (dataArray(j, i) - mean(i)) / std.Sqrt(stdev(i))
                Else
                    resultArray(j, i) = dataArray(j, i) - mean(i)
                End If
            Next
        Next

        Return resultArray
    End Function

    Public Shared Function AutoScaling(dataArray As Double(,), mean As Double(), stdev As Double()) As Double(,)
        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        Dim resultArray = New Double(rowSize - 1, columnSize - 1) {}
        For i = 0 To columnSize - 1
            For j = 0 To rowSize - 1
                If stdev(i) <> 0 Then
                    resultArray(j, i) = (dataArray(j, i) - mean(i)) / stdev(i)
                Else
                    resultArray(j, i) = dataArray(j, i) - mean(i)
                End If

            Next
        Next

        Return resultArray
    End Function

    Public Shared Sub StatisticsProperties(dataArray As Double(,), <Out> ByRef mean As Double(), <Out> ByRef stdev As Double())

        Dim rowSize = dataArray.GetLength(0)
        Dim columnSize = dataArray.GetLength(1)

        mean = New Double(columnSize - 1) {}
        stdev = New Double(columnSize - 1) {}
        Dim sum As Double

        For i = 0 To columnSize - 1
            sum = 0
            For j = 0 To rowSize - 1
                sum += dataArray(j, i)
            Next
            mean(i) = sum / rowSize
        Next

        For i = 0 To columnSize - 1
            sum = 0
            For j = 0 To rowSize - 1
                sum += std.Pow(dataArray(j, i) - mean(i), 2)
            Next
            stdev(i) = std.Sqrt(sum / (rowSize - 1))
        Next
    End Sub

    Public Shared Sub StatisticsProperties(dataArray As Double(), <Out> ByRef mean As Double, <Out> ByRef stdev As Double)
        Dim size = dataArray.Length

        mean = 0.0
        stdev = 0.0
        Dim sum = 0.0
        For i = 0 To size - 1
            sum += dataArray(i)
        Next
        mean = sum / size

        sum = 0.0
        For i = 0 To size - 1
            sum += std.Pow(dataArray(i) - mean, 2)
        Next
        stdev = std.Sqrt(sum / (size - 1))
    End Sub
End Class
