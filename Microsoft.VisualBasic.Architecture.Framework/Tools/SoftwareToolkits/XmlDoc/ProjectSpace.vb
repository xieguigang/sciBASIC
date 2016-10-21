#Region "Microsoft.VisualBasic::5ffc5d1bef09415b092ccba2f001da0d, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\XmlDoc\ProjectSpace.vb"

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

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Xml
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' A collection of one or more projects put together, and their attendant namespaces.
    ''' </summary>
    Public Class ProjectSpace
        Private projects As List(Of Project)

        Public Sub New()
            Me.projects = New List(Of Project)()
        End Sub

        Public Function GetProject(name As String) As Project
            For Each p As Project In Me.projects
                If p.Name.Equals(name) Then
                    Return p
                End If
            Next

            Return Nothing
        End Function

        Private Function EnsureProject(name As String) As Project
            Dim p As Project = Me.GetProject(name)

            If p Is Nothing Then
                p = New Project(name)

                Me.projects.Add(p)
            End If

            Return p
        End Function

        Public Sub ImportFromXmlDocFolder(path As String)
            If Not Directory.Exists(path) Then
                Throw New InvalidOperationException()
            End If

            Dim di As New DirectoryInfo(path)

            Dim files As IEnumerable(Of FileInfo) =
                (ls - l - wildcards("*.xml") <= path) _
                .Select(AddressOf FileIO.FileSystem.GetFileInfo)

            For Each fi As FileInfo In files
                Try
                    Call Me.LoadFile(fi)   ' 可能有其他的不是CLR Assembly XML的文件在这里，忽略掉这个错误
                Catch ex As Exception
                    ex = New Exception(fi.FullName, ex)
                    Call App.LogException(ex)
                End Try
            Next
        End Sub

        Public Sub ImportFromXmlDocFile(path As String)
            If Not path.FileExists Then
                Throw New InvalidOperationException()
            End If

            Dim fi As New FileInfo(path)

            Me.LoadFile(fi)
        End Sub

        Private Sub LoadFile(fi As FileInfo)
            Using fs As New FileStream(fi.FullName, FileMode.Open)
                Dim streamWriter As New StreamReader(fs)
                Dim s As New StringReader(Serialization.TrimAssemblyDoc(streamWriter.ReadToEnd))

                Using xr As XmlReader = XmlReader.Create(s)
                    Dim xd As New XmlDocument()

                    xd.Load(xr)

                    Dim nameNode As XmlNode = xd.DocumentElement.SelectSingleNode("assembly/name")

                    If nameNode IsNot Nothing Then
                        Dim p As Project = Me.EnsureProject(nameNode.InnerText)

                        p.ProcessXmlDoc(xd)
                    End If
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="folderPath"></param>
        ''' <param name="pageTemplate">a markdown page template. This token: [content] will be replaced with generated content.</param>
        Public Sub ExportMarkdownFiles(folderPath As String, pageTemplate As String, Optional hexoPublish As Boolean = False)
            For Each p As Project In Me.projects
                For Each pn As ProjectNamespace In p.Namespaces
                    pn.ExportMarkdownFile(folderPath, pageTemplate, hexoPublish)

                    For Each pt As ProjectType In pn.Types
                        pt.ExportMarkdownFile(folderPath, pageTemplate, hexoPublish)
                    Next
                Next
            Next
        End Sub

        Public Const TemplateToken As String = "[content]"

        Public Function ExportMarkdownFiles(folderPath As String, Optional hexoPublish As Boolean = False) As Boolean
            ExportMarkdownFiles(folderPath, TemplateToken, hexoPublish)
            Return BuildIndex(folderPath, hexoPublish)
        End Function

        Public Function BuildIndex(out As String, Optional hexoPublish As Boolean = False) As Boolean
            Dim path As String = out & "/index.md"
            Dim allns As String() =
                LinqAPI.Exec(Of String) <= From x As Project
                                           In projects
                                           Select x.Namespaces.Select(Function(ns) ns.Path)

            Dim ext As String = If(hexoPublish, ".html", ".md")
            Dim links As String() = allns.OrderBy(Function(ns) ns).ToArray(Function(ns) $"+ [{ns}](N-{ns}{ext})")
            Dim sb As String = "Browser by namespace:" & vbCrLf & vbCrLf & links.JoinBy(vbCrLf)

            If hexoPublish Then
                sb = $"---
title: API index
date: {Now.ToString}
---

" & sb
            End If

            Return sb.SaveTo(path)
        End Function
    End Class
End Namespace
