#Region "Microsoft.VisualBasic::967bb3e4808535d72913e773d1d5004f, Microsoft.VisualBasic.Core\Extensions\Math\StatisticsMathExtensions\Linq\EnumerableStatsMode.vb"

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
    End Module
End Namespace
