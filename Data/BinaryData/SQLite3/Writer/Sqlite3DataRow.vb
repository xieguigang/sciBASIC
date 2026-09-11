Imports System.Collections.Generic

Namespace Writer

    ''' <summary>
    ''' 写入模块的内存数据行, 取值语义与读取侧 <c>Sqlite3Row</c> 保持一致(可按序号或列名访问)。
    ''' </summary>
    Public Class Sqlite3DataRow

        Public ReadOnly Property Table As Sqlite3TableWriter
        Public ReadOnly Property RowId As Long
        Public ReadOnly Property Values As Object()

        Friend Sub New(table As Sqlite3TableWriter, rowId As Long, values As Object())
            Me.Table = table
            Me.RowId = rowId
            Me.Values = values
        End Sub

        Default Public ReadOnly Property Item(index As Integer) As Object
            Get
                If Values Is Nothing OrElse index < 0 OrElse index >= Values.Length Then
                    Return Nothing
                End If

                Return Values(index)
            End Get
        End Property

        Default Public ReadOnly Property Item(name As String) As Object
            Get
                Return Me(Table.GetOrdinal(name))
            End Get
        End Property

        Public Function IsDBNull(index As Integer) As Boolean
            Return Me(index) Is Nothing
        End Function

        Public Overrides Function ToString() As String
            Dim parts As New List(Of String)()

            For Each v As Object In Values
                parts.Add(If(v Is Nothing, "<NULL>", v.ToString()))
            Next

            Return "#" & RowId & " = [" & String.Join(", ", parts) & "]"
        End Function

    End Class

End Namespace
