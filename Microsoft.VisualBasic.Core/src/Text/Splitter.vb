#Region "Microsoft.VisualBasic::a539a9b87975a8fe28029b95cbd6a5eb, Microsoft.VisualBasic.Core\src\Text\Splitter.vb"

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


    ' Code Statistics:

    '   Total Lines: 160
    '    Code Lines: 98
    ' Comment Lines: 45
    '   Blank Lines: 17
    '     File Size: 6.48 KB


    '     Class Splitter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __split, isValidDelimiterBinary, isValidDelimiterText, Split
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' http://www.codeproject.com/Articles/7464/Very-Fast-Splitter-with-support-for-Multi-Characte

Namespace Text

    ''' <summary>
    ''' Split strings with support to multi-character, multi-lines Delimiter
    ''' </summary>
    Public NotInheritable Class Splitter

        ''' <summary>
        ''' Holds the string to split
        ''' </summary>
        Dim expression As Char()
        ''' <summary>
        ''' Delimiter to split the expression with
        ''' </summary>
        Dim delimiter As Char()

        ''' <summary>
        ''' Constrctor for The Splitter
        ''' </summary>
        Protected Sub New()
        End Sub

        Private Function isValidDelimiterBinary(StringIndex As Integer, DelimiterIndex As Integer) As Boolean
            If DelimiterIndex = delimiter.Length Then
                Return True
            End If
            If StringIndex = expression.Length Then
                Return False
            End If
            ' If the current character of the expression matches the current character 
            ' of the Delimiter, then go to next character
            If expression(StringIndex) = delimiter(DelimiterIndex) Then
                Return isValidDelimiterBinary(StringIndex + 1, DelimiterIndex + 1)
            Else
                Return False
            End If
        End Function

        Private Function isValidDelimiterText(StringIndex As Integer, DelimiterIndex As Integer) As Boolean
            If DelimiterIndex = delimiter.Length Then
                Return True
            End If
            If StringIndex = expression.Length Then
                Return False
            End If
            ' If the current character of the expression matches the current character 
            ' of the Delimiter, then go to next character
            If Char.ToLower(expression(StringIndex)) = Char.ToLower(delimiter(DelimiterIndex)) Then
                Return isValidDelimiterText(StringIndex + 1, DelimiterIndex + 1)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 这个是安全的函数，空值会返回空集合
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="delimiter"></param>
        ''' <param name="isSingle"></param>
        ''' <param name="count"></param>
        ''' <param name="compare"></param>
        ''' <returns></returns>
        Public Shared Function Split(s$, delimiter$, isSingle As Boolean,
                                     Optional count% = Integer.MaxValue,
                                     Optional compare As CompareMethod = CompareMethod.Binary) As String()

            If s Is Nothing OrElse s.StringEmpty(whitespaceAsEmpty:=False) Then
                Return {}
            Else
                Return New Splitter().__split(s, delimiter, isSingle, count, compare)
            End If
        End Function

        Private Function __split(expression$, delimiter$, SingleSeparator As Boolean, count%, Compare As CompareMethod) As String()
            ' Array to hold Splitted Tokens
            Dim tokens As New List(Of String)

            'Update Private Members
            Me.expression = expression.ToCharArray
            Me.delimiter = delimiter.ToCharArray

            'If not using single separator, then use the regular split function
            If Not SingleSeparator Then
                If count >= 0 Then
                    Return expression.Split(delimiter.ToCharArray(), count)
                Else
                    Return expression.Split(delimiter.ToCharArray())
                End If
            End If

            'Check if count = 0 then return an empty array
            If count = 0 Then
                Return New String(-1) {}
                'Check if Count = 1 then return the whole expression
            ElseIf count = 1 Then
                Return New String() {expression}
            Else
                count -= 1
            End If

            Dim i As Integer
            ' Indexer to loop over the string with
            Dim iStart As Integer = 0
            'The Start index of the current token in the expression
            If Compare = CompareMethod.Binary Then

                For i = 0 To expression.Length - 1
                    If isValidDelimiterBinary(i, 0) Then
                        'Assign New Token
                        tokens.Add(expression.Substring(iStart, i - iStart))
                        'Update Index
                        i += delimiter.Length - 1
                        'Update Current Token Start index
                        iStart = i + 1
                        'If we reached the tokens limit , then exit For
                        If tokens.Count = count AndAlso count >= 0 Then
                            Exit For
                        End If
                    End If
                Next
            Else
                For i = 0 To expression.Length - 1
                    If isValidDelimiterText(i, 0) Then
                        'Assign New Token
                        tokens.Add(expression.Substring(iStart, i - iStart))
                        'Update Index
                        i += delimiter.Length - 1
                        'Update Current Token Start index
                        iStart = i + 1
                        'If we reached the tokens limit , then exit For
                        If tokens.Count = count AndAlso count >= 0 Then
                            Exit For
                        End If
                    End If
                Next
            End If

            ' If there is still data & have not been added
            If iStart < expression.Length Then
                Dim lastToken = expression.Substring(iStart, expression.Length - iStart)

                If lastToken = delimiter Then
                    tokens.Add(Nothing)
                Else
                    tokens.Add(lastToken)
                End If
                ' If there is no elements in the tokens array,
                ' then pass the whole string as the one element
            ElseIf tokens.Count = 0 Then
                tokens.Add(expression)
            End If

            'Return Splitted Tokens
            Return tokens.ToArray
        End Function
    End Class
End Namespace
