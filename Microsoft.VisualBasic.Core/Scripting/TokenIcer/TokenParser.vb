#Region "Microsoft.VisualBasic::9479e4d351638c81379704163fe558e6, Microsoft.VisualBasic.Core\Scripting\TokenIcer\TokenParser.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class TokenParser
    ' 
    '         Properties: InputString, UNDEFINED
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetToken, (+2 Overloads) Peek
    ' 
    '         Sub: PrepareRegex, ResetParser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
    Public Class TokenParser(Of Tokens As IComparable)
        ' This dictionary will store our RegEx rules
        Private ReadOnly _tokens As Dictionary(Of Tokens, String)
        ' This dictionary will store our matches
        Private ReadOnly _regExMatchCollection As Dictionary(Of Tokens, MatchCollection)
        ' This input string will store the string to parse
        Private _inputString As String
        ' This index is used internally so the parser knows where it left off
        Private _index As Integer

        ' A public setter for our input string
        ''' <summary>
        ''' InputString Property
        ''' </summary>
        ''' <value>
        ''' The string value that holds the input string.
        ''' </value>
        Public WriteOnly Property InputString() As String
            Set
                Call ResetParser()
                _inputString = Value
                Call PrepareRegex()
            End Set
        End Property

        Public ReadOnly Property UNDEFINED As Tokens

        ' Our Constructor, which simply initializes values
        ''' <summary>
        ''' Default Constructor
        ''' </summary>
        ''' <param name="tokens">Values is the regex expression</param>
        ''' <remarks>
        ''' The constructor initalizes memory and adds all of the tokens to the token dictionary.
        ''' </remarks>
        Public Sub New(tokens As IEnumerable(Of KeyValuePair(Of Tokens, String)), UNDEFINED As Tokens)
            Call Me.New(tokens.ToDictionary, UNDEFINED)
        End Sub

        ' Our Constructor, which simply initializes values
        ''' <summary>
        ''' Default Constructor
        ''' </summary>
        ''' <param name="tokens">Values is the regex expression</param>
        ''' <remarks>
        ''' The constructor initalizes memory and adds all of the tokens to the token dictionary.
        ''' </remarks>
        Public Sub New(tokens As Dictionary(Of Tokens, String), UNDEFINED As Tokens)
            _tokens = New Dictionary(Of Tokens, String)(tokens)
            _regExMatchCollection = New Dictionary(Of Tokens, MatchCollection)()
            _index = 0
            _inputString = String.Empty
            _UNDEFINED = UNDEFINED
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
        Public Function GetToken() As Token(Of Tokens)
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
                        Return New Token(Of Tokens)(pair.Key, match.Value)
                    ElseIf match.Index > _index Then
                        Exit For
                    End If
                Next
            Next

            ' If execution got here, then we increment our index pointer
            ' and return an Undefined token. 
            _index += 1
            Return New Token(Of Tokens)(UNDEFINED, "")
        End Function

        ' Peek() will retrieve a PeekToken object and will allow you to see the next token
        ' that GetToken() will retrieve.
        ''' <summary>
        ''' Returns the next token that GetToken() will return.
        ''' </summary>
        ''' <seealso cref="TokenParser(Of Tokens).Peek(PeekToken(Of Tokens))" />
        Public Function Peek() As PeekToken(Of Tokens)
            Return Peek(New PeekToken(Of Tokens)(_index, New Token(Of Tokens)(UNDEFINED, "")))
        End Function

        ' This is an overload for Peek(). By passing in the last PeekToken object
        ' received from Peek(), you can peek ahead to the next token, and the token after that, etc...
        ''' <summary>
        ''' Returns the next token after the Token passed here
        ''' </summary>
        ''' <param name="peekToken">The PeekToken token returned from a previous Peek() call</param>
        ''' <seealso cref="Peek()" />
        Public Function Peek(peekToken As PeekToken(Of Tokens)) As PeekToken(Of Tokens)
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
                    Dim pt As New PeekToken(Of Tokens)(_index, New Token(Of Tokens)(pair.Key, m.Value))
                    _index = oldIndex
                    Return pt
                End If
            Next
            Dim pt2 As New PeekToken(Of Tokens)(_index + 1, New Token(Of Tokens)(UNDEFINED, ""))
            _index = oldIndex
            Return pt2
        End Function
    End Class
End Namespace
