Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language.Default

Namespace HTML

    Public Structure ValueAttribute : Implements INamedValue, IsEmpty

        Public Property Name As String Implements INamedValue.Key
        Public Property Values As List(Of String)

        Public ReadOnly Property IsEmpty As Boolean Implements IsEmpty.IsEmpty
            Get
                Return Name.StringEmpty AndAlso Values.IsNullOrEmpty
            End Get
        End Property

        Public ReadOnly Property Value As String
            Get
                Return Values.FirstOrDefault
            End Get
        End Property

        Sub New(strText As String)
            Dim ep As Integer = InStr(strText, "=")
            Name = Mid(strText, 1, ep - 1)
            Dim Value = Mid(strText, ep + 1)
            If Value.First = """"c AndAlso Value.Last = """"c Then
                Value = Mid(Value, 2, Len(Value) - 2)
            End If

            Values = New List(Of String) From {Value}
        End Sub

        Sub New(name As String, value As String)
            Me.Name = name
            Me.Values = New List(Of String) From {value}
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name}={Values.Select(Function(v) $"""{v}""").JoinBy(", ")}"
        End Function
    End Structure

End Namespace