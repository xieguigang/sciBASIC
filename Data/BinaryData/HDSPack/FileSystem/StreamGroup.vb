#Region "Microsoft.VisualBasic::37127985fe21e4c531712f3819ae029a, Data\BinaryData\HDSPack\FileSystem\StreamGroup.vb"

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

    '   Total Lines: 258
    '    Code Lines: 152
    ' Comment Lines: 70
    '   Blank Lines: 36
    '     File Size: 9.70 KB


    '     Class StreamGroup
    ' 
    '         Properties: dirs, files, totalSize
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: AddDataBlock, AddDataGroup, BlockExists, CreateRootTree, GetDataBlock
    '                   GetDataGroup, GetObject, hasName, ToString, VisitBlock
    ' 
    '         Sub: DeleteNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.FileIO.Path

Namespace FileSystem

    ''' <summary>
    ''' a data folder
    ''' </summary>
    Public Class StreamGroup : Inherits StreamObject

        ''' <summary>
        ''' indexed by <see cref="FilePath.FileName"/>
        ''' </summary>
        ReadOnly tree As Dictionary(Of String, StreamObject)

        ''' <summary>
        ''' get total data size in current folder
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property totalSize As Long
            Get
                Dim size As Long

                For Each file In tree.Values
                    If TypeOf file Is StreamBlock Then
                        size += DirectCast(file, StreamBlock).size
                    Else
                        size += DirectCast(file, StreamGroup).totalSize
                    End If
                Next

                Return size
            End Get
        End Property

        ''' <summary>
        ''' get all file + dirs list in current dir root
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property files As StreamObject()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return tree.Values.ToArray
            End Get
        End Property

        Public ReadOnly Property dirs As StreamGroup()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return tree.Values _
                    .Where(Function(f) TypeOf f Is StreamGroup) _
                    .Select(Function(f) DirectCast(f, StreamGroup)) _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' create a new file tree
        ''' </summary>
        ''' <param name="directory">
        ''' the folder directory path
        ''' </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(directory As String)
            Call Me.New(directory.Split("/"c))
        End Sub

        ''' <summary>
        ''' create a new file tree
        ''' </summary>
        ''' <param name="dirs"></param>
        Sub New(dirs As IEnumerable(Of String))
            Call MyBase.New(New FilePath(dirs, isDir:=True, isAbs:=True))
            ' create a new empty folder tree
            tree = New Dictionary(Of String, StreamObject)
        End Sub

        Sub New(path As FilePath, tree As Dictionary(Of String, StreamObject))
            Call MyBase.New(path)
            ' assign the exists tree data
            Me.tree = tree
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasName(nodeName As String) As Boolean
            Return tree.ContainsKey(nodeName)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub DeleteNode(nodeName As String)
            Call tree.Remove(nodeName)
        End Sub

        ''' <summary>
        ''' get file
        ''' </summary>
        ''' <param name="filepath"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDataBlock(filepath As FilePath) As StreamBlock
            Return VisitBlock(filepath, checkExists:=False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDataGroup(filepath As FilePath) As StreamGroup
            Return VisitBlock(filepath, checkExists:=False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetObject(filepath As FilePath, Optional throw_err As Boolean = True) As StreamObject
            Return VisitBlock(filepath, checkExists:=Not throw_err)
        End Function

        ''' <summary>
        ''' Create a new file block on the fs tree
        ''' </summary>
        ''' <param name="filepath"></param>
        ''' <returns>
        ''' A file block object with blank allocation data, no postion and no data size, just
        ''' with the file path reference data
        ''' </returns>
        Public Function AddDataBlock(filepath As FilePath) As StreamBlock
            Dim createNew As New StreamBlock(filepath)
            ' then add current file block to the tree
            Dim dir As FilePath = filepath.ParentDirectory
            Dim dirBlock As StreamGroup = VisitBlock(dir, checkExists:=False)

            If dirBlock Is Nothing Then
                ' no dir tree
                dirBlock = AddDataGroup(dir)
            End If

            ' just add current file to fs tree
            Call dirBlock.tree.Add(filepath.FileName, createNew)

            Return createNew
        End Function

        Public Function AddDataGroup(filepath As FilePath) As StreamGroup
            Dim dir As StreamGroup = Me
            Dim file As StreamObject
            Dim names As String() = filepath.Components
            Dim name As String
            Dim targetName As String = filepath.FileName

            For i As Integer = 0 To names.Length - 1
                name = names(i)

                ' name is a file path component token
                If Not dir.hasName(name) Then
                    dir.tree.Add(name, New StreamGroup(filepath.Components.Take(i + 1)))
                    dir = DirectCast(dir.tree(name), StreamGroup)
                Else
                    file = dir.tree(name)

                    If TypeOf file Is StreamGroup Then
                        dir = DirectCast(file, StreamGroup)
                    Else
                        ' required a folder(file group)
                        ' but a file is exists?
                        Throw New InvalidProgramException($"a folder can not be created at location '{names.Take(i).JoinBy("/")}' due to the reason of there is a file whith the same name on the target location!")
                    End If
                End If
            Next

            Return dir
        End Function

        ''' <summary>
        ''' get file or directory
        ''' </summary>
        ''' <param name="filepath"></param>
        ''' <param name="checkExists">
        ''' just check the file is exists or not, do not throw any exception
        ''' </param>
        ''' <returns>
        ''' returns nothing if object not found!
        ''' </returns>
        Private Function VisitBlock(filepath As FilePath, checkExists As Boolean) As StreamObject
            Dim tree As Dictionary(Of String, StreamObject) = Me.tree
            Dim file As StreamObject
            Dim names As String() = filepath.Components
            Dim name As String
            ' root dir / contains no names
            Dim targetName As String = names.LastOrDefault

            If Me.referencePath = filepath Then
                Return Me
            End If

            For i As Integer = 0 To names.Length - 1
                name = names(i)

                If Not tree.ContainsKey(name) Then
                    Return Nothing
                Else
                    file = tree(name)

                    If TypeOf file Is StreamGroup Then
                        If i = names.Length - 1 AndAlso filepath.IsDirectory Then
                            Return file
                        Else
                            tree = DirectCast(file, StreamGroup).tree
                        End If
                    ElseIf i <> names.Length - 1 Then
                        ' required a folder(file group)
                        ' but a file is exists?
                        Return Nothing
                    End If
                End If
            Next

            If Not tree.ContainsKey(targetName) Then
                If checkExists Then
                    ' just check file is exists or not
                    Return Nothing
                Else
                    Throw New MissingPrimaryKeyException($"missing folder or file which is named '{targetName}', if you want to find a folder, then you must ensure that your object path is end with symbol '\' or '/'!")
                End If
            Else
                Return tree(targetName)
            End If
        End Function

        ''' <summary>
        ''' check file exists in current tree?
        ''' </summary>
        ''' <param name="filepath">
        ''' should be a path data which is relative
        ''' to current file tree node.
        ''' </param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BlockExists(filepath As FilePath) As Boolean
            Return Not VisitBlock(filepath, checkExists:=True) Is Nothing
        End Function

        Public Overrides Function ToString() As String
            Dim ndirs As Integer = Aggregate file As StreamObject
                                   In files
                                   Where TypeOf file Is StreamGroup
                                   Into Count
            Dim nfiles As Integer = files.Length - ndirs

            Return $"{MyBase.ToString} [{StringFormats.Lanudry(totalSize)}, {ndirs} dirs, {nfiles} files]"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateRootTree() As StreamGroup
            Return New StreamGroup("/")
        End Function

    End Class
End Namespace
