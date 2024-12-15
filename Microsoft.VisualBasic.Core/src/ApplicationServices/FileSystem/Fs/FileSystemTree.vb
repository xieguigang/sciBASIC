#Region "Microsoft.VisualBasic::d2b5f66895679cc7bb5d8563a1188eae, Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\Fs\FileSystemTree.vb"

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

    '   Total Lines: 138
    '    Code Lines: 82 (59.42%)
    ' Comment Lines: 35 (25.36%)
    '    - Xml Docs: 97.14%
    ' 
    '   Blank Lines: 21 (15.22%)
    '     File Size: 4.60 KB


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
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices

    ''' <summary>
    ''' A virtual filesystem tree
    ''' </summary>
    Public Class FileSystemTree : Implements Enumeration(Of String)

        Public Property Name As String
        Public Property Files As Dictionary(Of String, FileSystemTree)
        ''' <summary>
        ''' the parent folder
        ''' </summary>
        ''' <returns></returns>
        Public Property Parent As FileSystemTree
        Public Property data As Object

        ''' <summary>
        ''' absolute full path
        ''' </summary>
        ''' <returns></returns>
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
        ''' Get a node by its key name in current tree node
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

        ''' <summary>
        ''' absolute full path
        ''' </summary>
        ''' <returns></returns>
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
        ''' common shared method for get node by path
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

        Public Iterator Function GenericEnumerator() As IEnumerator(Of String) Implements Enumeration(Of String).GenericEnumerator
            Yield FullName

            If Files.IsNullOrEmpty Then
                Return
            End If

            For Each file As FileSystemTree In Files.Values
                For Each subfile As String In file.AsEnumerable
                    Yield subfile
                Next
            Next
        End Function
    End Class
End Namespace
