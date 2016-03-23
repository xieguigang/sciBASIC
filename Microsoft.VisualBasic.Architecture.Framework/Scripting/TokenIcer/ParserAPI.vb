Imports System.Runtime.CompilerServices

Namespace Scripting.TokenIcer

    Public Module ParserAPI

        <Extension>
        Public Function GetTokens(Of Tokens)(parser As TokenParser(Of Tokens), expr As String) As Token(Of Tokens)()
            Dim lstToken As New List(Of Token(Of Tokens))
            Dim tmp As Token(Of Tokens) = Nothing

            parser.InputString = expr
            Do While Not parser.GetToken.ShadowCopy(tmp) Is Nothing
                Call lstToken.Add(tmp)
            Loop

            Return lstToken.ToArray
        End Function

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

            Dim lstToken As Token(Of Tokens)() = parser.GetTokens(expr)
            Dim whiteSpace As Tokens = stackT.WhiteSpace
            Dim source As Token(Of Tokens)() = (From x As Token(Of Tokens)
                                                In lstToken
                                                Where Not stackT.Equals(x.TokenName, whiteSpace)
                                                Select x).ToArray
            Dim func As Func(Of Tokens) =
                StackParser.Parsing(Of Tokens)(source, stackT)
            Return func
        End Function

        <Extension> Public Function [As](Of Tokens, T)(x As Token(Of Tokens)) As T
            Dim obj As T = InputHandler.CTypeDynamic(Of T)(x.TokenValue)
            Return obj
        End Function

        <Extension> Public Function [CType](Of Tokens)(x As Token(Of Tokens), type As Type) As Object
            Dim obj As Object = InputHandler.CTypeDynamic(x.TokenValue, type)
            Return obj
        End Function

        ''' <summary>
        ''' Try cast the token value to a .NET object based on the token type name.
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension> Public Function [TryCast](Of Tokens)(x As Token(Of Tokens)) As Object
            Dim typeName As String = Scripting.ToString(x.TokenName)
            Dim type As Type = Nothing
            If Scripting.GetType(typeName, False).ShadowCopy(type) Is Nothing Then
                Return x.TokenValue
            Else
                Return CTypeDynamic(x.TokenValue, type)
            End If
        End Function
    End Module
End Namespace