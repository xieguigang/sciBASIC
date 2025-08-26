#Region "Microsoft.VisualBasic::9ebcd1e667a37b88ea0cd0cf52c620b0, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\ProjectSpace.vb"

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

    '   Total Lines: 154
    '    Code Lines: 106 (68.83%)
    ' Comment Lines: 18 (11.69%)
    '    - Xml Docs: 61.11%
    ' 
    '   Blank Lines: 30 (19.48%)
    '     File Size: 5.73 KB


    '     Class ProjectSpace
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateDocProject, EnsureProject, GetEnumerator, GetProject, IEnumerable_GetEnumerator
    '                   ToString
    ' 
    '         Sub: ImportFromXmlDocFile, ImportFromXmlDocFolder, LoadFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.FileIO.FileSystem
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace ApplicationServices.Development.XmlDoc.Assembly

    ''' <summary>
    ''' A collection of one or more projects put together, and their attendant namespaces.
    ''' </summary>
    Public Class ProjectSpace : Implements IEnumerable(Of Project)

        Protected projects As New List(Of Project)
        Protected handle$

        ReadOnly excludeVBSpecific As Boolean

        Sub New(excludeVBSpecific As Boolean)
            Me.excludeVBSpecific = excludeVBSpecific
        End Sub

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

        ''' <summary>
        ''' Batch imports of <see cref="ImportFromXmlDocFile(String)"/>
        ''' </summary>
        ''' <param name="path"></param>
        Public Sub ImportFromXmlDocFolder(path As String)
            If Not Directory.Exists(path) Then
                Throw New InvalidOperationException(path)
            Else
                handle = path
            End If

            Dim files = (ls - l - "*.xml" <= path).Select(AddressOf GetFileInfo)

            For Each fi As FileInfo In files
                Try
                    Call Me.LoadFile(fi)
                    Call $"Loaded {fi}".debug
                Catch ex As Exception
                    ' 可能有其他的不是CLR Assembly XML的文件在这里，忽略掉这个错误
                    ex = New Exception(fi.FullName, ex)
                    Call App.LogException(ex)
                End Try
            Next
        End Sub

        ''' <summary>
        ''' Imports xdoc document data from a specific xml file.
        ''' </summary>
        ''' <param name="path"></param>
        Public Sub ImportFromXmlDocFile(path As String)
            If Not path.FileExists Then
                Throw New InvalidOperationException(path)
            Else
                handle = path
                LoadFile(New FileInfo(path))
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub LoadFile(fi As FileInfo)
            Call CreateDocProject(fi, excludeVBSpecific, AddressOf EnsureProject)
        End Sub

        Public Shared Function CreateDocProject(xmlfile As [Variant](Of String, FileInfo),
                                                Optional excludeVBSpecific As Boolean = True,
                                                Optional projActivator As Func(Of String, Project) = Nothing) As Project

            If xmlfile Like GetType(FileInfo) Then
                xmlfile = xmlfile.TryCast(Of FileInfo).FullName
            Else
                xmlfile = xmlfile.TryCast(Of String).GetFullPath
            End If

            If projActivator Is Nothing Then
                projActivator = Function(name) New Project(name)
            End If

            ' 20200423
            ' file should be read access, or
            ' System.UnauthorizedAccessException: Access to the path "/usr/local/share/R_bin/REnv.xml" is denied.
            ' on linux platform
            Using fs As New FileStream(xmlfile, FileMode.Open, access:=FileAccess.Read)
                Dim streamWriter As New StreamReader(fs)
                Dim xml$ = streamWriter.ReadToEnd.TrimAssemblyDoc()
                Dim s As New StringReader(xml)

                Using xr As XmlReader = XmlReader.Create(s)
                    Dim xd As New XmlDocument()

                    Call xd.Load(xr)

                    Dim nameNode As XmlNode = xd _
                        .DocumentElement _
                        .SelectSingleNode("assembly/name")
                    Dim proj As Project

                    If nameNode IsNot Nothing Then
                        xml = nameNode.InnerText
                        proj = projActivator(xml)

                        Call proj.ProcessXmlDoc(xd, excludeVBSpecific)

                        Return proj
                    Else
                        Return Nothing
                    End If
                End Using
            End Using
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Project) Implements IEnumerable(Of Project).GetEnumerator
            For Each proj As Project In projects
                Yield proj
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
