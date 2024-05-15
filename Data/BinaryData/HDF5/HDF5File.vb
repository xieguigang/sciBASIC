#Region "Microsoft.VisualBasic::3f1ed8d1dc8fd4f65399b37891f0759f, Data\BinaryData\HDF5\HDF5File.vb"

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

    '   Total Lines: 184
    '    Code Lines: 101
    ' Comment Lines: 52
    '   Blank Lines: 31
    '     File Size: 7.30 KB


    ' Class HDF5File
    ' 
    '     Properties: attributes, fileName, superblock
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: attributeTable, GetCacheObject, Open, ToString
    ' 
    '     Sub: addCache, (+2 Overloads) Dispose, parseHeader
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Public Class HDF5File : Implements IDisposable

    Friend ReadOnly reader As BinaryReader

    ''' <summary>
    ''' Full name
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property fileName As String

    ''' <summary>
    ''' The superblock may begin at certain predefined offsets within the HDF5 file, allowing a 
    ''' block of unspecified content for users to place additional information at the beginning 
    ''' (and end) of the HDF5 file without limiting the HDF5 Library's ability to manage the 
    ''' objects within the file itself. This feature was designed to accommodate wrapping an 
    ''' HDF5 file in another file format or adding descriptive information to an HDF5 file without 
    ''' requiring the modification of the actual file's information. The superblock is located 
    ''' by searching for the HDF5 format signature at byte offset 0, byte offset 512, and at 
    ''' successive locations in the file, each a multiple of two of the previous location; 
    ''' in other words, at these byte offsets: 0, 512, 1024, 2048, and so on.
    '''
    ''' The superblock Is composed Of the format signature, followed by a superblock version number 
    ''' And information that Is specific To Each version Of the superblock.
    ''' </summary>
    Public ReadOnly Property superblock As Superblock

    ''' <summary>
    ''' 最顶端的数据对象，可能是一个数据块也可能是一个文件夹
    ''' </summary>
    Dim rootObjects As Dictionary(Of String, DataObjectFacade)
    ''' <summary>
    ''' 所读取的对象缓存: [address => <see cref="DataObject"/>]
    ''' </summary>
    Dim objectAddressMap As New Dictionary(Of Long, DataObject)()

    Friend ReadOnly globalHeaps As New Dictionary(Of Long, GlobalHeap)

    ''' <summary>
    ''' 根节点名称或者全路径来获取一个数据集对象
    ''' </summary>
    ''' <param name="symbolName">
    ''' + 如果只提供一个名称的话，则只会在根节点下面进行查找
    ''' + 如果提供的是一个全路径的话，则会依据这个路径依次进行查找操作
    ''' </param>
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

                If reader Is Nothing Then
                    ' 数据集或者文件夹对象不存在
                    Exit For
                End If
            Next

            Return reader
        End Get
    End Property

    Public ReadOnly Property attributes As Dictionary(Of String, Object)

    Sub New(fileName As String)
        Me.reader = New BinaryFileReader(fileName)
        Me.fileName = fileName.GetFullPath
        Me.superblock = New Superblock(Me, address:=Scan0)

        Call parseHeader()
    End Sub

    Sub New(buffer As Stream)
        Me.reader = New BinaryFileReader(buffer)
        Me.fileName = buffer.ToString
        Me.superblock = New Superblock(Me, address:=Scan0)

        If TypeOf buffer Is FileStream Then
            Me.fileName = DirectCast(buffer, FileStream).Name
        End If

        Call parseHeader()
    End Sub

    Public Shared Function Open(path As String) As HDF5File
        Return New HDF5File(path)
    End Function

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
        Dim objectFacade As New DataObjectFacade(sb, "root", rootSymbolTableEntry.objectHeaderAddress)
        Dim rootGroup As New Group(sb, objectFacade)
        Dim objects As List(Of DataObjectFacade) = rootGroup.objects

        _attributes = attributeTable(rootGroup.attributes, Me.superblock)
        rootObjects = objects.ToDictionary(Function(o) o.symbolName)
    End Sub

    Public Shared Function attributeTable(attrs As AttributeMessage(), sb As Superblock) As Dictionary(Of String, Object)
        Dim table As New Dictionary(Of String, Object)
        Dim reader As BinaryReader = sb.FileReader(-1)

        For Each a As AttributeMessage In attrs
            table(a.name) = AttributeMessage.ReadAttrValue(a, sb)
        Next

        Return table
    End Function

    Public Overrides Function ToString() As String
        Return fileName
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' 要检测冗余调用

    ' IDisposable
    Protected Sub Dispose(disposing As Boolean)
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
