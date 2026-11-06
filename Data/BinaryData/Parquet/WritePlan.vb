Option Strict On
Option Explicit On
Option Infer On

''' <summary>
''' 顶层列在内存中的形态。
''' </summary>
Public Enum ColumnKind

    ''' <summary>
    ''' 单层叶子列，直接对应一个物理列。
    ''' </summary>
    Leaf = 0

    ''' <summary>
    ''' 嵌套列（JSON 文本），写入时需要按 schema 进行 shredding。
    ''' </summary>
    Nested = 1

End Enum

''' <summary>
''' 一个顶层列的写入计划。
''' </summary>
Public Class ColumnPlan

    Public Property Name As String

    Public Property Kind As ColumnKind = ColumnKind.Leaf

    ''' <summary>
    ''' 该列在 schema 树中的顶层节点。
    ''' </summary>
    Public Property Node As Schema.ParquetSchemaNode

    ''' <summary>
    ''' 叶子列的强类型数组（长度与行数一致）。
    ''' </summary>
    Public Property Values As Array

    ''' <summary>
    ''' 叶子列的空值掩码，<c>Nothing</c> 表示没有空值。
    ''' </summary>
    Public Property NullMask As Boolean()

    ''' <summary>
    ''' 嵌套列每一行的 JSON 文本，<c>Nothing</c> 表示空值。
    ''' </summary>
    Public Property JsonValues As String()

    ''' <summary>
    ''' 嵌套列的类型描述（写入到 <c>FeatureVector.attributes</c> 以便读取时还原）。
    ''' </summary>
    Public Property TypeDefinition As String

End Class

''' <summary>
''' 完整的写入计划：schema 树 + 顶层列数据 + 行数。
''' </summary>
Public Class WritePlan

    Public Property Root As Schema.ParquetSchemaNode

    Public Property Columns As New List(Of ColumnPlan)

    Public Property RowCount As Integer

    Public ReadOnly Property LeafCount As Integer
        Get
            Dim count As Integer = 0

            For Each column As ColumnPlan In Columns
                If column.Kind = ColumnKind.Leaf Then
                    count += 1
                Else
                    count += column.Node.Leaves().Count
                End If
            Next

            Return count
        End Get
    End Property

End Class
