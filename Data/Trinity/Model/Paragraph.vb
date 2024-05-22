#Region "Microsoft.VisualBasic::005ff88053fa9f5f2b79e9da01bd98bb, Data\Trinity\Model\Paragraph.vb"

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

    '   Total Lines: 89
    '    Code Lines: 61 (68.54%)
    ' Comment Lines: 11 (12.36%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 17 (19.10%)
    '     File Size: 3.03 KB


    '     Class Paragraph
    ' 
    '         Properties: sentences
    ' 
    '         Function: GetParagraph, Segmentation, SplitParagraph, ToString, Trim
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Model

    ''' <summary>
    ''' a collection of the text <see cref="Sentence"/>.
    ''' </summary>
    Public Class Paragraph

        Public Property sentences As Sentence()

        Public Overrides Function ToString() As String
            Return sentences.JoinBy(". ")
        End Function

        Public Shared Iterator Function SplitParagraph(text As String) As IEnumerable(Of String)
            For Each block As String() In text.LineTokens.Split(Function(str) str.Trim = "")
                Dim trim As String() = block _
                    .Select(AddressOf Strings.Trim) _
                    .ToArray

                If trim.All(Function(str) str = "") Then
                    Continue For
                End If

                Yield trim.Where(Function(si) si <> "") _
                    .JoinBy(" ") _
                    .Trim
            Next
        End Function

        Public Shared Iterator Function Segmentation(text As String,
                                                     Optional delimiter As String = ".?!",
                                                     Optional chemicalNameRule As Boolean = False) As IEnumerable(Of Paragraph)
            Dim p As New Value(Of Paragraph)
            Dim del As Char() = delimiter.ToArray

            For Each par As String In SplitParagraph(text)
                If Not p = GetParagraph(par, del, chemicalNameRule) Is Nothing Then
                    Yield p
                End If
            Next
        End Function

        Private Function Trim() As Paragraph
            Dim list As New List(Of Sentence)

            For Each line As Sentence In sentences
                line = line.Trim

                If Not line.IsEmpty Then
                    list.Add(line)
                End If
            Next

            Return New Paragraph With {
                .sentences = list.ToArray
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="delimiter">
        ''' the delimiter of the sentences, usually be delimiter symbol ./?/! to end a sentence.
        ''' </param>
        ''' <returns></returns>
        Private Shared Function GetParagraph(text As String, delimiter As Char(), chemicalNameRule As Boolean) As Paragraph
            Dim sentences As String() = text.Split(delimiter)
            Dim sentenceList As Sentence() = sentences _
                .Select(Function(si) Sentence.Parse(si, chemicalNameRule)) _
                .ToArray
            Dim p As New Paragraph With {
                .sentences = sentenceList
            }

            p = p.Trim()

            If p.sentences.IsNullOrEmpty Then
                Return Nothing
            Else
                Return p
            End If
        End Function

    End Class
End Namespace
