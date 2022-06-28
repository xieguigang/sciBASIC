
Imports System.Data
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.FileIO.Path

Namespace FileSystem

    ''' <summary>
    ''' Hierarchical Data Stream Pack, A hdf5 liked file format
    ''' </summary>
    Public Class StreamPack : Implements IDisposable

        Public ReadOnly Property superBlock As StreamGroup
        Public ReadOnly Property globalAttributes As New LazyAttribute

        ReadOnly buffer As Stream
        ReadOnly init_size As Integer
        ReadOnly registriedTypes As New Index(Of String)

        Dim disposedValue As Boolean

        Const magic As String = "HDS"

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

        Sub New(filepath As String, Optional init_size As Integer = 1024)
            Call Me.New(filepath.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False), init_size:=init_size)
        End Sub

        Sub New(buffer As Stream, Optional init_size As Integer = 1024)
            Me.buffer = buffer
            Me.init_size = init_size

            If buffer.Length > 0 Then
                superBlock = ParseTree()
            Else
                superBlock = StreamGroup.CreateRootTree

                Call buffer.Write(Encoding.ASCII.GetBytes(magic), Scan0, magic.Length)
                Call buffer.SetLength(magic.Length + 1024 * 1024)
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

                    If Not code Like registriedTypes Then
                        Call registriedTypes.Add(code)
                    End If
                End If

                Call globalAttributes.Add(val.Name, val.Value)
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="fileName"></param>
        ''' <param name="attrs">
        ''' all attribute value will be serialized via messagepack
        ''' </param>
        Public Sub SetAttribute(fileName As String, attrs As Dictionary(Of String, Object))
            Dim file As StreamObject = superBlock.GetObject(New FilePath(fileName))

            For Each val As Object In attrs.Values
                If Not val Is Nothing Then
                    Dim code As String = val.GetType.FullName

                    If Not code Like registriedTypes Then
                        Call registriedTypes.Add(code)
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
            Dim magic As Byte() = New Byte(StreamPack.magic.Length - 1) {}
            Dim registry As New Dictionary(Of String, String)

            Call buffer.Read(magic, Scan0, magic.Length)

            If Encoding.ASCII.GetString(magic) <> StreamPack.magic Then
                Throw New FormatException("invalid magic header!")
            Else
                Dim bin As New BinaryDataReader(buffer)
                Dim bufSize As Integer = bin.ReadInt32
                Dim buf As Byte() = bin.ReadBytes(bufSize)

                For Each type As NamedValue(Of Integer) In New MemoryStream(buf).GetTypeRegistry
                    Call registriedTypes.Add(type.Name, type.Value)
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

        Public Function GetObject(fileName As String) As StreamObject
            Return superBlock.GetObject(New FilePath(fileName))
        End Function

        Public Function OpenBlock(block As StreamBlock) As Stream
            Return New SubStream(buffer, block.offset, block.size)
        End Function

        ''' <summary>
        ''' open a data block for read and write
        ''' 
        ''' if the target file block is missing from the tree, then this function will append a new file block
        ''' otherwise a substream object will be returns for read data
        ''' </summary>
        ''' <param name="fileName"></param>
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

                Return New SubStream(buffer, block.offset, block.size)
            Else
                ' create a new data object
                block = superBlock.AddDataBlock(path)

                Return New StreamBuffer(buffer, block, init_size)
            End If
        End Function

        Public Shared Function CreateNewStream(filepath As String, Optional init_size As Integer = 1024) As StreamPack
            Return New StreamPack(filepath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False), init_size)
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Dim treeMetadata As Byte() = superBlock.GetBuffer(registriedTypes)
                    Dim registeryMetadata As Byte() = registriedTypes.GetTypeCodes
                    Dim globalMetadata As Byte() = globalAttributes.Pack(registriedTypes)
                    Dim size As Byte() = BitConverter.GetBytes(treeMetadata.Length)
                    Dim size2 As Byte() = BitConverter.GetBytes(registeryMetadata.Length)
                    Dim size3 As Byte() = BitConverter.GetBytes(globalMetadata.Length)

                    Call buffer.Seek(magic.Length, SeekOrigin.Begin)
                    Call buffer.Write(size2, Scan0, size2.Length)
                    Call buffer.Write(registeryMetadata, Scan0, registeryMetadata.Length)
                    Call buffer.Write(size3, Scan0, size3.Length)
                    Call buffer.Write(globalMetadata, Scan0, globalMetadata.Length)
                    Call buffer.Write(size, Scan0, size.Length)
                    Call buffer.Write(treeMetadata, Scan0, treeMetadata.Length)

                    Call buffer.Flush()
                    Call buffer.Close()
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