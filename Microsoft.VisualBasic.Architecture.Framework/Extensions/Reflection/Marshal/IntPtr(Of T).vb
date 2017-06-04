#Region "Microsoft.VisualBasic::c56fe3bf739669906bd75068dbe03c80, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Reflection\Marshal\IntPtr(Of T).vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.InteropServices.Marshal

Namespace Emit.Marshal

    Public Delegate Sub UnsafeCopys(Of T)(source As System.IntPtr, destination As T(), startIndex As Integer, length As Integer)
    Public Delegate Sub UnsafeWrite(Of T)(destination As T(), startIndex As Integer, source As System.IntPtr, length As Integer)

    ''' <summary>
    ''' 内存指针
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>只不过这个对象是封装了写内存操作的</remarks>
    Public Class IntPtr(Of T) : Inherits Pointer(Of T)
        Implements IDisposable

        ''' <summary>
        ''' 第一个位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Scan0 As System.IntPtr

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="chunkSize"></param>
        ''' <param name="unsafeCopys">
        ''' Public Sub UnsafeCopys(Of <typeparamref name="T"/>)(source As <see cref="System.IntPtr"/>, destination As <typeparamref name="T"/>(), startIndex As <see cref="Integer"/>, length As <see cref="Integer"/>)
        ''' </param>
        ''' <param name="unsafeWrite">
        ''' Public Sub UnsafeWrite(Of <typeparamref name="T"/>)(destination As <typeparamref name="T"/>(), startIndex As <see cref="Integer"/>, source As <see cref="System.IntPtr"/>, length As <see cref="Integer"/>)
        ''' </param>
        Sub New(p As System.IntPtr, chunkSize As Integer, unsafeCopys As UnsafeCopys(Of T), unsafeWrite As UnsafeWrite(Of T))
            __writeMemory = unsafeWrite
            Scan0 = p
            buffer = New T(chunkSize - 1) {}
            Call unsafeCopys(Scan0, buffer, 0, buffer.Length)
        End Sub

        ''' <summary>
        ''' 方便进行数组操作的
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="p"></param>
        Sub New(ByRef raw As T(), Optional p As System.IntPtr = Nothing)
            Call MyBase.New(raw)
            Scan0 = p
        End Sub

        ''' <summary>
        ''' Public Delegate Sub UnsafeWrite(Of T)(destination As T(), startIndex As Integer, source As System.IntPtr, length As Integer)
        ''' </summary>
        Dim __writeMemory As UnsafeWrite(Of T)

        Private Sub __unsafeWrite(p As System.IntPtr)
            Call __writeMemory(buffer, 0, p, buffer.Length)
        End Sub

        ''' <summary>
        ''' Unsafe write memory
        ''' </summary>
        Public Sub Write()
            Call __unsafeWrite(Scan0)
        End Sub

        Public Sub Write(des As System.IntPtr)
            Call __unsafeWrite(des)
        End Sub

        Public Overrides Function ToString() As String
            Return $"* {GetType(T).Name} + {__index} --> {Current}  // {Scan0.ToString}"
        End Function

        Public Overloads Shared Operator +(ptr As IntPtr(Of T), d As Integer) As IntPtr(Of T)
            ptr.__index += d
            Return ptr
        End Operator

        Public Overloads Shared Operator -(ptr As IntPtr(Of T), d As Integer) As IntPtr(Of T)
            ptr.__index -= d
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
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
