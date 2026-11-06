Option Strict On
Option Explicit On
Option Infer On

Namespace Columns

    ''' <summary>
    ''' 一个<b>叶子列</b>（物理列）的数据：定义级别 / 重复级别 / 已定义数值。
    ''' <para>
    ''' <c>DefinitionLevels.Length</c> 表示该列在当前数据块中的数值槽位数量（包含 null 槽位），
    ''' <c>Values</c> 只包含定义级别达到 <c>Node.DefinitionLevel</c> 的那些数值。
    ''' </para>
    ''' </summary>
    Public Class DataColumn

        Public Property Node As Schema.ParquetSchemaNode

        Public Property RepetitionLevels As Integer()

        Public Property DefinitionLevels As Integer()

        Public Property Values As Array

        ''' <summary>
        ''' 数值槽位数量（包含 null 槽位）。当定义级别数据被省略（max_definition_level = 0）时，
        ''' 需要显式记录槽位数量。
        ''' </summary>
        Public Property NumSlots As Integer

        ''' <summary>
        ''' 该列的最大定义级别。
        ''' </summary>
        Public ReadOnly Property MaxDefinitionLevel As Integer
            Get
                Return Node.DefinitionLevel
            End Get
        End Property

        Public ReadOnly Property MaxRepetitionLevel As Integer
            Get
                Return Node.RepetitionLevel
            End Get
        End Property

        ''' <summary>
        ''' 数值槽位数量（含 null 槽位）。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                If NumSlots > 0 Then
                    Return NumSlots
                End If

                Return If(DefinitionLevels Is Nothing, 0, DefinitionLevels.Length)
            End Get
        End Property

        Public ReadOnly Property DefinedCount As Integer
            Get
                Return If(Values Is Nothing, 0, Values.Length)
            End Get
        End Property

    End Class

    ''' <summary>
    ''' 写入过程中用于累积某个叶子列数据的可变缓冲区。
    ''' </summary>
    Public Class ColumnBuffer

        Public Property Node As Schema.ParquetSchemaNode

        Public ReadOnly Property RepetitionLevels As New List(Of Integer)

        Public ReadOnly Property DefinitionLevels As New List(Of Integer)

        Public ReadOnly Property Values As New List(Of Object)

        Public Sub New(node As Schema.ParquetSchemaNode)
            Me.Node = node
        End Sub

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return DefinitionLevels.Count = 0
            End Get
        End Property

        Public Sub Add(rep As Integer, def As Integer, value As Object)
            Call RepetitionLevels.Add(rep)
            Call DefinitionLevels.Add(def)

            If def >= Node.DefinitionLevel Then
                Call Values.Add(value)
            End If
        End Sub

        Public Function ToDataColumn() As DataColumn
            Return New DataColumn With {
                .Node = Node,
                .NumSlots = DefinitionLevels.Count,
                .RepetitionLevels = RepetitionLevels.ToArray,
                .DefinitionLevels = DefinitionLevels.ToArray,
                .Values = ValueCast.ToTypedArray(Node, Values)
            }
        End Function

    End Class

    ''' <summary>
    ''' 数值类型转换辅助函数。
    ''' </summary>
    Public Module ValueCast

        ''' <summary>
        ''' 将装箱的数值列表转换为列物理类型对应的强类型数组。
        ''' </summary>
        Public Function ToTypedArray(node As Schema.ParquetSchemaNode, values As List(Of Object)) As Array
            Select Case node.PhysicalType.Value
                Case Format.ParquetType.[BOOLEAN]
                    Dim data As Boolean() = New Boolean(values.Count - 1) {}

                    For i As Integer = 0 To values.Count - 1
                        data(i) = Convert.ToBoolean(values(i))
                    Next

                    Return data
                Case Format.ParquetType.INT32
                    Dim data As Integer() = New Integer(values.Count - 1) {}

                    For i As Integer = 0 To values.Count - 1
                        data(i) = Convert.ToInt32(values(i))
                    Next

                    Return data
                Case Format.ParquetType.INT64
                    Dim data As Long() = New Long(values.Count - 1) {}

                    For i As Integer = 0 To values.Count - 1
                        data(i) = Convert.ToInt64(values(i))
                    Next

                    Return data
                Case Format.ParquetType.FLOAT
                    Dim data As Single() = New Single(values.Count - 1) {}

                    For i As Integer = 0 To values.Count - 1
                        data(i) = Convert.ToSingle(values(i))
                    Next

                    Return data
                Case Format.ParquetType.[DOUBLE]
                    Dim data As Double() = New Double(values.Count - 1) {}

                    For i As Integer = 0 To values.Count - 1
                        data(i) = Convert.ToDouble(values(i))
                    Next

                    Return data
                Case Else
                    Dim data As Byte()() = New Byte(values.Count - 1)() {}

                    For i As Integer = 0 To values.Count - 1
                        data(i) = CType(values(i), Byte())
                    Next

                    Return data
            End Select
        End Function

        ''' <summary>
        ''' 将数值转换为 DataFrame 中使用的 CLR 类型。
        ''' </summary>
        Public Function ToClrValue(type As Format.ParquetType, value As Object) As Object
            Select Case type
                Case Format.ParquetType.[BOOLEAN]
                    Return Convert.ToBoolean(value)
                Case Format.ParquetType.INT32
                    Return Convert.ToInt32(value)
                Case Format.ParquetType.INT64
                    Return Convert.ToInt64(value)
                Case Format.ParquetType.FLOAT
                    Return Convert.ToSingle(value)
                Case Format.ParquetType.[DOUBLE]
                    Return Convert.ToDouble(value)
                Case Else
                    Return value
            End Select
        End Function

    End Module

End Namespace
