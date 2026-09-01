#Region "Microsoft.VisualBasic::967bb3e4808535d72913e773d1d5004f, Microsoft.VisualBasic.Core\src\Extensions\Math\StatisticsMathExtensions\Linq\EnumerableStatsMode.vb"

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

    '   Total Lines: 81
    '    Code Lines: 65 (80.25%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (19.75%)
    '     File Size: 2.74 KB


    '     Module EnumerableStatsMode
    ' 
    '         Function: (+4 Overloads) Mode, (+2 Overloads) Modes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Math.Statistics.Linq

    Public Module EnumerableStatsMode

        <Extension>
        Public Function Modes(Of T As Structure)(source As IEnumerable(Of T?)) As IEnumerable(Of T)
            Dim values As IEnumerable(Of T) = source.Coalesce()
            If values.Any() Then
                Return values.Modes
            End If

            Return Enumerable.Empty(Of T)()
        End Function

        <Extension>
        Public Function Modes(Of T As Structure)(source As IEnumerable(Of T)) As IEnumerable(Of T)
            Dim modes__1 As New List(Of T)()

            Dim current As IEnumerable(Of T) = source
            Dim mode As T? = current.Mode
            While mode.HasValue AndAlso current.Count() > 1
                modes__1.Add(CType(mode, T))
                current = current.Where(Function(x) x.Equals(mode) = False).ToArray()
                mode = current.Mode
            End While
            Return modes__1
        End Function

        <Extension>
        Public Function Mode(Of T As Structure)(source As IEnumerable(Of T?)) As T
            Dim values As IEnumerable(Of T) = source.Coalesce()
            If values.Any() Then
                Return values.Mode
            End If

            Return Nothing
        End Function

        <Extension>
        Public Function Mode(Of T As Structure)(source As IEnumerable(Of T)) As T
            Dim sortedList = From number In source Order By number Select number

            Dim count As Integer = 0
            Dim max As Integer = 0
            Dim current As T = Nothing
            Dim mode__1 As New T?()

            For Each [next] As T In sortedList
                If current.Equals([next]) = False Then
                    current = [next]
                    count = 1
                Else
                    count += 1
                End If

                If count > max Then
                    max = count
                    mode__1 = current
                End If
            Next

            If max > 1 Then
                Return mode__1
            End If

            Return Nothing
        End Function

        <Extension>
        Public Function Mode(Of TSource, TMode As Structure)(source As IEnumerable(Of TSource), selector As Func(Of TSource, TMode)) As TMode
            Return source.[Select](selector).Mode
        End Function

        <Extension>
        Public Function Mode(Of TSource, TMode As Structure)(source As IEnumerable(Of TSource), selector As Func(Of TSource, TMode?)) As TMode
            Return source.[Select](selector).Mode
        End Function

        ''' <summary>
        ''' 计算出众数
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function EvaluateMode(data As IReadOnlyCollection(Of Double)) As Double
            If data Is Nothing OrElse data.Count = 0 Then Return Double.NaN
            If data.Count = 1 Then Return data(0)

            ' data 必须为已排序数组！
            Dim modeValue As Double = data(0)
            Dim modeCount As Integer = 1
            Dim currValue As Double = data(0)
            Dim currCount As Integer = 1

            ' Count the amount of repeat And update mode variables
            For i As Integer = 1 To data.Count - 1
                If data(i) = currValue Then
                    currCount += 1
                Else
                    ' 修正：使用 > 而不是 >=，确保在多个值频次相同时保留最先出现的那个值
                    If currCount > modeCount Then
                        modeCount = currCount
                        modeValue = currValue
                    End If

                    currValue = data(i)
                    currCount = 1
                End If
            Next

            ' Check the last count
            If currCount > modeCount Then
                modeValue = currValue
            End If

            ' 如果所有值都只出现一次(没有重复)，众数概念上无意义，这里返回第一个元素
            If modeCount = 1 Then
                Return data(0)
            Else
                Return modeValue
            End If
        End Function
    End Module
End Namespace
