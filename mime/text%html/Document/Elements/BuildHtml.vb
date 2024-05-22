#Region "Microsoft.VisualBasic::1b1567a36a3ee72e7fa5eb7e89a8eac7, mime\text%html\Document\Elements\BuildHtml.vb"

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

    '   Total Lines: 31
    '    Code Lines: 22 (70.97%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (29.03%)
    '     File Size: 940 B


    '     Module BuildHtmlDocument
    ' 
    '         Function: ToHtml
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq

Namespace Document

    Module BuildHtmlDocument

        <Extension>
        Public Function ToHtml(node As HtmlElement) As String
            Dim html As New StringBuilder

            If node.Attributes.IsNullOrEmpty Then
                Call html.AppendLine($"<{node.TagName}>")
            Else
                Call html.AppendLine($"<{node.TagName} {(From attr In node.Attributes Select $"{attr.Name}=""{attr.Values.JoinBy(" ")}""").JoinBy(" ")}>")
            End If

            Call html.AppendLine(node.InnerText)

            For Each child As InnerPlantText In node.HtmlElements.SafeQuery
                Call html.AppendLine(child.GetHtmlText)
            Next

            Call html.AppendLine($"</{node.TagName}>")

            Return html.ToString
        End Function

    End Module
End Namespace
