Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO

Namespace Outlining

    Public Module OutliningDataLoader

        <Extension>
        Public Function LoadOutlining(Of T As Class)(filepath As String) As IEnumerable(Of T)
            Dim file As File = File.Load(filepath)
            ' 按照列空格进行文件的等级切割
            Dim levels As New List(Of List(Of RowObject))
            Dim indent As Integer

            For Each row As RowObject In file
                indent = row.RowIndentLevel
            Next
        End Function

        <Extension>
        Public Function RowIndentLevel(row As RowObject) As Integer
            For i As Integer = 0 To row.NumbersOfColumn - 1
                If Not row(i).StringEmpty(whitespaceAsEmpty:=False) Then
                    Return i
                End If
            Next

            ' 这个是一个空行？？
            Return -1
        End Function
    End Module
End Namespace
