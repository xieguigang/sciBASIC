#Region "Microsoft.VisualBasic::fc4668a31e3861b3f35cb5cac7968356, mime\application%netcdf\HDF5\io\BinaryFileReader.vb"

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

    '     Class BinaryFileReader
    ' 
    '         Properties: offset
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: readByte
    ' 
    '         Sub: _BinaryFileReader, close
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace HDF5.IO


    Public Class BinaryFileReader
        Inherits BinaryReader

        Protected Friend m_randomaccessfile As FileStream

        Public Sub New(filepath As String)
            If String.ReferenceEquals(filepath, Nothing) Then
                Throw New System.ArgumentException("filepath must not be null")
            End If
            If filepath.Length = 0 Then
                Throw New System.ArgumentException("filepath must not be empty")
            End If

            _BinaryFileReader(New FileInfo(filepath))
        End Sub

        Public Sub New(file As FileInfo)
            _BinaryFileReader(file)
        End Sub

        Private Sub _BinaryFileReader(file As FileInfo)
            If file Is Nothing Then
                Throw New System.ArgumentException("file must not be null")
            End If

            Me.m_offset = 0
            Me.m_filesize = file.Length
            Me.m_littleEndian = True
            Me.m_maxOffset = 0

            Me.m_randomaccessfile = New FileStream(file.FullName, FileMode.Open)
        End Sub

        Public Overrides Property offset() As Long
            Get
                Return Me.m_offset
            End Get
            Set
                If Value < 0 Then
                    Throw New ArgumentException("offset must be positive and bigger than 0")
                End If
                If Value > Me.m_filesize Then
                    Throw New ArgumentException("offset must be positive and smaller than filesize")
                End If

                If Me.m_offset = Value Then
                    Return
                End If

                Me.m_offset = Value
                If Me.m_maxOffset < Value Then
                    Me.m_maxOffset = Value
                End If

                ' change underlying file value
                Me.m_randomaccessfile.Seek(Value, SeekOrigin.Begin)
            End Set
        End Property

        Public Overrides Function readByte() As Byte
            If Me.m_offset >= Me.m_filesize Then
                Throw New IOException("file offset reached to end of file")
            End If
            Dim b As Byte = CByte(Me.m_randomaccessfile.ReadByte())

            Me.m_offset += 1

            If Me.m_maxOffset < Me.m_offset Then
                Me.m_maxOffset = Me.m_offset
            End If

            Return b
        End Function

        Public Overrides Sub close()
            Try
                Me.m_randomaccessfile.Close()
            Catch generatedExceptionName As IOException
            End Try
            Me.m_offset = 0
        End Sub
    End Class

End Namespace

