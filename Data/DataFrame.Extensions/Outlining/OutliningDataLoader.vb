Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports TableSchema = Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider

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
        Public Iterator Function LoadOutlining(Of T As Class)(filepath$,
                                                              Optional strict As Boolean = False,
                                                              Optional ignoresBlankRow As Boolean = False) As IEnumerable(Of T)
            Dim file As File = File.Load(filepath)
            ' 按照列空格进行文件的等级切割
            Dim indent As Integer
            Dim currentIndent As Integer = -1
            Dim buffer As New List(Of RowObject)
            Dim schema As TableSchema = TableSchema.CreateObject(GetType(T), strict).CopyWriteDataToObject
            Dim rowBuilder As New RowBuilder(schema)



            Call rowBuilder.IndexOf(csv)
            Call rowBuilder.SolveReadOnlyMetaConflicts()

            ' 顺序需要一一对应，所以在最后这里进行了一下排序操作
            Dim LQuery = From item
                         In buf.Populate(Parallel)
                         Select item.lineNumber,
                             item.row,
                             data = rowBuilder.FillData(item.row, item.filledObject, metaBlank)
                         Order By lineNumber Ascending

            Return LQuery.Select(Function(x) x.data)

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


                    Else
                        buffer += row
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
