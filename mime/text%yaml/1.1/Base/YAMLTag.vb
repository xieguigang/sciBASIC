Imports System.Diagnostics.Contracts

Namespace Grammar11

    Public Structure YAMLTag

        Public Sub New(handle As String, content As String)
            Me.Handle = handle
            Me.Content = content
        End Sub

        Public Overrides Function ToString() As String
            Return If(IsEmpty, String.Empty, $"{Handle}{Content}")
        End Function

        <Pure>
        Public Function ToHeaderString() As String
            Return If(IsEmpty, String.Empty, $"{Handle} {Content}")
        End Function

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return String.IsNullOrEmpty(Handle)
            End Get
        End Property

        Public ReadOnly Property Handle() As String
        Public ReadOnly Property Content() As String
    End Structure
End Namespace
