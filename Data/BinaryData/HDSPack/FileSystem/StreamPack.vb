#Region "Microsoft.VisualBasic::aad682d85c78e3e55cae40f8ece8efc3, sciBASIC#\Data\BinaryData\HDSPack\FileSystem\StreamPack.vb"

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

'   Total Lines: 280
'    Code Lines: 172
' Comment Lines: 64
'   Blank Lines: 44
'     File Size: 11.44 KB


'     Class StreamPack
' 
'         Properties: files, globalAttributes, superBlock
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: CreateNewStream, GetGlobalAttribute, GetObject, (+2 Overloads) OpenBlock, ParseTree
' 
'         Sub: Clear, (+2 Overloads) Dispose, (+2 Overloads) SetAttribute
' 
' 
' /********************************************************************************/

#End Region


Imports System.Data
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.FileIO.Path
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.Net.Http

Namespace FileSystem

    ''' <summary>
    ''' Hierarchical Data Stream Pack, A hdf5 liked file format
    ''' </summary>
    Public Class StreamPack : Implements IDisposable

        Public ReadOnly Property superBlock As StreamGroup
        Public ReadOnly Property globalAttributes As New LazyAttribute

        ReadOnly buffer As Stream
        ReadOnly init_size As Integer
        ReadOnly is_readonly As Boolean = False

        ''' <summary>
        ''' the type list of the values in the <see cref="globalAttributes"/> data,
        ''' a messagepack schema should be defined for these types.
        ''' </summary>
        ReadOnly _registriedTypes As New Index(Of String)

        Dim disposedValue As Boolean

        Public Const Magic As String = "HDS"

        ''' <summary>
        ''' get all data files inside this hds data 
        ''' pack, not includes directory.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property files As StreamBlock()
            Get
                Return superBlock _
                    .ListFiles _
                    .Where(Function(f) TypeOf f Is StreamBlock) _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="filepath"></param>
        ''' <param name="init_size"></param>
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

            If buffer.Length > 128 Then
                superBlock = ParseTree()
            Else
                Call Clear(meta_size)
            End If
        End Sub

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

            Call buffer.Seek(Scan0, SeekOrigin.Begin)
            Call buffer.Write(Encoding.ASCII.GetBytes(Magic), Scan0, Magic.Length)
            Call buffer.SetLength(Magic.Length + meta_size)
            Call buffer.Flush()
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

        Private Function ParseTree() As StreamGroup
            ' verify data at first
            Dim magic As Byte() = New Byte(StreamPack.Magic.Length - 1) {}
            Dim registry As New Dictionary(Of String, String)

            Call buffer.Read(magic, Scan0, magic.Length)

            If Encoding.ASCII.GetString(magic) <> StreamPack.Magic Then
                Throw New FormatException("invalid magic header!")
            Else
                Dim bin As New BinaryDataReader(buffer) With {.ByteOrder = ByteOrder.BigEndian}
                Dim bufSize As Integer = bin.ReadInt32
                Dim buf As Byte() = bin.ReadBytes(bufSize)

                For Each type As NamedValue(Of Integer) In New MemoryStream(buf).GetTypeRegistry
                    Call _registriedTypes.Add(type.Name, type.Value)
                    Call registry.Add(type.Value.ToString, type.Name)
                Next

                ' parse global attributes
                bufSize = bin.ReadInt32
                buf = bin.ReadBytes(bufSize)
                ' unpack global attributes from the HDS stream
                _globalAttributes = New MemoryStream(buf).UnPack(Nothing, registry)
            End If

            ' and then parse filesystem tree
            Return TreeParser.Parse(buffer, registry)
        End Function

        ''' <summary>
        ''' Get target object and its corresponding attributes data
        ''' which is specified by the given <paramref name="fileName"/>
        ''' </summary>
        ''' <param name="fileName">
        ''' the dir object its file name must be ends with the symbol '\' or '/'
        ''' </param>
        ''' <returns></returns>
        Public Function GetObject(fileName As String) As StreamObject
            Return superBlock.GetObject(New FilePath(fileName))
        End Function

        Public Function OpenBlock(block As StreamBlock) As Stream
            Return New SubStream(buffer, block.offset, block.size)
        End Function

        Public Function FileExists(path As String) As Boolean
            Return superBlock.BlockExists(FilePath.Parse(path))
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
        ''' <returns></returns>
        Public Function OpenBlock(fileName As String) As Stream
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
                ' create a new data object
                block = superBlock.AddDataBlock(path)

                Return New StreamBuffer(buffer, block, init_size)
            End If
        End Function

        Public Shared Function CreateNewStream(filepath As String,
                                               Optional init_size As Integer = 1024,
                                               Optional meta_size As Integer = 4096 * 1024) As StreamPack
            Return New StreamPack(
                buffer:=filepath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False),
                init_size:=init_size,
                meta_size:=meta_size
            )
        End Function

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
            Call buffer.Close()
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    If Not is_readonly Then
                        Call flushStreamPack()
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

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
