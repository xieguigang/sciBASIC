#Region "Microsoft.VisualBasic::d4057e7254eae74f90294a3a97e0f8ee, gr\avi\UInt8Array.vb"

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

    '   Total Lines: 110
    '    Code Lines: 75 (68.18%)
    ' Comment Lines: 14 (12.73%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 21 (19.09%)
    '     File Size: 3.32 KB


    ' Class UInt8Array
    ' 
    '     Properties: buf, length
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: subarray, ToString
    ' 
    '     Sub: (+2 Overloads) Dispose, writeBytes, writeInt, writeLong, writeShort
    '          writeString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Public Class UInt8Array : Implements IDisposable

    Dim buffer As FileStream
    Dim path As String
    Dim begin As Long = Scan0

    Public ReadOnly Property length As Long

    Private WriteOnly Property buf(location As Long) As Byte()
        Set(value As Byte())
            Call buffer.Seek(begin + location, SeekOrigin.Begin)
            Call buffer.Write(value, Scan0, value.Length)
        End Set
    End Property

    Sub New(path As String, size As Long)
        Me.path = path
        Me.buffer = path.Open(FileMode.OpenOrCreate, doClear:=True)
        Me.length = size
    End Sub

    Private Sub New()
    End Sub

    Public Function subarray(begin As Long) As UInt8Array
        Return New UInt8Array With {
            .buffer = buffer,
            .begin = begin,
            .path = path
        }
    End Function

    Public Sub writeBytes(idx As Long, bytes As Byte())
        buf(idx) = bytes
    End Sub

    Public Sub writeShort(idx As Long, num As Short)
        buf(idx) = {num And 255, (num >> 8) And 255}
    End Sub

    Public Sub writeInt(idx As Long, num As Integer)
        buf(idx) = {
            (num) And 255,
            (num >> 8) And 255,
            (num >> 16) And 255,
            (num >> 24) And 255
        }
    End Sub

    Public Sub writeLong(idx As Long, num As Long)
        buf(idx) = {
            (num) And 255,
            (num >> 8) And 255,
            (num >> 16) And 255,
            (num >> 24) And 255,
            0, 0, 0, 0
        }
    End Sub

    Public Sub writeString(idx As Long, str As String)
        Dim bytes As Byte() = New Byte(str.Length - 1) {}

        For i As Integer = 0 To str.Length - 1
            bytes(i) = Asc(str.Chars(i)) And 255
        Next

        buf(idx) = bytes
    End Sub

    Public Overrides Function ToString() As String
        Return $"memory://" & path
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call buffer.Flush()
                Call buffer.Close()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
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
