#Region "Microsoft.VisualBasic::bdca02441610c315bcd242daa7f51611, Data\DataFrame\DATA\Excel\Coordinates.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Table = Microsoft.VisualBasic.Data.csv.IO.File

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

        Public Function Index(c$) As Point
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function ColumnIndex(i%) As String
            Return i.TranslateDecimal(alphas:=AZ.Objects)
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

        ''' <summary>
        ''' 也就是获取得到列的顶点编号
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension> Public Function YValue(x As IEnumerable(Of Char)) As Integer
            Dim value#
            Dim power% = 0

            For Each c In x.Reverse
                value += (Asc(c) - ZERO) * (26 ^ power)
            Next

            Return CInt(value)
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
