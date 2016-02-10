' This file was Auto Generated with TokenIcer
Imports System.Collections.Generic
Imports System.Text.RegularExpressions

Namespace Scripting.TokenIcer

    ''' <summary>
    ''' TokenParser
    ''' </summary>
    ''' <remarks>
    ''' TokenParser is the main parser engine for converting input into lexical tokens.
    ''' 
    ''' Auto Generated from
    ''' http://www.codeproject.com/Articles/220042/Easily-Create-Your-Own-Parser
    ''' </remarks>
    Public Class TokenParser
        ' This dictionary will store our RegEx rules
        Private ReadOnly _tokens As Dictionary(Of Tokens, String)
        ' This dictionary will store our matches
        Private ReadOnly _regExMatchCollection As Dictionary(Of Tokens, MatchCollection)
        ' This input string will store the string to parse
        Private _inputString As String
        ' This index is used internally so the parser knows where it left off
        Private _index As Integer

        ' This is our token enumeration. It holds every token defined in the grammar
        ''' <summary>
        ''' Tokens is an enumeration of all possible token values.
        ''' </summary>
        Public Enum Tokens
            UNDEFINED = 0
            CallFunc = 1
            Float = 2
            Factorial = 3
            [Integer] = 4
            ArrayType = 5
            ParamDeli = 6
            WhiteSpace = 7
            [Let] = 8
            Equals = 9
            LPair = 10
            RPair = 11
            Asterisk = 12
            Slash = 13
            Plus = 14
            Minus = 15
            Power = 16
            [Mod] = 17
            Pretend = 18
            [And] = 19
            [Not] = 20
            [Or] = 21
            var = 22
            varRef = 23
            constRef = 24
        End Enum

        ' A public setter for our input string
        ''' <summary>
        ''' InputString Property
        ''' </summary>
        ''' <value>
        ''' The string value that holds the input string.
        ''' </value>
        Public WriteOnly Property InputString() As String
            Set
                _inputString = Value
                PrepareRegex()
            End Set
        End Property

        ' Our Constructor, which simply initializes values
        ''' <summary>
        ''' Default Constructor
        ''' </summary>
        ''' <remarks>
        ''' The constructor initalizes memory and adds all of the tokens to the token dictionary.
        ''' </remarks>
        Public Sub New()
            _tokens = New Dictionary(Of Tokens, String)()
            _regExMatchCollection = New Dictionary(Of Tokens, MatchCollection)()
            _index = 0
            _inputString = String.Empty

            ' These lines add each grammar rule to the dictionary
            _tokens.Add(Tokens.CallFunc, "->\s*[a-zA-Z_][a-zA-Z0-9_]*")
            _tokens.Add(Tokens.Float, "[0-9]+\.+[0-9]+")
            _tokens.Add(Tokens.Factorial, "[0-9]+!")
            _tokens.Add(Tokens.[Integer], "[0-9]+")
            _tokens.Add(Tokens.ArrayType, "[a-zA-Z_][a-zA-Z0-9_]*\(\)")
            _tokens.Add(Tokens.ParamDeli, ",")
            _tokens.Add(Tokens.WhiteSpace, "[ \t]+")
            _tokens.Add(Tokens.[Let], "[Ll][Ee][Tt]")
            _tokens.Add(Tokens.Equals, "=")
            _tokens.Add(Tokens.LPair, "\(")
            _tokens.Add(Tokens.RPair, "\)")
            _tokens.Add(Tokens.Asterisk, "\*")
            _tokens.Add(Tokens.Slash, "\/")
            _tokens.Add(Tokens.Plus, "\+")
            _tokens.Add(Tokens.Minus, "\-")
            _tokens.Add(Tokens.Power, "\^")
            _tokens.Add(Tokens.[Mod], "%")
            _tokens.Add(Tokens.Pretend, "Pretend")
            _tokens.Add(Tokens.[And], "[aA][nN][dD]")
            _tokens.Add(Tokens.[Not], "[nN][oO][tT]")
            _tokens.Add(Tokens.[Or], "[oO][rR]")
            _tokens.Add(Tokens.var, "[a-zA-Z_][a-zA-Z0-9_]*")
            _tokens.Add(Tokens.varRef, "\$[a-zA-Z0-9_]*")
            _tokens.Add(Tokens.constRef, "[&][a-zA-Z0-9_]*")
        End Sub

        ' This function preloads the matches based on our rules and the input string
        ''' <summary>
        ''' PrepareRegex prepares the regex for parsing by pre-matching the Regex tokens.
        ''' </summary>
        Private Sub PrepareRegex()
            _regExMatchCollection.Clear()
            For Each pair As KeyValuePair(Of Tokens, String) In _tokens
                _regExMatchCollection.Add(pair.Key, Regex.Matches(_inputString, pair.Value))
            Next
        End Sub

        ' ResetParser() will reset the parser.
        ' Keep in mind that you must set the input string again
        ''' <summary>
        ''' ResetParser resets the parser to its inital state. Reloading InputString is required.
        ''' </summary>
        ''' <seealso cref="TokenParser.InputString" />
        Public Sub ResetParser()
            _index = 0
            _inputString = String.Empty
            _regExMatchCollection.Clear()
        End Sub

        ' GetToken() retrieves the next token and returns a token object
        ''' <summary>
        ''' GetToken gets the next token in queue
        ''' </summary>
        ''' <remarks>
        ''' GetToken attempts to the match the next character(s) using the
        ''' Regex rules defined in the dictionary. If a match can not be
        ''' located, then an Undefined token will be created with an empty
        ''' string value. In addition, the token pointer will be incremented
        ''' by one so that this token doesn't attempt to get identified again by
        ''' GetToken()
        ''' </remarks>
        Public Function GetToken() As Token
            ' If we are at the end of our input string then
            ' we return null to signify the end of our input string.
            ' While parsing tokens, you will undoubtedly be in a loop.
            ' Having your loop check for a null token is a good way to end that
            ' loop
            If _index >= _inputString.Length Then
                Return Nothing
            End If

            ' Iterate through our prepared matches/Tokens dictionary
            For Each pair As KeyValuePair(Of Tokens, MatchCollection) In _regExMatchCollection
                ' Iterate through each prepared match
                For Each match As Match In pair.Value
                    ' If we find a match, update our index pointer and return a new Token object
                    If match.Index = _index Then
                        _index += match.Length
                        Return New Token(pair.Key, match.Value)
                    ElseIf match.Index > _index Then
                        Exit For
                    End If
                Next
            Next

            ' If execution got here, then we increment our index pointer
            ' and return an Undefined token. 
            _index += 1
            Return New Token(Tokens.UNDEFINED, String.Empty)
        End Function

        ' Peek() will retrieve a PeekToken object and will allow you to see the next token
        ' that GetToken() will retrieve.
        ''' <summary>
        ''' Returns the next token that GetToken() will return.
        ''' </summary>
        ''' <seealso cref="Peek(PeekToken)" />
        Public Function Peek() As PeekToken
            Return Peek(New PeekToken(_index, New Token(Tokens.UNDEFINED, String.Empty)))
        End Function

        ' This is an overload for Peek(). By passing in the last PeekToken object
        ' received from Peek(), you can peek ahead to the next token, and the token after that, etc...
        ''' <summary>
        ''' Returns the next token after the Token passed here
        ''' </summary>
        ''' <param name="peekToken">The PeekToken token returned from a previous Peek() call</param>
        ''' <seealso cref="Peek()" />
        Public Function Peek(peekToken As PeekToken) As PeekToken
            Dim oldIndex As Integer = _index

            _index = peekToken.TokenIndex

            If _index >= _inputString.Length Then
                _index = oldIndex
                Return Nothing
            End If

            For Each pair As KeyValuePair(Of Tokens, String) In _tokens
                Dim r As New Regex(pair.Value)
                Dim m As Match = r.Match(_inputString, _index)

                If m.Success AndAlso m.Index = _index Then
                    _index = _index + m.Length
                    Dim pt As New PeekToken(_index, New Token(pair.Key, m.Value))
                    _index = oldIndex
                    Return pt
                End If
            Next
            Dim pt2 As New PeekToken(_index + 1, New Token(Tokens.UNDEFINED, String.Empty))
            _index = oldIndex
            Return pt2
        End Function
    End Class
End Namespace

