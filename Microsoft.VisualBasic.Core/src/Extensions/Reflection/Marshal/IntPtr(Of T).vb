#Region "Microsoft.VisualBasic::de07bdc5ed2e4ee689eeb55581758e40, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Marshal\IntPtr(Of T).vb"

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

    '   Total Lines: 181
    '    Code Lines: 69 (38.12%)
    ' Comment Lines: 90 (49.72%)
    '    - Xml Docs: 82.22%
    ' 
    '   Blank Lines: 22 (12.15%)
    '     File Size: 7.31 KB


    '     Delegate Sub
    ' 
    ' 
    '     Delegate Sub
    ' 
    ' 
    '     Class IntPtr
    ' 
    '         Properties: Scan0
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Read, ToString
    ' 
    '         Sub: __unsafeWrite, (+2 Overloads) Dispose, (+3 Overloads) Write
    ' 
    '         Operators: -, +
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports PInvoke = System.Runtime.InteropServices.Marshal

Namespace Emit.Marshal

    ''' <summary>
    ''' 读取原始的内存数据的操作
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="destination"></param>
    ''' <param name="startIndex"></param>
    ''' <param name="length"></param>
    Public Delegate Sub UnsafeCopys(Of T)(source As System.IntPtr, destination As T(), startIndex As Integer, length As Integer)
    ''' <summary>
    ''' 向原始的内存数据执行写入操作的函数指针
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="destination"></param>
    ''' <param name="startIndex"></param>
    ''' <param name="source"></param>
    ''' <param name="length"></param>
    Public Delegate Sub UnsafeWrite(Of T)(destination As T(), startIndex As Integer, source As System.IntPtr, length As Integer)

    ''' <summary>
    ''' Unmanaged Memory pointer in VisualBasic language.(内存指针)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>只不过这个对象是封装了写内存操作的</remarks>
    Public Class IntPtr(Of T) : Inherits Pointer(Of T)
        Implements IDisposable

        ''' <summary>
        ''' The position in the memory region of the first byte for read.
        ''' </summary>
        ''' <returns>(第一个位置)</returns>
        Public ReadOnly Property Scan0 As System.IntPtr

        ''' <summary>
        ''' ```vbnet
        ''' Public Delegate Sub UnsafeWrite(Of T)(destination As T(), startIndex As Integer, source As System.IntPtr, length As Integer)
        ''' ```
        ''' </summary>
        ReadOnly __writeMemory As UnsafeWrite(Of T)
        ReadOnly __unsafeCopys As UnsafeCopys(Of T)

        ''' <summary>
        ''' Make data unsafe copy in this constructor function
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="chunkSize"></param>
        ''' <param name="unsafeCopys">
        ''' ```vbnet
        ''' Public Sub UnsafeCopys(Of <typeparamref name="T"/>)(source As <see cref="System.IntPtr"/>, destination As <typeparamref name="T"/>(), startIndex As <see cref="Integer"/>, length As <see cref="Integer"/>)
        ''' ```
        ''' </param>
        ''' <param name="unsafeWrite">
        ''' ```vbnet
        ''' Public Sub UnsafeWrite(Of <typeparamref name="T"/>)(destination As <typeparamref name="T"/>(), startIndex As <see cref="Integer"/>, source As <see cref="System.IntPtr"/>, length As <see cref="Integer"/>)
        ''' ```
        ''' </param>
        Sub New(p As System.IntPtr, chunkSize As Integer, unsafeCopys As UnsafeCopys(Of T), unsafeWrite As UnsafeWrite(Of T))
            __writeMemory = unsafeWrite
            __unsafeCopys = unsafeCopys

            Scan0 = p
            buffer = New T(chunkSize - 1) {}
            Call unsafeCopys(Scan0, buffer, 0, buffer.Length)
        End Sub

        ''' <summary>
        ''' 方便进行数组操作的
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="p"></param>
        Sub New(ByRef raw As T(), Optional p As System.IntPtr? = Nothing)
            Call MyBase.New(raw)

            ' 20241014
            ' avoid the error: nullable value must have a value
            If Not p Is Nothing Then
                Scan0 = p
            End If
        End Sub

        Public Sub Write(data As T())
            buffer = data
            Call __unsafeWrite(Scan0)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub __unsafeWrite(p As System.IntPtr)
            Call __writeMemory(buffer, 0, p, buffer.Length)
        End Sub

        ''' <summary>
        ''' Unsafe write memory
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Write()
            Call __unsafeWrite(Scan0)
        End Sub

        Public Function Read() As T()
            Call __unsafeCopys(Scan0, buffer, 0, buffer.Length)
            Return buffer
        End Function

        ''' <summary>
        ''' Please be carefull by using this method, if the memory region size of <see cref="Scan0"/> 
        ''' in this memory pointer is larger than <paramref name="des"/>, this method will caused 
        ''' exception.
        ''' </summary>
        ''' <param name="des"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Write(des As System.IntPtr)
            Call __unsafeWrite(des)
        End Sub

        Public Overrides Function ToString() As String
            Return $"* {GetType(T).Name} + {index} --> {Current}  // {Scan0.ToString}"
        End Function

        ''' <summary>
        ''' Move forward the current position of this memory pointer <paramref name="ptr"/> by a specific step <paramref name="d"/>
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="d"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(ptr As IntPtr(Of T), d As Integer) As IntPtr(Of T)
            ptr.index += d
            Return ptr
        End Operator

        ''' <summary>
        ''' Move backward the current position of this memory pointer <paramref name="ptr"/> by a specific step <paramref name="d"/>
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <param name="d"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator -(ptr As IntPtr(Of T), d As Integer) As IntPtr(Of T)
            ptr.index -= d
            Return ptr
        End Operator

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Write()
                    ' Free HGlobal memory
                    Call PInvoke.FreeHGlobal(Scan0)
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.

        ''' <summary>
        ''' write memory and release the memory pointer
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
