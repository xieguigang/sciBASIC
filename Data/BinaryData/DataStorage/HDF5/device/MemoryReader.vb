Imports System.IO

Namespace HDF5.device

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
    End Class
End Namespace