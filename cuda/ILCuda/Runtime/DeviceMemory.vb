#Region "Microsoft.VisualBasic::11f113c37f0dbe336b2e8f993c10276f, cuda\ILCuda\Runtime\DeviceMemory.vb"

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

    '   Total Lines: 56
    '    Code Lines: 45 (80.36%)
    ' Comment Lines: 2 (3.57%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (16.07%)
    '     File Size: 1.82 KB


    '     Class DeviceMemory
    ' 
    '         Properties: ByteSize, Pointer
    ' 
    '         Sub: Allocate, Clear, Dispose, Release, ThrowIfDisposed
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    Public MustInherit Class DeviceMemory
        Implements IDisposable

        Protected _pointer As ULong
        Protected _byteSize As ULong
        Protected _disposed As Boolean

        ''' <summary>设备端指针（CUdeviceptr，64 位）</summary>
        Public ReadOnly Property Pointer As ULong
            Get
                Return _pointer
            End Get
        End Property

        ''' <summary>已分配的字节数</summary>
        Public ReadOnly Property ByteSize As ULong
            Get
                Return _byteSize
            End Get
        End Property

        Protected Sub Allocate(bytes As ULong)
            If bytes = 0 Then Throw New ArgumentOutOfRangeException(NameOf(bytes), "分配的字节数必须大于 0")
            CudaDriverApi.Check(CudaDriverApi.cuMemAlloc_v2(_pointer, bytes), "cuMemAlloc_v2")
            _byteSize = bytes
        End Sub

        Public Sub Clear()
            ThrowIfDisposed()
            CudaDriverApi.Check(CudaDriverApi.cuMemsetD8_v2(_pointer, 0, _byteSize), "cuMemsetD8_v2")
        End Sub

        Protected Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(Me.GetType().Name)
        End Sub

        Protected Overridable Sub Release()
            If _pointer <> 0 Then
                Try
                    CudaDriverApi.Check(CudaDriverApi.cuMemFree_v2(_pointer), "cuMemFree_v2")
                Catch
                End Try
                _pointer = 0
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            Release()
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
