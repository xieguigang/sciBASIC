﻿#Region "Microsoft.VisualBasic::7158a0eb4617fb997bc45868f36cfcc9, Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\FileSystemTree.vb"

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

    '   Total Lines: 134
    '    Code Lines: 82 (61.19%)
    ' Comment Lines: 31 (23.13%)
    '    - Xml Docs: 90.32%
    ' 
    '   Blank Lines: 21 (15.67%)
    '     File Size: 4.40 KB


    '     Class FileSystemTree
    ' 
    '         Properties: data, Files, FullName, Name, Parent
    ' 
    '         Function: AddFile, BuildTree, DeleteFile, (+2 Overloads) GetFile, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.FileIO.Path

Namespace ApplicationServices

    ''' <summary>
    ''' A virtual filesystem tree
    ''' </summary>
    Public Class FileSystemTree

        Public Property Name As String
        Public Property Files As Dictionary(Of String, FileSystemTree)
        Public Property Parent As FileSystemTree
        Public Property data As Object

        Public ReadOnly Property FullName As String
            Get
                If Parent Is Nothing OrElse Parent Is Me Then
                    Return $"/{Name}"
                Else
                    Return $"{Parent}/{Name}"
                End If
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' returns nothing if the file with given <paramref name="name"/> was could not be found
        ''' </returns>
        Public Function GetFile(name As String) As FileSystemTree
            Return Files.TryGetValue(name)
        End Function

        ''' <summary>
        ''' add a new filesystem node and then returns the new node object.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function AddFile(name As String) As FileSystemTree
            Dim node As New FileSystemTree With {
                .Name = name,
                .Files = New Dictionary(Of String, FileSystemTree),
                .Parent = Me
            }
            Files.Add(name, node)
            Return node
        End Function

        Public Overrides Function ToString() As String
            Return FullName
        End Function

        Public Shared Function BuildTree(files As IEnumerable(Of String)) As FileSystemTree
            Dim root As New FileSystemTree With {
                .Files = New Dictionary(Of String, FileSystemTree),
                .Parent = Nothing,
                .Name = Nothing
            }
            Dim tokens As String()
            Dim dir As FileSystemTree
            Dim node As FileSystemTree

            For Each path As String In files
                tokens = FilePath.ParseTokens(path)
                dir = root

                For Each name As String In tokens
                    node = dir.GetFile(name)

                    If node Is Nothing Then
                        dir = dir.AddFile(name)
                    Else
                        dir = node
                    End If
                Next

                dir.data = path
            Next

            Return root
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="fs"></param>
        ''' <param name="path"></param>
        ''' <returns>
        ''' this function returns nothing if the <paramref name="path"/> not found.
        ''' </returns>
        Public Shared Function GetFile(fs As FileSystemTree, path As String) As FileSystemTree
            Dim tokens As String() = FilePath.ParseTokens(path)
            Dim dir As FileSystemTree = fs
            Dim node As FileSystemTree

            For Each name As String In tokens
                node = dir.GetFile(name)

                If node Is Nothing Then
                    Return Nothing
                Else
                    dir = node
                End If
            Next

            Return dir
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="fs"></param>
        ''' <param name="path"></param>
        ''' <returns>
        ''' this function returns nothing if the required node has not been found, and returns the node which has been deleted.
        ''' </returns>
        Public Shared Function DeleteFile(fs As FileSystemTree, path As String) As FileSystemTree
            Dim parent_dir As String = path.ParentPath
            Dim dir_node = GetFile(fs, parent_dir)

            If dir_node Is Nothing Then
                Return Nothing
            End If

            Dim node_name As String = path.FileName
            Dim node As FileSystemTree = dir_node.Files.TryGetValue(node_name)
            dir_node.Files.Remove(node_name)
            Return node
        End Function

    End Class
End Namespace
