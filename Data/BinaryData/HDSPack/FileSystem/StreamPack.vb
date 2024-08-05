#Region "Microsoft.VisualBasic::5aeee2c22344250050ff9a53c33d4058, Data\BinaryData\HDSPack\FileSystem\StreamPack.vb"

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

    '   Total Lines: 615
    '    Code Lines: 354 (57.56%)
    ' Comment Lines: 180 (29.27%)
    '    - Xml Docs: 71.67%
    ' 
    '   Blank Lines: 81 (13.17%)
    '     File Size: 25.28 KB


    '     Class StreamPack
    ' 
    '         Properties: files, globalAttributes, is_readonly, superBlock
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Allocate, AllocateNext, CreateNewStream, Delete, FileExists
    '                   FileSize, (+2 Overloads) GetFiles, GetFullPath, GetGlobalAttribute, GetObject
    '                   (+2 Overloads) OpenBlock, OpenFile, OpenReadOnly, ParseTree, ReadAllText
    '                   (+2 Overloads) TestMagic, ToString, WriteText
    ' 
    '         Sub: Clear, Close, (+2 Overloads) Dispose, Flush, flushStreamPack
    '              ParseMetadata, (+2 Overloads) SetAttribute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.FileIO.Path
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.Net.Http

Namespace FileSystem

    ''' <summary>
    ''' Hierarchical Data Stream Pack, A hdf5 liked file format
    ''' </summary>
    Public Class StreamPack : Implements IFileSystemEnvironment
        Implements IDisposable

        Public ReadOnly Property superBlock As StreamGroup
        Public ReadOnly Property globalAttributes As New LazyAttribute
        Public ReadOnly Property is_readonly As Boolean Implements IFileSystemEnvironment.readonly

        ''' <summary>
        ''' usually be the underlying local file stream for read/write pack data
        ''' </summary>
        ReadOnly buffer As Stream
        ReadOnly init_size As Integer

        ''' <summary>
        ''' the type list of the values in the <see cref="globalAttributes"/> data,
        ''' a messagepack schema should be defined for these types.
        ''' </summary>
        ReadOnly _registriedTypes As New Index(Of String)

        Dim disposedValue As Boolean

        Public Const Magic As String = "HDS"

        Public ReadOnly Property isDiskFile As Boolean
            Get
                Return TypeOf buffer Is FileStream
            End Get
        End Property

        ''' <summary>
        ''' gets the file path of the underlying stream data of current stream pack object
        ''' </summary>
        ''' <returns>
        ''' this property will returns the file path string if the underlying stream 
        ''' data is <see cref="isDiskFile"/>, otherwise nothing will be returns.
        ''' </returns>
        Public ReadOnly Property filepath As String
            Get
                If isDiskFile Then
                    Return DirectCast(buffer, FileStream).Name
                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' <summary>
        ''' get all data files inside this hds data 
        ''' pack, not includes directory.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property files As StreamBlock()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return superBlock _
                    .ListFiles _
                    .Where(Function(f) TypeOf f Is StreamBlock) _
                    .Select(Function(f) DirectCast(f, StreamBlock)) _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' open or create new stream pack data object
        ''' </summary>
        ''' <param name="filepath"></param>
        ''' <param name="init_size"></param>
        ''' <param name="meta_size">
        ''' default 1MB for save the header data
        ''' </param>
        Sub New(filepath As String,
                Optional init_size As Integer = 1024,
                Optional meta_size As Long = 1024 * 1024,
                Optional createNew As Boolean = False,
                Optional [readonly] As Boolean = False)

            Call Me.New(
                buffer:=filepath.Open(
                    mode:=FileMode.OpenOrCreate,
                    doClear:=createNew,
                    [readOnly]:=False
                ),
                init_size:=init_size,
                meta_size:=meta_size,
                [readonly]:=[readonly]
            )
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="init_size"></param>
        ''' <param name="meta_size">
        ''' the size in bytes of the tree header data
        ''' </param>
        Sub New(buffer As Stream,
                Optional init_size As Integer = 1024,
                Optional meta_size As Long = 1024 * 1024,
                Optional [readonly] As Boolean = False)

            Me.is_readonly = [readonly]
            Me.buffer = buffer
            Me.init_size = init_size

            If Not buffer.CanSeek AndAlso [readonly] Then
                ' zip stream, probably...
                ' convert to memory stream
                Dim ms As New MemoryStream

                Call buffer.CopyTo(ms)
                Call buffer.Dispose()
                Call ms.Seek(Scan0, SeekOrigin.Begin)

                Me.buffer = ms
            End If

            If Me.buffer.Length > 128 Then
                superBlock = ParseTree()
            Else
                Call Clear(meta_size)
            End If
        End Sub

        Public Function Delete(path As String) As Boolean Implements IFileSystemEnvironment.DeleteFile
            Dim dir As String = path.ParentPath(full:=False) & "/"
            Dim name As String = path.FileName
            Dim folder = GetObject(dir)

            If folder Is Nothing Then
                ' folder is already missing, skip
                Return True
            End If

            If TypeOf folder Is StreamGroup Then
                Call DirectCast(folder, StreamGroup).DeleteNode(name)
            End If

            Return True
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="meta_size">
        ''' the size in bytes of the tree header data
        ''' </param>
        Public Sub Clear(Optional meta_size As Long = 1024 * 1024)
            _superBlock = StreamGroup.CreateRootTree
            _globalAttributes = New LazyAttribute
            _registriedTypes.Clear()

            If Not is_readonly Then
                Call buffer.Seek(Scan0, SeekOrigin.Begin)
                Call buffer.Write(Encoding.ASCII.GetBytes(Magic), Scan0, Magic.Length)
                Call buffer.SetLength(Magic.Length + meta_size)
                Call buffer.Flush()
            End If
        End Sub

        Public Function GetGlobalAttribute(name As String) As Object
            If globalAttributes.attributes.ContainsKey(name) Then
                Return LazyAttribute.GetValue(globalAttributes.attributes(name))
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' set global attributes
        ''' </summary>
        ''' <param name="attrs"></param>
        Public Sub SetAttribute(ParamArray attrs As NamedValue(Of Object)())
            For Each val As NamedValue(Of Object) In attrs
                If Not val.Value Is Nothing Then
                    Dim code As String = val.GetType.FullName

                    If Not code Like _registriedTypes Then
                        Call _registriedTypes.Add(code)
                    End If
                End If

                Call globalAttributes.Add(val.Name, val.Value)
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="fileName">
        ''' the dir object its file name must be ends with the symbol '\' or '/'
        ''' </param>
        ''' <param name="attrs">
        ''' all attribute value will be serialized via messagepack
        ''' </param>
        Public Sub SetAttribute(fileName As String, attrs As Dictionary(Of String, Object))
            Dim reference As New FilePath(fileName)
            Dim file As StreamObject = superBlock.GetObject(reference)

            For Each val As Object In attrs.Values
                If Not val Is Nothing Then
                    Dim code As String = val.GetType.FullName

                    If Not code Like _registriedTypes Then
                        Call _registriedTypes.Add(code)
                    End If
                End If
            Next

            If file Is Nothing Then
                Throw New MissingPrimaryKeyException(fileName)
            Else
                Call file.AddAttributes(attrs)
            End If
        End Sub

        ''' <summary>
        ''' get debug view of current data pack object
        ''' </summary>
        ''' <returns>
        ''' </returns>
        Public Overrides Function ToString() As String
            If TypeOf buffer Is FileStream Then
                Return DirectCast(buffer, FileStream).Name.FileName
            ElseIf TypeOf buffer Is MemoryStream Then
                Return $"memory://{buffer.GetHashCode}<size={StringFormats.Lanudry(buffer.Length)}>"
            Else
                Return buffer.ToString
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function TestMagic(magic As Byte()) As Boolean
            Return Encoding.ASCII.GetString(magic) = StreamPack.Magic
        End Function

        Public Shared Function TestMagic(buffer As Stream) As Boolean
            Dim magic As Byte() = New Byte(StreamPack.Magic.Length - 1) {}
            Call buffer.Read(magic, Scan0, magic.Length)
            Return Encoding.ASCII.GetString(magic) = StreamPack.Magic
        End Function

        Private Function ParseTree() As StreamGroup
            ' verify data at first
            Dim is_magic As Boolean = TestMagic(buffer)
            Dim registry As New Dictionary(Of String, String)

            If Not is_magic Then
                Throw New FormatException("invalid magic header!")
            Else
                Call ParseMetadata(buffer, registry)
            End If

            ' and then parse filesystem tree
            Return TreeParser.Parse(buffer, registry)
        End Function

        Private Sub ParseMetadata(buffer As Stream, registry As Dictionary(Of String, String))
            Dim bin As New BinaryDataReader(buffer) With {.ByteOrder = ByteOrder.BigEndian}
            Dim bufSize As Integer = bin.ReadInt32
            Dim buf As Byte() = bin.ReadBytes(bufSize)

            If Not buf.IsNullOrEmpty Then
                For Each type As NamedValue(Of Integer) In New MemoryStream(buf).GetTypeRegistry
                    Call _registriedTypes.Add(type.Name, type.Value)
                    Call registry.Add(type.Value.ToString, type.Name)
                Next
            End If

            If Not bin.EndOfStream Then
                ' parse global attributes
                bufSize = bin.ReadInt32
                buf = bin.ReadBytes(bufSize)

                If buf.Length = 0 Then
                    _globalAttributes = New LazyAttribute
                Else
                    ' unpack global attributes from the HDS stream
                    _globalAttributes = New MemoryStream(buf).UnPack(Nothing, registry)
                End If
            Else
                _globalAttributes = New LazyAttribute
            End If
        End Sub

        ''' <summary>
        ''' Get target object and its corresponding attributes data
        ''' which is specified by the given <paramref name="fileName"/>
        ''' </summary>
        ''' <param name="fileName">
        ''' the dir object its file name must be ends with the symbol '\' or '/'
        ''' </param>
        ''' <returns>
        ''' returns nothing if object is not found!
        ''' </returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetObject(fileName As String) As StreamObject
            Return superBlock.GetObject(New FilePath(fileName), throw_err:=False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function OpenBlock(block As StreamBlock) As Stream
            Return New SubStream(buffer, block.offset, block.size)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="ZERO_Nonexists">
        ''' this parameter is not working at here
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FileExists(path As String, Optional ZERO_Nonexists As Boolean = False) As Boolean Implements IFileSystemEnvironment.FileExists
            Return superBlock.BlockExists(FilePath.Parse(path))
        End Function

        ''' <summary>
        ''' a more advanced wrapper for <see cref="OpenBlock(String, Integer)"/> function
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="mode">this parameter is no use in streampack</param>
        ''' <param name="access"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the parameter <paramref name="access"/> only supports data mode:
        ''' 
        ''' 1. <see cref="FileAccess.Read"/>
        ''' 2. <see cref="FileAccess.Write"/>
        ''' 
        ''' </remarks>
        Public Function OpenFile(path As String,
                                 Optional mode As FileMode = FileMode.OpenOrCreate,
                                 Optional access As FileAccess = FileAccess.Read) As Stream Implements IFileSystemEnvironment.OpenFile

            If access = FileAccess.ReadWrite Then
                Throw New InvalidOperationException("HDS Streampack is not supports read/write stream mode!")
            ElseIf access = FileAccess.Read Then
                Return OpenBlock(path)
            Else
                ' for write data
                ' delete file at first
                ' and then open new file for write?
                Delete(path)
                Return OpenBlock(path)
            End If
        End Function

        ''' <summary>
        ''' open a data block for read and write
        ''' 
        ''' if the target file block is missing from the tree, then this function 
        ''' will append a new file block based on the <see cref="is_readonly"/> 
        ''' flag is set to false or not, otherwise a substream object will be 
        ''' returned for read data
        ''' </summary>
        ''' <param name="fileName">
        ''' the dir object its file name must be ends with the symbol '\' or '/'
        ''' </param>
        ''' <param name="buffer_size">options for write data only</param>
        ''' <returns>
        ''' this function returns two type of the stream:
        ''' 
        ''' 1. <see cref="SubStream"/> for readonly, this kind of stream should not call the dispose method, or the based file stream will be closed too!
        ''' 2. <see cref="StreamBuffer"/> for writeonly, this kind of stream must be use the dispose method for commit data to based file stream
        ''' 
        ''' based on the target file object is existsed or not
        ''' </returns>
        Public Function OpenBlock(fileName As String, Optional buffer_size As Integer = -1) As Stream
            Dim path As New FilePath("/" & fileName)
            Dim block As StreamBlock

            If path.IsDirectory Then
                Throw New Exception($"can not open a directry({fileName}) as a data block!")
            End If

            If superBlock.BlockExists(path) Then
                ' get current object data
                block = superBlock.GetDataBlock(path)

                If App.MemoryLoad = MemoryLoads.Light Then
                    Return New SubStream(buffer, block.offset, block.size)
                Else
                    Dim ms As New MemoryStream()
                    Dim buf As Byte() = New Byte(block.size - 1) {}

                    Call buffer.Seek(block.offset, SeekOrigin.Begin)
                    Call buffer.Read(buf, Scan0, block.size)
                    Call ms.Write(buf, Scan0, buf.Length)
                    Call ms.Seek(Scan0, SeekOrigin.Begin)

                    Return ms
                End If
            ElseIf is_readonly Then
                Throw New ReadOnlyException($"can not create data block for the missing file '{path.ToString}' due to the reason of target stream is set readonly!")
            Else
                ' create file block with pre-allocated location region 
                ' if the buffer size is already known
                If buffer_size > 0 Then
                    With Allocate(buffer_size)
                        ' create a new empty data object
                        '
                        ' 20230625 IMPORTANT NOTE: the duplicated code show below is 
                        ' necessary!
                        ' do not add new data block before the allocate function
                        ' or the invalid offset will be produce!
                        block = superBlock.AddDataBlock(path)
                        block.offset = .position
                        block.size = .size
                    End With
                Else
                    ' just create a new data object
                    block = superBlock.AddDataBlock(path)
                End If

                Return New StreamBuffer(buffer, block, init_size)
            End If
        End Function

        ''' <summary>
        ''' try allocate a free block inside the file, not append to it
        ''' </summary>
        ''' <param name="buffer_size"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function may be too slow if too much file to process
        ''' </remarks>
        Public Function Allocate(buffer_size As Integer) As BufferRegion
            Dim files As StreamBlock() = superBlock _
                .ListFiles _
                .OfType(Of StreamBlock) _
                .OrderBy(Function(f) f.offset) _
                .ToArray

            If files.Length = 0 Then
                Return New BufferRegion(Me.buffer.Length, buffer_size)
            ElseIf files.Length = 1 Then
                Return AllocateNext(files(Scan0).GetRegion, buffer_size)
            End If

            For i As Integer = 0 To files.Length - 2
                Dim p0 As BufferRegion = files(i).GetRegion
                Dim p1 As BufferRegion = files(i + 1).GetRegion

                If p1.position - p0.nextBlock > buffer_size Then
                    Return AllocateNext(p0, buffer_size)
                End If
            Next

            Return AllocateNext(files.Last.GetRegion, buffer_size)
        End Function

        Private Shared Function AllocateNext(scan0 As BufferRegion, buffer_size As Integer) As BufferRegion
            Dim pNext As Long = scan0.nextBlock

            ' 20230625 do not do ofset padding at here
            ' otherwise the offset position after padding + buffer size will
            ' exceded the start position of the next block!
            ' pNext += 1
            ' pNext += pNext Mod 8

            ' raw
            ' --+---------+-----
            '             |
            ' -----+------|---+-
            ' result after offset padding may corrupt the data of next block

            Return New BufferRegion(pNext, buffer_size)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateNewStream(filepath As String,
                                               Optional init_size As Integer = 1024,
                                               Optional meta_size As Integer = 4096 * 1024) As StreamPack
            Return New StreamPack(
                buffer:=filepath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False),
                init_size:=init_size,
                meta_size:=meta_size
            )
        End Function

        Public Shared Function OpenReadOnly(filepath As String) As StreamPack
            Dim stream As Stream = filepath.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
            Dim pack As New StreamPack(stream, [readonly]:=True)
            Return pack
        End Function

        ''' <summary>
        ''' just write stream header data
        ''' </summary>
        Private Sub flushStreamPack()
            Dim treeMetadata As Byte() = New MemoryStream(superBlock.GetBuffer(_registriedTypes)).GZipStream.ToArray
            Dim registeryMetadata As Byte() = _registriedTypes.GetTypeCodes
            Dim globalMetadata As Byte() = globalAttributes.Pack(_registriedTypes)
            Dim size As Byte() = NetworkByteOrderBitConvertor.GetBytes(treeMetadata.Length)
            Dim size2 As Byte() = NetworkByteOrderBitConvertor.GetBytes(registeryMetadata.Length)
            Dim size3 As Byte() = NetworkByteOrderBitConvertor.GetBytes(globalMetadata.Length)

            Call buffer.Seek(Magic.Length, SeekOrigin.Begin)
            Call buffer.Write(size2, Scan0, size2.Length)
            Call buffer.Write(registeryMetadata, Scan0, registeryMetadata.Length)
            Call buffer.Write(size3, Scan0, size3.Length)
            Call buffer.Write(globalMetadata, Scan0, globalMetadata.Length)
            Call buffer.Write(size, Scan0, size.Length)
            Call buffer.Write(treeMetadata, Scan0, treeMetadata.Length)

            Call buffer.Flush()
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    If Not is_readonly Then
                        Call flushStreamPack()
                        Call buffer.Close()
                    Else
                        Call buffer.Dispose()
                    End If
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        ''' <summary>
        ''' 已经在dispose函数之中处理好所有文件数据保存，以及文件释放的工作，直接dispose即可
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>
        ''' Save the file tree and close the file stream
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Close() Implements IFileSystemEnvironment.Close
            Call Me.Dispose()
        End Sub

        Public Function FileSize(path As String) As Long Implements IFileSystemEnvironment.FileSize
            Dim stream As StreamBlock = Me.GetObject(path)

            If stream Is Nothing Then
                Return -1
            Else
                Return stream.size
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this method is a safe method, it alwasy delete the 
        ''' target file stream at first and then do text write
        ''' </remarks>
        Private Function WriteText(text As String, path As String) As Boolean Implements IFileSystemEnvironment.WriteText
            Call Delete(path)
            Return Extensions.WriteText(Me, text, path)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function ReadAllText(path As String) As String Implements IFileSystemEnvironment.ReadAllText
            Return Extensions.ReadText(Me, path)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub Flush() Implements IFileSystemEnvironment.Flush
            Call flushStreamPack()
        End Sub

        ''' <summary>
        ''' Get all files inside the given folder path
        ''' </summary>
        ''' <param name="dir"></param>
        ''' <returns>
        ''' this function returns an empty collection of the file 
        ''' path string if the given <paramref name="dir"/> is not 
        ''' exists in the archive file tree or its value is 
        ''' nothing
        ''' </returns>
        Public Function GetFiles(dir As String) As IEnumerable(Of String)
            Dim ls = Me.GetObject(dir & "/")

            If ls Is Nothing Then
                Return New String() {}
            End If

            Return From obj As StreamObject
                   In DirectCast(ls, StreamGroup).ListFiles()
                   Where TypeOf obj Is StreamBlock
                   Let filepath As String = obj.referencePath.ToString
                   Select filepath
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFiles() As IEnumerable(Of String) Implements IFileSystemEnvironment.GetFiles
            Return Me.ListFiles _
                .Where(Function(b) TypeOf b Is StreamBlock) _
                .Select(Function(f)
                            Return f.referencePath.ToString
                        End Function)
        End Function

        Public Function GetFullPath(filename As String) As String Implements IFileSystemEnvironment.GetFullPath
            Return ("/" & filename).Replace("\", "/").StringReplace("[/]{2,}", "/")
        End Function
    End Class
End Namespace
