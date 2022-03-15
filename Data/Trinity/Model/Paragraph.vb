#Region "Microsoft.VisualBasic::e4f2912ffa95899b2184f10096cb1784, sciBASIC#\Data\Trinity\Model\Paragraph.vb"

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

    '   Total Lines: 68
    '    Code Lines: 52
    ' Comment Lines: 0
    '   Blank Lines: 16
    '     File Size: 1.96 KB


    '     Class Paragraph
    ' 
    '         Properties: sentences
    ' 
    '         Function: GetParagraph, Segmentation, ToString, Trim
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Model

    Public Class Paragraph

        Public Property sentences As Sentence()

        Public Overrides Function ToString() As String
            Return sentences.JoinBy(". ")
        End Function

        Public Shared Iterator Function Segmentation(text As String) As IEnumerable(Of Paragraph)
            Dim p As New Value(Of Paragraph)

            For Each block As String() In text.LineTokens.Split(Function(str) str.Trim = "")
                If block.All(Function(str) str.Trim.StringEmpty) Then
                    Continue For
                End If

                If Not p = block _
                    .Select(AddressOf Strings.Trim) _
                    .JoinBy(" ") _
                    .Trim _
                    .DoCall(AddressOf GetParagraph) Is Nothing Then

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

        Private Shared Function GetParagraph(text As String) As Paragraph
            Dim sentences As String() = text.Split("."c, "?"c, "!"c)
            Dim sentenceList As Sentence() = sentences _
                .Select(AddressOf Sentence.Parse) _
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
