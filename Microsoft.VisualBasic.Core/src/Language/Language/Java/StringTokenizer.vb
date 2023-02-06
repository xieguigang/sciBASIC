Imports System.Runtime.CompilerServices

Namespace Language.Java

    Public Class StringTokenizer

        ReadOnly rawText As String
        ReadOnly sep As String
        ReadOnly tokens As String()

        Dim i As i32 = 0

        Public ReadOnly Property countTokens As Integer
            Get
                Return tokens.Length
            End Get
        End Property

        Public Sub New(text As String, sep As String)
            Me.rawText = text
            Me.sep = sep
            Me.tokens = text.StringSplit(sep)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function nextToken() As String
            Return tokens.ElementAtOrDefault(++i)
        End Function

        Public Overrides Function ToString() As String
            Return $"{countTokens} tokens from '{rawText}'"
        End Function
    End Class
End Namespace