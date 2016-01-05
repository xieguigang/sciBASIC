Imports System.CodeDom
Imports System.Text

Namespace Parser

    ''' <summary>
    ''' Divides the string into tokens.
    ''' </summary>
    Public Class Tokenizer
        Private _En As CharEnumerator
        Private _IsInvalid As Boolean = False
        Private _PrevToken As Token = Token.NullToken

        ''' <summary>
        ''' A tokenizer is always constructed on a single string.  Create one tokenizer per string.
        ''' </summary>
        ''' <param name="s">string to tokenize</param>
        Public Sub New(s As String)
            _En = s.GetEnumerator()
            MoveNext()
        End Sub

        ''' <summary>
        ''' Moves to the next character.  If there are no more characters, then the tokenizer is
        ''' invalid.
        ''' </summary>
        Private Sub MoveNext()
            If Not _En.MoveNext() Then
                _IsInvalid = True
            End If
        End Sub

        ''' <summary>
        ''' Allows access to the token most recently parsed.
        ''' </summary>
        Public ReadOnly Property Current() As Token
            Get
                Return _PrevToken
            End Get
        End Property

        ''' <summary>
        ''' Indicates that there are no more characters in the string and tokenizer is finished.
        ''' </summary>
        Public ReadOnly Property IsInvalid() As Boolean
            Get
                Return _IsInvalid
            End Get
        End Property

        ''' <summary>
        ''' Is the current character a letter or underscore?
        ''' </summary>
        Public ReadOnly Property IsChar() As Boolean
            Get
                If _IsInvalid Then
                    Return False
                End If
                Return ((_En.Current >= "A"c AndAlso _En.Current <= "Z"c) OrElse (_En.Current >= "a"c AndAlso _En.Current <= "z"c) OrElse _En.Current = "_"c)
            End Get
        End Property

        ''' <summary>
        ''' Is the current character a dot (".")?
        ''' </summary>
        Public ReadOnly Property IsDot() As Boolean
            Get
                If _IsInvalid Then
                    Return False
                End If
                Return _En.Current = "."c
            End Get
        End Property

        ''' <summary>
        ''' Is the current character a comma?
        ''' </summary>
        Public ReadOnly Property IsComma() As Boolean
            Get
                If _IsInvalid Then
                    Return False
                End If
                Return _En.Current = ","c
            End Get
        End Property

        ''' <summary>
        ''' Is the current character a number?
        ''' </summary>
        Public ReadOnly Property IsNumber() As Boolean
            Get
                If _IsInvalid Then
                    Return False
                End If
                Return (_En.Current >= "0"c AndAlso _En.Current <= "9"c)
            End Get
        End Property

        ''' <summary>
        ''' Is the current character a whitespace character?
        ''' </summary>
        Public ReadOnly Property IsSpace() As Boolean
            Get
                If _IsInvalid Then
                    Return False
                End If
                Return (_En.Current = " "c OrElse _En.Current = ControlChars.Tab)
            End Get
        End Property

        ''' <summary>
        ''' Is the current character an operator?
        ''' </summary>
        Public ReadOnly Property IsOperator() As Boolean
            Get
                If _IsInvalid Then
                    Return False
                End If
                Select Case _En.Current
                    Case ">"c, "<"c, "="c, "-"c, "+"c, "!"c, _
                        "/"c, "%"c, "*"c, "&"c, "|"c, "("c, _
                        ")"c, "["c, "]"c, """"c
                        Return True
                    Case Else
                        Return False
                End Select
            End Get
        End Property

        ''' <summary>
        ''' Gets the next token in the string.  Reads as many characters as necessary to retrieve
        ''' that token.
        ''' </summary>
        ''' <returns>next token</returns>
        Public Function GetNextToken() As Token
            If _IsInvalid Then
                Return Token.NullToken
            End If

            Dim token__1 As Token
            If IsChar Then
                token__1 = GetString()
            ElseIf IsComma Then
                token__1 = New Token(",", TokenType.Comma, TokenPriority.None)
                MoveNext()
            ElseIf IsDot Then
                token__1 = New Token(".", TokenType.Dot, TokenPriority.None)
                MoveNext()
            ElseIf IsNumber Then
                token__1 = GetNumber()
            ElseIf IsSpace Then
                ' Eat space and do recursive call.
                MoveNext()
                token__1 = GetNextToken()
            ElseIf IsOperator Then
                token__1 = GetOperator()
            Else
                token__1 = Token.NullToken
                MoveNext()
            End If

            _PrevToken = token__1
            Return token__1
        End Function

        ''' <summary>
        ''' Anything that starts with a character is considered a string.  This could be a 
        ''' primitive quoted string, a primitive expression, or an identifier
        ''' </summary>
        ''' <returns></returns>
        Private Function GetString() As Token
            ' Handle empty strings
            If _PrevToken.Type = TokenType.Quote AndAlso _En.Current = """"c Then
                MoveNext()
                Return New Token(String.Empty, TokenType.Primitive, TokenPriority.None)
            End If
            Dim sb As New StringBuilder()
            sb.Append(_En.Current)
            While True
                If _IsInvalid Then
                    Exit While
                End If
                MoveNext()
                If _IsInvalid Then
                    Exit While
                End If

                If IsChar Then
                    sb.Append(_En.Current)
                ElseIf IsNumber Then
                    sb.Append(_En.Current)
                Else
                    If _PrevToken.Type = TokenType.Quote Then
                        If _En.Current = """"c Then
                            MoveNext()
                            Exit While
                        ElseIf _En.Current = "\"c Then
                            ' In the case of \, we'll add that character and whatever character follows it.
                            sb.Append(_En.Current)
                            MoveNext()
                            If Not _IsInvalid Then
                                sb.Append(_En.Current)
                            End If
                        Else
                            sb.Append(_En.Current)
                        End If
                    Else
                        Exit While
                    End If
                End If
            End While
            Dim s As String = sb.ToString()

            ' "false" or "true" is a primitive expression.
            If s = "false" OrElse s = "true" Then
                Return New Token([Boolean].Parse(s), TokenType.Primitive, TokenPriority.None)
            End If

            ' The previous token was a quote, so this is a primitive string.
            If _PrevToken.Type = TokenType.Quote Then
                Return New Token(s, TokenType.Primitive, TokenPriority.None)
            End If

            ' The default is that the string indicates an identifier.
            Return New Token(s, TokenType.Identifier, TokenPriority.None)
        End Function

        ''' <summary>
        ''' A token that starts with a number can be an integer, a long, or a double.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' An integer is the default for numbers.  Numbers can also be followed by a
        ''' l, L, d, or D character to indicate a long or a double value respectively.
        ''' Any numbers containing a dot (".") are considered doubles.
        ''' </remarks>
        Private Function GetNumber() As Token
            Dim sb As New StringBuilder()
            sb.Append(_En.Current)
            Dim isDouble As Boolean = False
            Dim isLong As Boolean = False
            Dim cont As Boolean = True
            While cont
                If _IsInvalid Then
                    Exit While
                End If
                MoveNext()
                If _IsInvalid Then
                    Exit While
                End If

                If IsNumber Then
                    sb.Append(_En.Current)
                ElseIf IsChar Then
                    Select Case _En.Current
                        Case "D"c, "d"c
                            isDouble = True
                            MoveNext()
                            If IsChar OrElse IsNumber Then
                                sb.Append(_En.Current)
                                Throw New ArgumentException("Invalid number: " & sb.ToString())
                            Else
                                cont = False
                            End If
                        Case "L"c, "l"c
                            isLong = True
                            MoveNext()
                            If IsChar OrElse IsNumber Then
                                sb.Append(_En.Current)
                                Throw New ArgumentException("Invalid number: " & sb.ToString())
                            Else
                                cont = False
                            End If
                        Case Else
                            sb.Append(_En.Current)
                            Throw New ArgumentException("Invalid number: " & sb.ToString())
                    End Select
                ElseIf IsDot Then
                    sb.Append(_En.Current)
                    If isDouble Then
                        ' The number has already been marked as a double, which means it already
                        ' contains a number.
                        Throw New ArgumentException("Invalid number: " & sb.ToString())
                    Else
                        isDouble = True
                    End If
                Else
                    Exit While
                End If
            End While
            Dim s As String = sb.ToString()
            If isLong Then
                Return New Token(Int64.Parse(s), TokenType.Primitive, TokenPriority.None)
            End If
            If isDouble Then
                Return New Token([Double].Parse(s), TokenType.Primitive, TokenPriority.None)
            End If
            Return New Token(Int32.Parse(s), TokenType.Primitive, TokenPriority.None)
        End Function

        ''' <summary>
        ''' Some operators take more than one character.  Also, the tokenizer is able to 
        ''' categorize the token's priority based on what kind of operator it is.
        ''' </summary>
        ''' <returns></returns>
        Private Function GetOperator() As Token
            Dim op As New String(_En.Current, 1)
            Select Case _En.Current
                Case "<"c, "="c, ">"c
                    MoveNext()
                    If _En.Current = "="c Then
                        op += _En.Current
                        MoveNext()
                    End If
                    Return New Token(op, TokenType.[Operator], TokenPriority.Equality)
                Case "-"c
                    MoveNext()
                    If _PrevToken.Type = TokenType.Primitive OrElse _PrevToken.Type = TokenType.Identifier OrElse _PrevToken.Type = TokenType.CloseParens Then
                        Return New Token(op, TokenType.[Operator], TokenPriority.PlusMinus)
                    Else
                        Return New Token(op, TokenType.[Operator], TokenPriority.UnaryMinus)
                    End If
                Case "+"c
                    MoveNext()
                    Return New Token(op, TokenType.[Operator], TokenPriority.PlusMinus)
                Case "!"c
                    MoveNext()
                    If _En.Current = "="c Then
                        op += _En.Current
                        MoveNext()
                        Return New Token(op, TokenType.[Operator], TokenPriority.Equality)
                    Else
                        Return New Token(op, TokenType.[Operator], TokenPriority.[Not])
                    End If
                Case "*"c, "/"c
                    MoveNext()
                    Return New Token(op, TokenType.[Operator], TokenPriority.MulDiv)
                Case "%"c
                    MoveNext()
                    Return New Token(op, TokenType.[Operator], TokenPriority.[Mod])
                Case "|"c
                    MoveNext()
                    If _En.Current = "|"c Then
                        op += _En.Current
                        MoveNext()
                    End If
                    Return New Token(op, TokenType.[Operator], TokenPriority.[Or])
                Case "&"c
                    MoveNext()
                    If _En.Current = "&"c Then
                        op += _En.Current
                        MoveNext()
                    End If
                    Return New Token(op, TokenType.[Operator], TokenPriority.[And])
                Case "("c
                    MoveNext()
                    Return New Token(op, TokenType.OpenParens, TokenPriority.None)
                Case ")"c
                    MoveNext()
                    Return New Token(op, TokenType.CloseParens, TokenPriority.None)
                Case "["c
                    MoveNext()
                    Return New Token(op, TokenType.OpenBracket, TokenPriority.None)
                Case "]"c
                    MoveNext()
                    Return New Token(op, TokenType.CloseBracket, TokenPriority.None)
                Case """"c
                    ' When we detect a quote, we can just ignore it since the user doesn't really need to know about it.
                    MoveNext()
                    _PrevToken = New Token(op, TokenType.Quote, TokenPriority.None)
                    Return GetString()
            End Select
            Return Token.NullToken
        End Function
    End Class
End Namespace