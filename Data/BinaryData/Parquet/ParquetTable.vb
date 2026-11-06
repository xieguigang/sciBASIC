Option Strict On
Option Explicit On
Option Infer On

Imports System.Linq

''' <summary>
''' 从 Parquet 文件读取出来的列式数据表（未转换为 DataFrame 之前的中间模型）。
''' </summary>
Public Class ParquetTable

    Public Property Schema As Schema.ParquetSchema

    Public Property Columns As New List(Of Columns.DataColumn)

    Public Property NumRows As Long

    Public Property Metadata As Format.FileMetaData

    Public ReadOnly Property Leaves As List(Of Schema.ParquetSchemaNode)
        Get
            If Schema Is Nothing Then
                Return New List(Of Schema.ParquetSchemaNode)
            End If

            Return Schema.Leaves
        End Get
    End Property

    ''' <summary>
    ''' 按 <c>path_in_schema</c>（以 <c>.</c> 分隔）查找叶子列数据。
    ''' </summary>
    Public Function GetColumn(path As String) As Columns.DataColumn
        Dim names As String() = path.Split("."c)

        Return GetColumn(names)
    End Function

    Public Function GetColumn(path As IEnumerable(Of String)) As Columns.DataColumn
        Dim target As Schema.ParquetSchemaNode = Schema.FindLeaf(path)

        If target Is Nothing Then
            Return Nothing
        End If

        Return Columns.FirstOrDefault(Function(column) column.Node Is target)
    End Function

End Class
