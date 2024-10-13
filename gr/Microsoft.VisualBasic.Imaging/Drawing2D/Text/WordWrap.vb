#Region "Microsoft.VisualBasic::a4cd2fe870dc3305a68c035365df58fb, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\WordWrap.vb"

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

    '   Total Lines: 123
    '    Code Lines: 76 (61.79%)
    ' Comment Lines: 26 (21.14%)
    '    - Xml Docs: 57.69%
    ' 
    '   Blank Lines: 21 (17.07%)
    '     File Size: 4.77 KB


    '     Module WordWrap
    ' 
    '         Function: DoWordWrap, Explode
    ' 
    '         Sub: DrawTextCentraAlign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Text

    Public Module WordWrap

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="wrappedText">
        ''' Label text that re-layout by <see cref="WordWrap.DoWordWrap(String, Integer, String)"/>.
        ''' </param>
        ''' <param name="location"></param>
        ''' <param name="brush"></param>
        ''' <param name="font"></param>
        <Extension>
        Public Sub DrawTextCentraAlign(g As IGraphics, wrappedText$, location As PointF, brush As Brush, font As Font)
            Dim lines = wrappedText.LineTokens

            If lines.Length = 1 Then
                Call g.DrawString(wrappedText, font, brush, location)
            Else
                Dim max As SizeF = lines _
                    .Select(Function(l)
                                Return g.MeasureString(l, font)
                            End Function) _
                    .OrderByDescending(Function(w) w.Width) _
                    .First
                Dim size As SizeF
                Dim y As Single = location.Y - max.Height
                Dim x As Single = location.X

                For Each line As String In lines
                    size = g.MeasureString(line, font)
                    x = location.X + (max.Width - size.Width) / 2
                    y = y + size.Height

                    Call g.DrawString(line, font, brush, New PointF(x, y))
                Next
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="str">目标字符串</param>
        ''' <param name="width">一行文本所限定的字符个数</param>
        ''' <param name="splitChars">在行末所插入的分隔符</param>
        ''' <returns></returns>
        <Extension>
        Public Function DoWordWrap(str$, width%, Optional splitChars$ = " -" & VisualBasic.Text.ASCII.TAB) As String
            Dim words = Explode(str, splitChars)
            Dim curLineLength = 0
            Dim sb As New StringBuilder()

            For i As Integer = 0 To words.Length - 1
                Dim word = words(i)

                ' If adding the New word to the current line would be too long,
                ' then put it on a New line (And split it up if it's too long).
                If (curLineLength + word.Length > width) Then

                    ' Only move down to a New line if we have text on the current line.
                    ' Avoids situation where wrapped whitespace causes emptylines in text.
                    If (curLineLength > 0) Then
                        sb.Append(Environment.NewLine)
                        curLineLength = 0
                    End If

                    ' If the current word Is too long to fit on a line even on it's own then
                    ' split the word up.
                    Do While (word.Length > width)
                        sb.Append(word.Substring(0, width - 1) & "-")
                        word = word.Substring(width - 1)
                        sb.Append(Environment.NewLine)
                    Loop

                    ' Remove leading whitespace from the word so the New line starts flush to the left.
                    word = word.TrimStart()
                End If

                sb.Append(word)
                curLineLength += word.Length
            Next

            Return sb.ToString()
        End Function

        Public Function Explode(str$, splitChars As Char()) As String()
            Dim parts As New List(Of String)()
            Dim startIndex = 0

            Do While True
                Dim index = str.IndexOfAny(splitChars, startIndex)

                If (index = -1) Then
                    Return parts + str.Substring(startIndex)
                End If

                Dim word = str.Substring(startIndex, index - startIndex)
                Dim nextChar = str.Substring(index, 1)(0)

                ' Dashes And the likes should stick to the word occuring before it. 
                ' Whitespace doesn't have to.
                If (Char.IsWhiteSpace(nextChar)) Then
                    parts.Add(word)
                    parts.Add(nextChar.ToString())
                Else
                    parts.Add(word + nextChar)
                End If

                startIndex = index + 1
            Loop

            Throw New Exception("This exception will never happends")
        End Function
    End Module
End Namespace
