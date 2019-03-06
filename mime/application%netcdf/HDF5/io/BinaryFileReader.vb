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
                    Throw New System.ArgumentException("offset must be positive and bigger than 0")
                End If
                If Value > Me.m_filesize Then
                    Throw New System.ArgumentException("offset must be positive and smaller than filesize")
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
