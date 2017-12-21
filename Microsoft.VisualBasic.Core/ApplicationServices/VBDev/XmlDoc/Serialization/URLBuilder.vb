#Region "Microsoft.VisualBasic::dd06b1574ce20bc2c7f091f361fe4921, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\VBDev\XmlDoc\Serialization\URLBuilder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly

Namespace ApplicationServices.Development.XmlDoc.Serialization

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
