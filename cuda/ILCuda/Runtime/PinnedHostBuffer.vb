#Region "Microsoft.VisualBasic::5ecd493974c83c18bbe63ebe65faedf4, cuda\ILCuda\Runtime\PinnedHostBuffer.vb"

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

    '   Total Lines: 180
    '    Code Lines: 117 (65.00%)
    ' Comment Lines: 31 (17.22%)
    '    - Xml Docs: 25.81%
    ' 
    '   Blank Lines: 32 (17.78%)
    '     File Size: 7.39 KB


    '     Class PinnedHostBuffer
    ' 
    '         Properties: ByteSize, Count, Pointer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Read, ToString
    ' 
    '         Sub: Clear, CopyFrom, CopyTo, Dispose, ReadInto
    '              ThrowIfDisposed, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 页锁定（pinned / page-locked）主机内存
'
' 为什么需要它：
'   cuMemcpyHtoDAsync / cuMemcpyDtoHAsync 要求主机端内存是页锁定的，
'   否则驱动无法启动 DMA，只能退化成同步的分段拷贝，异步就失去意义。
'
'   这里用 cuMemAllocHost 向驱动申请真正的页锁定内存，并提供强类型的
'   读写入口：常见基元类型走 Marshal.Copy（一次拷贝），其余值类型退回
'   Marshal.StructureToPtr / PtrToStructure 的逐元素路径。
'
' 典型用法（用一条流把上传与计算重叠起来）：
'   Using stage As New PinnedHostBuffer(Of Single)(n),
'         stream As New CudaStream()
'       stage.Write(values)
'       device.WriteAsync(stage, stream)
'       kernel.Launch(..., stream, device, n)
'       stream.Synchronize()
'   End Using
' ------------------------------------------------------------------------

Imports System.Runtime.InteropServices

Namespace Runtime

    ''' <summary>一段页锁定的主机内存，用作异步拷贝的中转区</summary>
    Public NotInheritable Class PinnedHostBuffer(Of T As Structure)
        Implements IDisposable

        Private ReadOnly _count As Integer
        Private ReadOnly _elementSize As Integer
        Private _pointer As IntPtr
        Private _disposed As Boolean

        Public Sub New(count As Integer)
            If count <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(count))

            _count = count
            _elementSize = Marshal.SizeOf(GetType(T))

            Dim bytes = CLng(count) * CLng(_elementSize)
            Dim handle As IntPtr = IntPtr.Zero

            CudaDriverApi.Check(
                CudaDriverApi.cuMemAllocHost_v2(handle, CULng(bytes)), "cuMemAllocHost_v2")

            _pointer = handle
        End Sub

        ''' <summary>元素个数</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _count
            End Get
        End Property

        ''' <summary>总字节数</summary>
        Public ReadOnly Property ByteSize As ULong
            Get
                Return CULng(_count) * CULng(_elementSize)
            End Get
        End Property

        ''' <summary>页锁定内存的主机地址（可直接交给异步拷贝）</summary>
        Public ReadOnly Property Pointer As IntPtr
            Get
                Return _pointer
            End Get
        End Property

        ''' <summary>把主机数组拷贝进页锁定区（H -> pinned）</summary>
        Public Sub Write(values As T())
            ThrowIfDisposed()
            CopyTo(_pointer, values)
        End Sub

        ''' <summary>从页锁定区拷回一个主机数组（pinned -> H）</summary>
        Public Function Read() As T()
            ThrowIfDisposed()

            Dim result(_count - 1) As T
            CopyFrom(_pointer, result)
            Return result
        End Function

        ''' <summary>把页锁定区的内容读进调用方提供的数组</summary>
        Public Sub ReadInto(values As T())
            ThrowIfDisposed()
            CopyFrom(_pointer, values)
        End Sub

        ''' <summary>按字节清零</summary>
        Public Sub Clear()
            ThrowIfDisposed()

            For i As Integer = 0 To CInt(ByteSize) - 1
                Marshal.WriteByte(_pointer, i, 0)
            Next
        End Sub

        ' ------------------------------------------------------------------
        ' 封送：常见基元类型走 Marshal.Copy（快路径），其余走通用结构封送
        ' ------------------------------------------------------------------

        Private Shared Sub CopyTo(target As IntPtr, values As T())
            If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))

            Dim elementType = GetType(T)

            If elementType Is GetType(Single) Then
                Marshal.Copy(DirectCast(CObj(values), Single()), 0, target, values.Length)
            ElseIf elementType Is GetType(Double) Then
                Marshal.Copy(DirectCast(CObj(values), Double()), 0, target, values.Length)
            ElseIf elementType Is GetType(Integer) Then
                Marshal.Copy(DirectCast(CObj(values), Integer()), 0, target, values.Length)
            ElseIf elementType Is GetType(Long) Then
                Marshal.Copy(DirectCast(CObj(values), Long()), 0, target, values.Length)
            ElseIf elementType Is GetType(Short) Then
                Marshal.Copy(DirectCast(CObj(values), Short()), 0, target, values.Length)
            ElseIf elementType Is GetType(Byte) Then
                Marshal.Copy(DirectCast(CObj(values), Byte()), 0, target, values.Length)
            Else
                Dim size = Marshal.SizeOf(elementType)

                For i As Integer = 0 To values.Length - 1
                    Marshal.StructureToPtr(values(i), IntPtr.Add(target, i * size), False)
                Next
            End If
        End Sub

        Private Shared Sub CopyFrom(source As IntPtr, values As T())
            If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))

            Dim elementType = GetType(T)

            If elementType Is GetType(Single) Then
                Marshal.Copy(source, DirectCast(CObj(values), Single()), 0, values.Length)
            ElseIf elementType Is GetType(Double) Then
                Marshal.Copy(source, DirectCast(CObj(values), Double()), 0, values.Length)
            ElseIf elementType Is GetType(Integer) Then
                Marshal.Copy(source, DirectCast(CObj(values), Integer()), 0, values.Length)
            ElseIf elementType Is GetType(Long) Then
                Marshal.Copy(source, DirectCast(CObj(values), Long()), 0, values.Length)
            ElseIf elementType Is GetType(Short) Then
                Marshal.Copy(source, DirectCast(CObj(values), Short()), 0, values.Length)
            ElseIf elementType Is GetType(Byte) Then
                Marshal.Copy(source, DirectCast(CObj(values), Byte()), 0, values.Length)
            Else
                Dim size = Marshal.SizeOf(elementType)

                For i As Integer = 0 To values.Length - 1
                    values(i) = DirectCast(Marshal.PtrToStructure(IntPtr.Add(source, i * size), elementType), T)
                Next
            End If
        End Sub

        Private Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(Me.GetType().Name)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            If _pointer <> IntPtr.Zero Then
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuMemFreeHost(_pointer), "cuMemFreeHost")
                Catch
                End Try
                _pointer = IntPtr.Zero
            End If

            GC.SuppressFinalize(Me)
        End Sub

        Public Overrides Function ToString() As String
            Return $"PinnedHost(Of {GetType(T).Name})[{_count}] @0x{_pointer:X} ({ByteSize} bytes)"
        End Function
    End Class
End Namespace
