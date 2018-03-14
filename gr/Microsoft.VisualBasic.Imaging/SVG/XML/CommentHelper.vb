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