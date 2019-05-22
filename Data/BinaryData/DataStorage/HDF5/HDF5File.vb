Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace HDF5

    Public Class HDF5File : Implements IDisposable

        Friend ReadOnly reader As BinaryReader

        ''' <summary>
        ''' Full name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property fileName As String

        Public ReadOnly Property superblock As Superblock
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Superblock(Me, Scan0)
            End Get
        End Property

        ''' <summary>
        ''' 最顶端的数据对象，可能是一个数据块也可能是一个文件夹
        ''' </summary>
        Dim rootObjects As Dictionary(Of String, DataObjectFacade)
        ''' <summary>
        ''' 所读取的对象缓存: [address => <see cref="DataObject"/>]
        ''' </summary>
        Dim objectAddressMap As New Dictionary(Of Long, DataObject)()

        ''' <summary>
        ''' 根节点名称或者全路径来获取一个数据集对象
        ''' </summary>
        ''' <param name="symbolName"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property GetObject(symbolName As String) As HDF5Reader
            Get
                Dim path As String() = symbolName _
                    .Replace("\", "/") _
                    .Split("/"c) _
                    .Where(Function(t) Not t.StringEmpty) _
                    .ToArray
                Dim rootName$ = path(Scan0)
                Dim obj As DataObjectFacade = rootObjects _
                    .Keys _
                    .First(Function(name) name.TextEquals(rootName)) _
                    .GetValueOrDefault(rootObjects)
                Dim reader As New HDF5Reader(Me, obj)

                For Each token As String In path.Skip(1)
                    reader = reader.ParseDataObject(dataSetName:=token)
                Next

                Return reader
            End Get
        End Property

        Public ReadOnly Property attributes As Dictionary(Of String, Object)

        Sub New(fileName As String)
            Me.reader = New BinaryFileReader(fileName)
            Me.fileName = fileName.GetFullPath

            Call parseHeader()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub addCache(data As DataObject)
            objectAddressMap(key:=data.address) = data
        End Sub

        ''' <summary>
        ''' 获取已经被缓存的数据对象，如果目标还没有被读取的话，则返回空值
        ''' </summary>
        ''' <param name="address"></param>
        ''' <returns></returns>
        Public Function GetCacheObject(address As Long) As DataObject
            If Not objectAddressMap.ContainsKey(address) Then
                Return Nothing
            Else
                Return objectAddressMap(key:=address)
            End If
        End Function

        Private Sub parseHeader()
            Dim sb As Superblock = Me.superblock
            Dim rootSymbolTableEntry As SymbolTableEntry = sb.rootGroupSymbolTableEntry
            Dim objectFacade As New DataObjectFacade(Me.reader, sb, "root", rootSymbolTableEntry.objectHeaderAddress)
            Dim rootGroup As New Group(Me.reader, sb, objectFacade)
            Dim objects As List(Of DataObjectFacade) = rootGroup.objects

            _attributes = attributeTable(rootGroup.attributes, Me)
            rootObjects = objects.ToDictionary(Function(o) o.symbolName)
        End Sub

        Public Shared Function attributeTable(attrs As AttributeMessage(), file As HDF5File) As Dictionary(Of String, Object)
            Dim table As New Dictionary(Of String, Object)
            Dim reader = file.reader
            Dim sb As Superblock = file.superblock

            For Each a As AttributeMessage In attrs
                table(a.name) = AttributeMessage.ReadAttrValue(reader, a, sb)
            Next

            Return table
        End Function

        Public Overrides Function ToString() As String
            Return fileName
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
                    Call reader.Dispose()
                End If

                ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
                ' TODO: 将大型字段设置为 null。
            End If
            disposedValue = True
        End Sub

        ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码以正确实现可释放模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
            Dispose(True)
            ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace