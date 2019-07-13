#Region "Microsoft.VisualBasic::b4c6cf1fdb754cc37475717210cfd02f, gr\avi\UInt8Array.vb"

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

    ' Class UInt8Array
    ' 
    '     Properties: length
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: subarray, ToString
    ' 
    '     Sub: (+2 Overloads) Dispose, Flush, writeBytes, writeInt, writeLong
    '          writeShort, writeString
    ' 
    ' /********************************************************************************/

#End Region

Public Class UInt8Array : Implements IDisposable

    Dim buf As Byte()
    Dim path As String
    Dim begin As Integer

    Public ReadOnly Property length As Integer
        Get
            Return buf.Length
        End Get
    End Property

    Sub New(path As String, size As Integer)
        Me.path = path
        Me.buf = New Byte(size - 1) {}
    End Sub

    Private Sub New()
    End Sub

    Public Function subarray(begin As Integer) As UInt8Array
        Dim subChunk As Byte() = New Byte(buf.Length - begin - 1) {}
        Call Array.ConstrainedCopy(buf, begin, subChunk, Scan0, subChunk.Length)
        Return New UInt8Array With {
            .buf = subChunk,
            .begin = begin
        }
    End Function

    Public Sub Flush(subchunk As UInt8Array)
        Call Array.ConstrainedCopy(subchunk.buf, Scan0, buf, subchunk.begin, subchunk.length)
    End Sub

    Public Sub writeBytes(idx As Integer, bytes As Byte())
        For i As Integer = 0 To bytes.Length - 1
            buf(idx + i) = bytes(i)
        Next
    End Sub

    Public Sub writeShort(idx As Integer, num As Short)
        buf(idx) = num And 255
        buf(idx + 1) = (num >> 8) And 255
    End Sub

    Public Sub writeInt(idx As Integer, num As Integer)
        buf(idx) = num And 255
        buf(idx + 1) = (num >> 8) And 255
        buf(idx + 2) = (num >> 16) And 255
        buf(idx + 3) = (num >> 24) And 255
    End Sub

    Public Sub writeLong(idx As Integer, num As Long)
        buf(idx) = num And 255
        buf(idx + 1) = (num >> 8) And 255
        buf(idx + 2) = (num >> 16) And 255
        buf(idx + 3) = (num >> 24) And 255
        buf(idx + 4) = 0
        buf(idx + 5) = 0
        buf(idx + 6) = 0
        buf(idx + 7) = 0
    End Sub

    Public Sub writeString(idx As Integer, str As String)
        For i As Integer = 0 To str.Length - 1
            buf(idx + i) = Asc(str.Chars(i)) And 255
        Next
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
                Call buf.FlushStream(path)
                Call buf.Free
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
