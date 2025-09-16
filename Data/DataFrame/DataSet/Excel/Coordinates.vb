#Region "Microsoft.VisualBasic::ab9adf17f60ed78b542821dda166a83d, Data\DataFrame\DataSet\Excel\Coordinates.vb"

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

    '   Total Lines: 130
    '    Code Lines: 95 (73.08%)
    ' Comment Lines: 14 (10.77%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 21 (16.15%)
    '     File Size: 4.28 KB


    '     Module Coordinates
    ' 
    '         Function: CellValue, ColumnIndex, Dimension, Index, RangeSelects
    '                   Spans, YValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Table = Microsoft.VisualBasic.Data.Framework.IO.File

Namespace Excel

    Public Module Coordinates

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CellValue(data As IO.File, c As String) As String
            With Coordinates.Index(c)
                Return data.Cell(.X, .Y)
            End With
        End Function

        ReadOnly AZ As New Index(Of Char)("ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        ReadOnly ZERO% = Asc("A"c) - 1

        Public Function Index(c As String) As Point
            Dim Y As New List(Of Char)
            Dim X%

            For Each [char] As Char In c
                If AZ.IndexOf([char]) > -1 Then
                    Y.Add([char])
                Else
                    X = CInt(Val(c.Skip(Y.Count).CharString))
                    Exit For
                End If
            Next

            Return New Point(X, Y.YValue)
        End Function

        ''' <summary>
        ''' 也就是获取得到列的顶点编号
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function YValue(x As IEnumerable(Of Char)) As Integer
            Dim value#
            Dim power% = 0
            Dim offset As Integer = 0

            ' power = 0, offset = 0
            ' power > 0, offset = 1

            For Each c As Char In x.Reverse
                value += (Asc(c) - ZERO) + (26 * power - offset)
                power += 1
                offset = 1
            Next

            Return CInt(value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ColumnIndex(i%) As String
            Return i.TranslateDecimal(alphabets:=AZ.Objects)
        End Function

        <Extension> Public Function Dimension(table As Table) As String
            Dim allSpans = table _
                .Select(AddressOf Spans) _
                .ToArray
            Dim notEmptyRows%() = allSpans _
                .SeqIterator _
                .Where(Function(x)
                           Return x.value.start <> 0 AndAlso x.value.ends <> 0
                       End Function) _
                .Select(Function(x) x.i) _
                .ToArray
            Dim top = notEmptyRows.Min
            Dim left = Aggregate s In allSpans Where s.start > 0 Into Min(s.start)
            Dim bottom = notEmptyRows.Max
            Dim right = Aggregate s In allSpans Where s.ends > 0 Into Max(s.ends)

            left -= 1
            right -= 1
            top += 1
            bottom += 1

            Return $"{left.ColumnIndex}{top}:{right.ColumnIndex}{bottom}"
        End Function

        <Extension>
        Public Function RangeSelects(data As IO.File, range As String) As String()
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' 返回非空白列的起始列编号和结束列表号
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Spans(row As RowObject) As (start%, ends%)
            Dim start%, ends%

            If row.IsNullOrEmpty OrElse row.All(Function(s) s.StringEmpty) Then
                ' 空白行
                Return (0, 0)
            End If

            For i As Integer = 0 To row.NumbersOfColumn - 1
                If Not row(i).StringEmpty Then
                    start = i
                    Exit For
                End If
            Next

            For i As Integer = row.NumbersOfColumn - 1 To 0 Step -1
                If Not row(i).StringEmpty Then
                    ends = i
                    Exit For
                End If
            Next

            ' Excel之中的下表都是从1开始的
            Return (start + 1, ends + 1)
        End Function
    End Module
End Namespace
