#Region "Microsoft.VisualBasic::ad347a05c952990b7cae8d101c490ddf, gr\Microsoft.VisualBasic.Imaging\SVG\XML\CommentHelper.vb"

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

    '   Total Lines: 36
    '    Code Lines: 28 (77.78%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (22.22%)
    '     File Size: 1.13 KB


    '     Module CommentHelper
    ' 
    '         Function: CreateComment
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace SVG.XML

    Module CommentHelper

        Const declare$ = "SVG document was created by sciBASIC svg image driver:"

        <Extension>
        Public Function CreateComment(xmlComment As String) As String
            Dim comment As New StringBuilder
            Dim indent As New String(" "c, 6)

            Call comment.AppendLine _
                        .Append(indent) _
                        .AppendLine([declare]) _
                        .AppendLine _
                        .Append(indent & New String(" "c, 3)) _
                        .AppendLine("visit: " & LICENSE.githubURL)

            If Not xmlComment.StringEmpty Then
                For Each line$ In xmlComment.LineTokens
                    comment.AppendLine _
                           .Append(indent) _
                           .Append(line)
                Next
            End If

            comment.AppendLine _
                   .Append("  ")

            Return comment.ToString
        End Function
    End Module
End Namespace
