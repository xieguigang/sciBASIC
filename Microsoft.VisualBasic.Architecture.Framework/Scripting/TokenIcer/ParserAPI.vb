Imports System.Runtime.CompilerServices

Namespace Scripting.TokenIcer

    Public Module ParserAPI

        <Extension>
        Public Function TokenParser(Of Tokens)(parser As TokenParser(Of Tokens),
                                               expr As String,
                                               stackT As StackTokens(Of Tokens)) As Func(Of Tokens)

            Dim lstToken As New List(Of Token(Of Tokens))
            Dim tmp As Token(Of Tokens) = Nothing

            parser.InputString = expr
            Do While Not parser.GetToken.ShadowCopy(tmp) Is Nothing
                Call lstToken.Add(tmp)
            Loop

            Dim func As Func(Of Tokens) =
                StackParser.Parsing(Of Tokens)(lstToken, stackT)
            Return func
        End Function
    End Module
End Namespace