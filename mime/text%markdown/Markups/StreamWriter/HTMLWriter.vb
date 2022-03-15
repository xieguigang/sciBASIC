#Region "Microsoft.VisualBasic::5f3c341dc1d4682b7b8c2cec0a511eb3, sciBASIC#\mime\text%markdown\Markups\StreamWriter\HTMLWriter.vb"

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

    '   Total Lines: 83
    '    Code Lines: 55
    ' Comment Lines: 11
    '   Blank Lines: 17
    '     File Size: 3.00 KB


    '     Module HTMLWriter
    ' 
    '         Function: __generateDocNode, IsBr, Save, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.MIME.Html.Document

Namespace StreamWriter

    Public Module HTMLWriter

        ''' <summary>
        ''' Saves the html data model into a specific text document
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <param name="SaveTo"></param>
        ''' <returns></returns>
        <Extension> Public Function Save(doc As HtmlDocument, SaveTo As String) As Boolean
            Return ToString(doc).SaveTo(SaveTo)
        End Function

        ''' <summary>
        ''' Generates document string from the html data model.
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <returns></returns>
        Public Function ToString(doc As HtmlDocument) As String
            Dim sbr As New StringBuilder(1024)

            For Each node In doc.HtmlElements
                Call sbr.Append(node.__generateDocNode(""))
            Next

            Return sbr.ToString
        End Function

        <Extension> Private Function __generateDocNode(node As InnerPlantText, indent As String) As String
            If node.IsPlantText Then
                Return node.InnerText
            End If

            Dim sbr As StringBuilder = New StringBuilder(1024)
            Dim nodeElement = DirectCast(node, HtmlElement)

            If nodeElement.IsBr Then
                Return "<br />"
            End If

            Dim attrs As String =
                If(nodeElement.Attributes.IsNullOrEmpty,
                "",
                " " & String.Join(" ", nodeElement.Attributes.Select(Function(attr) $"{attr.Name}=""{attr.Value}""").ToArray))

            If nodeElement.HtmlElements.IsNullOrEmpty Then
                Call sbr.Append($"<{nodeElement.Name}{attrs}></{nodeElement.Name}>")
            Else
                If nodeElement.OnlyInnerText Then
                    Return $"<{nodeElement.Name}{attrs}>{nodeElement.HtmlElements.First.InnerText}</{nodeElement.Name}>"
                End If

                Call sbr.Append($"<{nodeElement.Name}{attrs}>")
                Dim NodeIndent = indent & "  "
                For Each node In nodeElement.HtmlElements

                    If Not node.IsPlantText Then
                        Call sbr.AppendLine()
                        Call sbr.Append(NodeIndent)
                    End If

                    Call sbr.Append(node.__generateDocNode(NodeIndent))
                Next

                Call sbr.AppendLine()
                Call sbr.Append($"{indent}</{nodeElement.Name}>")
            End If

            Return sbr.ToString
        End Function

        <Extension> Public Function IsBr(node As HtmlElement) As Boolean
            Return String.Equals(node.name, "br") AndAlso
            node.Attributes.IsNullOrEmpty AndAlso
            node.HtmlElements.IsNullOrEmpty
        End Function
    End Module
End Namespace
