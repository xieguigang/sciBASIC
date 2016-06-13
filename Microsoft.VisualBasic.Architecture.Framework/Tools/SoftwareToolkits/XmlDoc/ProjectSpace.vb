' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Xml

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' A collection of one or more projects put together, and their attendant namespaces.
    ''' </summary>
    Public Class ProjectSpace
        Private projects As List(Of Project)

        Public Sub New()
            Me.projects = New List(Of Project)()
        End Sub

        Public Function GetProject(name As [String]) As Project
            For Each p As Project In Me.projects
                If p.Name.Equals(name) Then
                    Return p
                End If
            Next

            Return Nothing
        End Function

        Private Function EnsureProject(name As [String]) As Project
            Dim p As Project = Me.GetProject(name)

            If p Is Nothing Then
                p = New Project(name)

                Me.projects.Add(p)
            End If

            Return p
        End Function

        Public Sub ImportFromXmlDocFolder(path As [String])
            If Not Directory.Exists(path) Then
                Throw New InvalidOperationException()
            End If

            Dim di As New DirectoryInfo(path)

            Dim files As FileInfo() = di.GetFiles()

            For Each fi As FileInfo In files
                Me.LoadFile(fi)
            Next
        End Sub

        Public Sub ImportFromXmlDocFile(path As [String])
            If Not File.Exists(path) Then
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
        Public Sub ExportMarkdownFiles(folderPath As [String], pageTemplate As [String])
            For Each p As Project In Me.projects
                For Each pn As ProjectNamespace In p.Namespaces
                    pn.ExportMarkdownFile(folderPath, pageTemplate)

                    For Each pt As ProjectType In pn.Types
                        pt.ExportMarkdownFile(folderPath, pageTemplate)
                    Next
                Next
            Next
        End Sub

        Public Const TemplateToken As String = "[content]"

        Public Sub ExportMarkdownFiles(folderPath As [String])
            Call ExportMarkdownFiles(folderPath, TemplateToken)
        End Sub
    End Class
End Namespace