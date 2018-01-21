#Region "Microsoft.VisualBasic::a5c5bd319d3e3cb5bbd1ea2d5222f6ae, ..\sciBASIC#\Microsoft.VisualBasic.Core\ApplicationServices\VBDev\XmlDoc\ProjectSpace.vb"

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

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.IO
Imports System.Xml
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace ApplicationServices.Development.XmlDoc.Assembly

    ''' <summary>
    ''' A collection of one or more projects put together, and their attendant namespaces.
    ''' </summary>
    Public Class ProjectSpace

        Dim projects As New List(Of Project)
        Dim handle$

        Public Overrides Function ToString() As String
            Return handle
        End Function

        Public Function GetProject(name As String) As Project
            For Each p As Project In Me.projects
                If p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) Then
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
            Else
                handle = path
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
            Else
                handle = path
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

                    Call xd.Load(xr)

                    Dim nameNode As XmlNode = xd.DocumentElement.SelectSingleNode("assembly/name")

                    If nameNode IsNot Nothing Then
                        Dim p As Project = Me.EnsureProject(nameNode.InnerText)

                        p.ProcessXmlDoc(xd)
                    End If
                End Using
            End Using
        End Sub
    End Class
End Namespace
