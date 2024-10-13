#Region "Microsoft.VisualBasic::0dcf340b42dd0b324dc0248a10c3b9fb, mime\text%html\Render\CSS\CssBoxWordSplitter.vb"

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

    '   Total Lines: 122
    '    Code Lines: 77 (63.11%)
    ' Comment Lines: 17 (13.93%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 28 (22.95%)
    '     File Size: 3.68 KB


    '     Class CssBoxWordSplitter
    ' 
    '         Properties: Box, Text, Words
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CollapsesWhiteSpaces, EliminatesLineBreaks, IsLineBreak, IsSpace, IsTab
    ' 
    '         Sub: CutWord, SplitWords
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Render.CSS
#If NET48 Then
    ''' <summary>
    ''' Splits text on words for a box
    ''' </summary>
    Friend Class CssBoxWordSplitter
#Region "Static"

        ''' <summary>
        ''' Returns a bool indicating if the specified box white-space processing model specifies
        ''' that sequences of white spaces should be collapsed on a single whitespace
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Shared Function CollapsesWhiteSpaces(b As CssBox) As Boolean
            Return b.WhiteSpace = CssConstants.Normal OrElse b.WhiteSpace = CssConstants.Nowrap OrElse b.WhiteSpace = CssConstants.PreLine
        End Function

        ''' <summary>
        ''' Returns a bool indicating if line breaks at the source should be eliminated
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Shared Function EliminatesLineBreaks(b As CssBox) As Boolean
            Return b.WhiteSpace = CssConstants.Normal OrElse b.WhiteSpace = CssConstants.Nowrap
        End Function

#End Region

#Region "Fields"


        Dim _curword As CssBoxWord

#End Region

#Region "Ctor"

        Private Sub New()
            _Words = New List(Of CssBoxWord)()
            _curword = Nothing
        End Sub

        Public Sub New(box As CssBox, text As String)
            Me.New()
            _Box = box
            _Text = text.Replace(vbCr, String.Empty)
        End Sub

#End Region

        Public ReadOnly Property Words() As List(Of CssBoxWord)
        Public ReadOnly Property Text() As String
        Public ReadOnly Property Box() As CssBox

#Region "Public Metods"

        ''' <summary>
        ''' Splits the text on words using rules of the specified box
        ''' </summary>
        Public Sub SplitWords()

            If String.IsNullOrEmpty(Text) Then
                Return
            End If

            _curword = New CssBoxWord(Box)

            Dim onspace As Boolean = IsSpace(Text(0))

            For i As Integer = 0 To Text.Length - 1
                If IsSpace(Text(i)) Then
                    If Not onspace Then
                        CutWord()
                    End If

                    If IsLineBreak(Text(i)) Then
                        _curword.AppendChar(ControlChars.Lf)
                        CutWord()
                    ElseIf IsTab(Text(i)) Then
                        _curword.AppendChar(ControlChars.Tab)
                        CutWord()
                    Else
                        _curword.AppendChar(" "c)
                    End If

                    onspace = True
                Else
                    If onspace Then
                        CutWord()
                    End If
                    _curword.AppendChar(Text(i))

                    onspace = False
                End If
            Next

            CutWord()
        End Sub

        Private Sub CutWord()
            If _curword.Text.Length > 0 Then
                Words.Add(_curword)
            End If
            _curword = New CssBoxWord(Box)
        End Sub

        Private Function IsSpace(c As Char) As Boolean
            Return c = " "c OrElse c = ControlChars.Tab OrElse c = ControlChars.Lf
        End Function

        Private Function IsLineBreak(c As Char) As Boolean
            Return c = ControlChars.Lf OrElse c = ChrW(7)
        End Function

        Private Function IsTab(c As Char) As Boolean
            Return c = ControlChars.Tab
        End Function
#End Region
    End Class
#End If
End Namespace
