Imports System.Runtime.CompilerServices

Namespace Scripting.TokenIcer

    Public Module ParserAPI

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <param name="parser"></param>
        ''' <param name="expr">表达式字符串</param>
        ''' <param name="stackT"></param>
        ''' <returns></returns>
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

            Dim whiteSpace As Tokens = stackT.WhiteSpace
            Dim source = (From x In lstToken Where Not stackT.Equals(x.TokenName, whiteSpace) Select x)
            Dim func As Func(Of Tokens) =
                StackParser.Parsing(Of Tokens)(source, stackT)
            Return func
        End Function
    End Module
End Namespace