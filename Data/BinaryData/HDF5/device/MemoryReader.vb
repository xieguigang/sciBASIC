#Region "Microsoft.VisualBasic::4d2424e570ebc63a35013f6c9b404ce5, Data\BinaryData\HDF5\device\MemoryReader.vb"

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

    '   Total Lines: 73
    '    Code Lines: 53
    ' Comment Lines: 2
    '   Blank Lines: 18
    '     File Size: 2.02 KB


    '     Class MemoryReader
    ' 
    '         Properties: offset
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getBuffer, readByte
    ' 
    '         Sub: close, setPosition
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace device

    Public Class MemoryReader : Inherits BinaryReader

        Dim memory As MemoryStream

        Public Overrides Property offset() As Long
            Get
                Return MyBase.offset
            End Get
            Set(value As Long)
                If value < 0 Then
                    Throw New ArgumentException("offset must be positive and bigger than 0")
                End If
                If value > Me.filesize Then
                    Throw New ArgumentException("offset must be positive and smaller than filesize")
                End If

                Call setPosition(value)
            End Set
        End Property

        Sub New(memory As MemoryStream)
            Me.memory = memory

            Me.offset = 0
            Me.filesize = memory.Length
            Me.m_littleEndian = True
            Me.m_maxOffset = 0
        End Sub

        Private Sub setPosition(value As Long)
            If MyBase.offset = value Then
                Return
            End If

            MyBase.offset = value

            If Me.m_maxOffset < value Then
                Me.m_maxOffset = value
            End If

            ' change underlying file value
            Call memory.Seek(value, SeekOrigin.Begin)
        End Sub

        Public Overrides Sub close()
            ' do nothing
        End Sub

        Public Overrides Function readByte() As Byte
            If Me.offset >= Me.filesize Then
                Throw New IOException("file offset reached to end of file")
            End If

            Dim b As Byte = CByte(memory.ReadByte())

            MyBase.offset += 1

            If Me.m_maxOffset < Me.offset Then
                Me.m_maxOffset = Me.offset
            End If

            Return b
        End Function

        Public Overrides Function getBuffer() As ByteBuffer
            Return New ByteBuffer(memory)
        End Function
    End Class
End Namespace
