#Region "Microsoft.VisualBasic::2618d1233ef96d3e933da3df9f1c8696, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\MemoryStreamPool.vb"

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

    '   Total Lines: 65
    '    Code Lines: 52
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 1.91 KB


    '     Class MemoryStreamPool
    ' 
    '         Properties: CanRead, CanSeek, CanWrite, Length, Position
    ' 
    '         Function: Read, Seek
    ' 
    '         Sub: Flush, SetLength, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace ApplicationServices

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' just recommended apply this object for fast binary data read
    ''' </remarks>
    Public Class MemoryStreamPool : Inherits Stream

        ReadOnly pool As MemoryStream()

        ''' <summary>
        ''' size of each stream object in pool
        ''' </summary>
        ReadOnly buffer_size As Integer

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' the total size in bytes of the stream pool
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property Length As Long
            Get
                Return Aggregate ms As MemoryStream
                       In pool
                       Into Sum(ms.Length)
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As Long)
                Throw New NotImplementedException()
            End Set
        End Property

        Private Sub New(pool As IEnumerable(Of MemoryStream), size As Integer)
            Me.pool = pool.ToArray
            Me.buffer_size = size
        End Sub

        Public Overrides Sub Flush()
            For Each ms As MemoryStream In pool
                Call ms.Flush()
            Next
        End Sub

        Public Overrides Sub SetLength(value As Long)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Seek(offset As Long, origin As SeekOrigin) As Long
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="buffer_size">
        ''' default buffer size is 1GB
        ''' </param>
        ''' <returns></returns>
        Public Shared Function FromFile(path As String, Optional buffer_size As Integer = 1024 * 1024 * 1024 * 1) As MemoryStreamPool
            Dim pool As New List(Of MemoryStream)
            Dim buffer As Byte() = New Byte(buffer_size - 1) {}
            Dim file As Stream = New FileStream(path:=path, mode:=FileMode.Open, access:=FileAccess.Read)

            If file.Length < buffer_size Then
                buffer = New Byte(file.Length - 1) {}
                file.Read(buffer, Scan0, file.Length)
                pool.Add(New MemoryStream(buffer))
            Else
                Dim size As Integer = buffer_size

                Do While file.Position < file.Length - 1
                    Call file.Read(buffer, Scan0, count:=size)
                    Call pool.Add(New MemoryStream(buffer))

                    If file.Length - file.Position < buffer_size Then
                        size = file.Length - file.Position
                    End If
                Loop
            End If

            Call file.Dispose()

            Return New MemoryStreamPool(pool, buffer_size)
        End Function
    End Class
End Namespace
