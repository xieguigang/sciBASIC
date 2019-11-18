Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO

Namespace Outlining

    Public Module OutliningDataLoader

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="filepath"></param>
        ''' <param name="ignoresBlankRow">
        ''' 如果遇到空白行是抛出错误还是忽略掉该空白行？默认是不忽略掉该空白行，即抛出错误
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function LoadOutlining(Of T As Class)(filepath As String, Optional ignoresBlankRow As Boolean = False) As IEnumerable(Of T)
            Dim file As File = File.Load(filepath)
            ' 按照列空格进行文件的等级切割
            Dim levels As New List(Of List(Of RowObject))
            Dim indent As Integer
            Dim currentIndent As Integer = -1

            For Each row As RowObject In file
                indent = row.RowIndentLevel

                If indent < 0 Then
                    If ignoresBlankRow Then
                        Continue For
                    Else
                        Throw New DataException($"Row blank!")
                    End If
                Else
                    If currentIndent <> indent Then
                        currentIndent = indent

                        If levels.Count <= indent Then
                            levels.Add(New List(Of RowObject))
                            levels(indent).Add(row)
                        ElseIf currentIndent = 0 Then
                            ' add top level row

                        Else
                            ' ignores headers
                        End If
                    Else
                        levels(indent).Add(row)
                    End If
                End If
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
