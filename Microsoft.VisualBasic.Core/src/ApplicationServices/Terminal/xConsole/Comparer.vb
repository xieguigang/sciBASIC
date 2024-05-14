#Region "Microsoft.VisualBasic::521a6385da64640c5632686b88208ddd, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\xConsole\Comparer.vb"

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

    '   Total Lines: 108
    '    Code Lines: 56
    ' Comment Lines: 34
    '   Blank Lines: 18
    '     File Size: 3.40 KB


    '     Class Comparer
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Find
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace ApplicationServices.Terminal.xConsole

#Region "COMPARER 💻"

    ''' <summary>
    ''' This can compute the input then return back the most appropriate word.
    ''' </summary>
    Public Class Comparer

        ''' <summary>
        ''' This is the word to find
        ''' </summary>
        Public Word As String = String.Empty

        ''' <summary>
        ''' Descrizione
        ''' </summary>
        Public Description As String = String.Empty

        ''' <summary>
        ''' Init to 0!
        ''' </summary>
        Private Points As Integer = 0

        ''' <summary>
        ''' Initliaze a new instance
        ''' </summary>
        ''' <param name="w">The word to find</param>
        ''' <param name="p">It's should be 0</param>
        Public Sub New(w As String, p As Integer)
            Word = w
            Points = p
        End Sub

        ''' <summary>
        ''' Initliaze a new instance
        ''' </summary>
        ''' <param name="w">The word to find</param>
        Public Sub New(w As String)
            Word = w
            Points = 0
        End Sub

        ''' <summary>
        ''' Initliaze a new instance
        ''' </summary>
        ''' <param name="w">The word to find</param>
        ''' <param name="desc">Description (do nothing)</param>
        Public Sub New(w As String, desc As String)
            Word = w
            Description = desc
        End Sub

        ''' <summary>
        ''' Find a word from an input abbreviation (es n > name)
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Shared Function Find(abbr As String, ByRef Words As List(Of Comparer)) As String
            Dim Result As String = String.Empty
            Dim Best As Integer = 0
            Dim c As Integer = 0

            While Words.Count > c AndAlso Words(c) IsNot Nothing
                Dim word = Words(c)

                If abbr = word.Word Then
                    Result = abbr
                    Exit While
                End If

                For i As Integer = 0 To abbr.Length - 1
                    If abbr.Length < word.Word.Length AndAlso abbr(i) = word.Word(i) Then
                        word.Points += 1
                    Else
                        word.Points = 0
                        Exit For
                    End If
                Next

                If word.Points > Best Then
                    Best = word.Points
                End If


                c += 1
            End While
            ' End while
            Dim n As i32 = 0
            For Each word As Comparer In Words
                If word.Points = Best AndAlso word.Points > 0 Then
                    Result = word.Word
                    If ++n > 1 Then
                        Result = String.Empty
                    End If
                End If
            Next

            Return Result
        End Function
    End Class

    '////////////////////////////////////////////////////////////////////////////////////////////////
    '////////////////////////////////////////////////////////////////////////////////////////////////
    '////////////////////////////////////////////////////////////////////////////////////////////////
#End Region
End Namespace
