#Region "Microsoft.VisualBasic::d18920e9770fef11cc65e003d51d3825, sciBASIC#\Data\Trinity\Model\Sentence.vb"

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

'   Total Lines: 100
'    Code Lines: 70
' Comment Lines: 10
'   Blank Lines: 20
'     File Size: 2.98 KB


'     Class Sentence
' 
'         Properties: IsEmpty, segments
' 
'         Function: matchIndex, Parse, searchIndex, ToString, Trim
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Parser

Namespace Model

    Public Class Sentence

        ''' <summary>
        ''' 带有前后顺序的单词列表
        ''' </summary>
        ''' <returns></returns>
        Public Property words As Word()

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return words.IsNullOrEmpty OrElse words.All(AddressOf TextRank.IsEmpty)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(tokens As IEnumerable(Of String))
            words = tokens _
                .Select(Function(si) New Word(si)) _
                .ToArray
        End Sub

        Public Function has(token As String) As Boolean
            Return Array.IndexOf(words, token) > -1
        End Function

        ''' <summary>
        ''' exactly token matched
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function matchIndex(token As String) As Integer
            Return Array.IndexOf(words, token)
        End Function

        ''' <summary>
        ''' search for starts with [prefix]
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function searchIndex(token As String) As Integer
            For i As Integer = 0 To words.Length - 1
                If _words(i).StartsWith(token) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Public Overrides Function ToString() As String
            Return words.JoinBy(" ")
        End Function

        Friend Shared Function Parse(line As String, chemicalNameRule As Boolean) As Sentence
            Dim tokens As String() = New SentenceCharWalker(line) _
                .GetTokens _
                .ToArray

            If chemicalNameRule Then
                tokens = Sentence.ChemicalNameRule(tokens).ToArray
            End If

            Return New Sentence(tokens)
        End Function

        ''' <summary>
        ''' try to fix of the un-expected token split for the chemical name
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        Private Shared Iterator Function ChemicalNameRule(tokens As String()) As IEnumerable(Of String)
            Dim open As Boolean = False
            Dim buf As New CharBuffer

            For Each si As String In tokens
                If open Then
                    buf.Add(" "c)
                    buf.Add(si)

                    If si.Count("("c) = 0 AndAlso si.Count(")"c) > 0 Then
                        open = False
                        Yield New String(buf.PopAllChars)
                    End If
                End If
                If si.Count("("c) > 0 AndAlso si.Count(")"c) = 0 Then
                    open = True
                    buf.Add(si)
                Else
                    Yield si
                End If
            Next

            If buf > 0 Then
                Yield New String(buf.PopAllChars)
            End If
        End Function

        Friend Function Trim() As Sentence
            Return New Sentence With {
               .words = words _
                   .Where(Function(si) Not TextRank.IsEmpty(si)) _
                   .ToArray
            }
        End Function

    End Class
End Namespace
