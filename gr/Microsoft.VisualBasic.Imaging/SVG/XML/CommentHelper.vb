#Region "Microsoft.VisualBasic::83bde54d14c29185050b92504f0c3587, gr\Microsoft.VisualBasic.Imaging\SVG\XML\CommentHelper.vb"

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

    '     Module CommentHelper
    ' 
    '         Function: CreateComment
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml

Namespace SVG.XML

    Module CommentHelper

        Const declare$ = "SVG document was created by sciBASIC svg image driver:"

        <Extension>
        Public Function CreateComment(svg As SVGXml) As XmlComment
            Dim comment As New StringBuilder
            Dim indent As New String(" "c, 6)

            Call comment.AppendLine _
                        .Append(indent) _
                        .AppendLine([declare]) _
                        .AppendLine _
                        .Append(indent & New String(" "c, 3)) _
                        .AppendLine("visit: " & LICENSE.githubURL)

            If Not svg.XmlComment.StringEmpty Then
                For Each line$ In svg.XmlComment.lTokens
                    comment.AppendLine _
                           .Append(indent) _
                           .Append(line)
                Next
            End If

            comment.AppendLine _
                   .Append("  ")

            Return New XmlDocument().CreateComment(comment.ToString)
        End Function
    End Module
End Namespace
