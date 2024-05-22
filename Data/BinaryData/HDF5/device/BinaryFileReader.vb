#Region "Microsoft.VisualBasic::a55a6f28838f80970a55a5a8e990529d, Data\BinaryData\HDF5\device\BinaryFileReader.vb"

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

    '   Total Lines: 122
    '    Code Lines: 93 (76.23%)
    ' Comment Lines: 3 (2.46%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 26 (21.31%)
    '     File Size: 3.95 KB


    '     Class BinaryFileReader
    ' 
    '         Properties: offset
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: getBuffer, readByte, StopIfMissingFile, ToString
    ' 
    '         Sub: close, setPosition
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace device

    Public Class BinaryFileReader : Inherits BinaryReader

        Dim randomaccessfile As Stream

        Public Overrides Property offset() As Long
            Get
                Return MyBase.offset
            End Get
            Set
                If Value < 0 Then
                    Throw New ArgumentException("offset must be positive and bigger than 0")
                End If
                If Value > Me.filesize Then
                    Throw New ArgumentException("offset must be positive and smaller than filesize")
                End If

                Call setPosition(Value)
            End Set
        End Property

        Public Sub New(filepath As String)
            Call Me.New(StopIfMissingFile(filepath))
        End Sub

        Private Shared Function StopIfMissingFile(filepath As String) As FileInfo
            If filepath.StringEmpty Then
                Throw New ArgumentException($"filepath({filepath}) must not be null or empty!")
            ElseIf filepath.FileLength <= 0 Then
                Throw New FileLoadException($"file({filepath}) missing or zero length!")
            End If

            Return New FileInfo(filepath)
        End Function

        Sub New(buffer As Stream)
            Me.offset = 0
            Me.m_littleEndian = True
            Me.m_maxOffset = 0
            Me.randomaccessfile = buffer
            Me.filesize = buffer.Length
        End Sub

        Public Sub New(file As FileInfo)
            If file Is Nothing Then
                Throw New ArgumentException("file must not be null")
            End If

            Me.offset = 0
            Me.m_littleEndian = True
            Me.m_maxOffset = 0
            Me.filesize = file.Length
            Me.randomaccessfile = New FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
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
            Me.randomaccessfile.Seek(value, SeekOrigin.Begin)
        End Sub

        Public Overrides Function readByte() As Byte
            If Me.offset >= Me.filesize Then
                ' Throw New IOException("file offset reached to end of file")
                Return Nothing
            End If

            Dim b As Byte = CByte(Me.randomaccessfile.ReadByte())

            MyBase.offset += 1

            If Me.m_maxOffset < Me.offset Then
                Me.m_maxOffset = Me.offset
            End If

            Return b
        End Function

        Public Overrides Function ToString() As String
            Dim debug As String
            Dim pos As Long = randomaccessfile.Position
            Dim width As Integer = 64

            ' randomaccessfile.Position = pos - width
            debug = Helpers.getDebugView(Me, width)
            randomaccessfile.Position = pos

            If TypeOf randomaccessfile Is FileStream Then
                Return $"{MyBase.ToString()}  #{DirectCast(randomaccessfile, FileStream).Name.FileName} '{debug}'"
            Else
                Return $"{MyBase.ToString()}  #{randomaccessfile.ToString} '{debug}'"
            End If
        End Function

        Public Overrides Sub close()
            Try
                Me.randomaccessfile.Close()
            Catch ex As IOException
                Call App.LogException(ex)
            Finally
                MyBase.offset = 0
            End Try
        End Sub

        Public Overrides Function getBuffer() As ByteBuffer
            Return New ByteBuffer(randomaccessfile)
        End Function
    End Class

End Namespace
