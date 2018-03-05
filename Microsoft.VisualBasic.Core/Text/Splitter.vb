#Region "Microsoft.VisualBasic::f663f882ee374e66c69193b6809e73ac, Microsoft.VisualBasic.Core\Text\Splitter.vb"

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
        Dim m_Expression As Char()
        ''' <summary>
        ''' Delimiter to split the expression with
        ''' </summary>
        Dim m_Delimiter As Char()

        ''' <summary>
        ''' Constrctor for The Splitter
        ''' </summary>
        Protected Sub New()
        End Sub

        Private Function isValidDelimiterBinary(StringIndex As Integer, DelimiterIndex As Integer) As Boolean
            If DelimiterIndex = m_Delimiter.Length Then
                Return True
            End If
            If StringIndex = m_Expression.Length Then
                Return False
            End If
            'If the current character of the expression matches the current character of the Delimiter , then go to next character
            If m_Expression(StringIndex) = m_Delimiter(DelimiterIndex) Then
                Return isValidDelimiterBinary(StringIndex + 1, DelimiterIndex + 1)
            Else
                Return False
            End If
        End Function

        Private Function isValidDelimiterText(StringIndex As Integer, DelimiterIndex As Integer) As Boolean
            If DelimiterIndex = m_Delimiter.Length Then
                Return True
            End If
            If StringIndex = m_Expression.Length Then
                Return False
            End If
            'If the current character of the expression matches the current character of the Delimiter , then go to next character
            If [Char].ToLower(m_Expression(StringIndex)) = [Char].ToLower(m_Delimiter(DelimiterIndex)) Then
                Return isValidDelimiterText(StringIndex + 1, DelimiterIndex + 1)
            Else
                Return False
            End If
        End Function

        Public Shared Function Split(s As String, delimiter As String, isSingle As Boolean,
                                     Optional count As Integer = Integer.MaxValue,
                                     Optional compare As CompareMethod = CompareMethod.Binary) As String()
            Return New Splitter().__split(s, delimiter, isSingle, count, compare)
        End Function

        Private Function __split(Expression As String, Delimiter As String, SingleSeparator As Boolean, Count As Integer, Compare As CompareMethod) As String()
            'Update Private Members
            m_Expression = Expression.ToCharArray
            m_Delimiter = Delimiter.ToCharArray

            'Array to hold Splitted Tokens
            Dim Tokens As New List(Of String)
            'If not using single separator, then use the regular split function
            If Not SingleSeparator Then
                If Count >= 0 Then
                    Return Expression.Split(Delimiter.ToCharArray(), Count)
                Else
                    Return Expression.Split(Delimiter.ToCharArray())
                End If
            End If

            'Check if count = 0 then return an empty array
            If Count = 0 Then
                Return New String(-1) {}
                'Check if Count = 1 then return the whole expression
            ElseIf Count = 1 Then
                Return New String() {Expression}
            Else
                Count -= 1
            End If

            Dim i As Integer
            ' Indexer to loop over the string with
            Dim iStart As Integer = 0
            'The Start index of the current token in the expression
            If Compare = CompareMethod.Binary Then

                For i = 0 To Expression.Length - 1
                    If isValidDelimiterBinary(i, 0) Then
                        'Assign New Token
                        Tokens.Add(Expression.Substring(iStart, i - iStart))
                        'Update Index
                        i += Delimiter.Length - 1
                        'Update Current Token Start index
                        iStart = i + 1
                        'If we reached the tokens limit , then exit For
                        If Tokens.Count = Count AndAlso Count >= 0 Then
                            Exit For
                        End If
                    End If
                Next
            Else
                For i = 0 To Expression.Length - 1
                    If isValidDelimiterText(i, 0) Then
                        'Assign New Token
                        Tokens.Add(Expression.Substring(iStart, i - iStart))
                        'Update Index
                        i += Delimiter.Length - 1
                        'Update Current Token Start index
                        iStart = i + 1
                        'If we reached the tokens limit , then exit For
                        If Tokens.Count = Count AndAlso Count >= 0 Then
                            Exit For
                        End If
                    End If
                Next
            End If
            Dim LastToken As String = ""
            'If there is still data & have not been added
            If iStart < Expression.Length Then
                LastToken = Expression.Substring(iStart, Expression.Length - iStart)
                If LastToken = Delimiter Then
                    Tokens.Add(Nothing)
                Else
                    Tokens.Add(LastToken)
                End If
                'If there is no elements in the tokens array , then pass the whole string as the one element
            ElseIf Tokens.Count = 0 Then
                Tokens.Add(Expression)
            End If

            'Return Splitted Tokens
            Return Tokens.ToArray
        End Function
    End Class
End Namespace
