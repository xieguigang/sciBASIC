Imports Microsoft.VisualBasic.SoftwareToolkits.XmlDoc.Assembly

Namespace SoftwareToolkits.XmlDoc.Serialization

    Public Class URLBuilder

        Public ReadOnly Property [lib] As Libraries
        Public ReadOnly Property jsName As String

        ''' <summary>
        ''' link url after hexo generates the static site
        ''' </summary>
        Dim ext As String

        Sub New(type As Libraries, js$)
            Me.jsName = js
            Me.lib = type
            Me.ext = If(type = Libraries.Hexo, ".html", ".md")
        End Sub

        Sub New()
            Me.New(Libraries.Github, Nothing)
        End Sub

        Public Function GetNamespaceTypeUrl(ns As ProjectNamespace, pt As ProjectType) As String
            Dim file$
            Dim link$

            If [lib] = Libraries.Hexo Then
                file = "T-" & ns.Path & "." & pt.Name & $"{ext}"
                link = $"[{pt.Name}]({file})"
            ElseIf [lib] = Libraries.Github Then
                file = $"./{pt.Name}.md"
                link = $"[{pt.Name}]({file})"
            Else
                link = $"<a href=""#"" onClick=""load('/docs/{ns.Path}/{pt.Name}.md')"">{pt.Name}</a>"
            End If

            Return link
        End Function

        ''' <summary>
        ''' ``*.md`` output path
        ''' </summary>
        ''' <param name="folderPath$"></param>
        ''' <param name="ns"></param>
        ''' <returns></returns>
        Public Function GetNamespaceSave(folderPath$, ns As ProjectNamespace) As String
            With Me
                Dim path$ = If(.[lib] <> Libraries.Hexo,
                    folderPath & "/" & ns.Path & "/index.md",
                    folderPath & "/N-" & ns.Path & ".md")

                Return path
            End With
        End Function

        Public Function GetTypeSave(folderPath$, type As ProjectType) As String
            Dim path$ = If([lib] = Libraries.Hexo,
                folderPath & "/T-" & type.[Namespace].Path & "." & type.Name & ".md",
                folderPath & "/" & type.Name & ".md")

            Return path
        End Function

        Public Function GetTypeNamespaceLink(type As ProjectType) As String
            Dim link$

            If [lib] <> Libraries.xDoc Then
                Dim file$ = If([lib] = Libraries.Hexo,
                    $"N-{type.[Namespace].Path}{ext}",
                    "./index.md")
                link = $"[{type.[Namespace].Path}]({file})"
            Else
                link = $"<a href=""#"" onClick=""load('/docs/{type.[Namespace].Path}/index.md')"">{type.Namespace.Path}</a>"
            End If

            Return link
        End Function
    End Class
End Namespace