#Region "Microsoft.VisualBasic::de11401d5f361830e4c906c74a52a0ca, gr\Microsoft.VisualBasic.Imaging\SVG\XML\CommentHelper.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



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
        Public Function CreateComment(xmlComment As String) As XmlComment
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

            Return New XmlDocument().CreateComment(comment.ToString)
        End Function
    End Module
End Namespace
