#Region "Microsoft.VisualBasic::dd06b1574ce20bc2c7f091f361fe4921, ..\sciBASIC#\Microsoft.VisualBasic.Core\ApplicationServices\VBDev\XmlDoc\Serialization\URLBuilder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

        ''' <summary>
        ''' link url after hexo generates the static site
        ''' </summary>
        Dim ext As String

        Sub New(type As Libraries)
            Me.lib = type
            Me.ext = If(type = Libraries.Hexo, ".html", ".md")
        End Sub

        Sub New()
            Me.New(Libraries.Github)
        End Sub

        Public Function GetNamespaceTypeUrl(ns As ProjectNamespace, pt As ProjectType) As String
            Dim file$
            Dim link$

            Select Case [lib]
                Case Libraries.Hexo
                    file = "T-" & ns.Path & "." & pt.Name & $"{ext}"
                    link = $"[{pt.Name}]({file})"
                Case Libraries.Github
                    file = $"./{pt.Name}.md"
                    link = $"[{pt.Name}]({file})"
                Case Libraries.xDoc
                    link = $"/docs/{ns.Path.Replace("."c, "/"c)}/{pt.Name}.md"
                Case Else
                    Throw New NotImplementedException
            End Select

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
                Dim path$

                Select Case .lib
                    Case Libraries.Hexo
                        path = folderPath & "/N-" & ns.Path & ".md"
                    Case Else
                        path = folderPath & "/" & ns.Path & "/index.md"
                End Select

                Return path
            End With
        End Function

        Public Function GetTypeSave(folderPath$, type As ProjectType) As String
            Dim path$

            Select Case [lib]
                Case Libraries.Hexo
                    path = folderPath & "/T-" & type.[Namespace].Path & "." & type.Name & ".md"
                Case Else
                    path = folderPath & "/" & type.Name & ".md"
            End Select

            Return path
        End Function

        Public Function GetTypeNamespaceLink(type As ProjectType) As String
            Dim link$

            If [lib] <> Libraries.xDoc Then
                Dim file$

                If [lib] = Libraries.Hexo Then
                    file = $"N-{type.[Namespace].Path}{ext}"
                Else
                    file = "./index.md"
                End If

                link = $"[{type.[Namespace].Path}]({file})"
            Else
                link = $"/docs/{type.[Namespace].Path.Replace("."c, "/"c)}/index.md"
            End If

            Return link
        End Function
    End Class
End Namespace
