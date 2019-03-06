
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 

Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO

Namespace HDF5.[Structure]

    Public Class LayoutMessage
        Private m_address As Long
        Private m_version As Integer
        Private m_numberOfDimensions As Integer
        Private m_type As Integer
        Private m_dataAddress As Long
        Private m_continuousSize As Long
        Private m_chunkSize As Integer()
        Private m_dataSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_version = [in].readByte()

            If Me.m_version < 3 Then
                Me.m_numberOfDimensions = [in].readByte()
                Me.m_type = [in].readByte()

                [in].skipBytes(5)

                Dim isCompact As Boolean = (Me.m_type = 0)
                If Not isCompact Then
                    Me.m_dataAddress = ReadHelper.readO([in], sb)
                End If

                Me.m_chunkSize = New Integer(Me.m_numberOfDimensions - 1) {}
                For i As Integer = 0 To Me.m_numberOfDimensions - 1
                    Me.m_chunkSize(i) = [in].readInt()
                Next

                If isCompact Then
                    Me.m_dataSize = [in].readInt()
                    Me.m_dataAddress = [in].offset
                End If
            Else
                Me.m_type = [in].readByte()

                If Me.m_type = 0 Then
                    Me.m_dataSize = [in].readShort()
                    Me.m_dataAddress = [in].offset
                ElseIf Me.m_type = 1 Then
                    Me.m_dataAddress = ReadHelper.readO([in], sb)
                    Me.m_continuousSize = ReadHelper.readL([in], sb)
                ElseIf Me.m_type = 2 Then
                    Me.m_numberOfDimensions = [in].readByte()
                    Me.m_dataAddress = ReadHelper.readO([in], sb)
                    Me.m_chunkSize = New Integer(Me.m_numberOfDimensions - 1) {}

                    For i As Integer = 0 To Me.m_numberOfDimensions - 1
                        Me.m_chunkSize(i) = [in].readInt()
                    Next
                End If
            End If
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property version() As Integer
            Get
                Return Me.m_version
            End Get
        End Property

        Public Overridable ReadOnly Property numberOfDimensions() As Integer
            Get
                Return Me.m_numberOfDimensions
            End Get
        End Property

        Public Overridable ReadOnly Property dataAddress() As Long
            Get
                Return Me.m_dataAddress
            End Get
        End Property

        Public Overridable ReadOnly Property continuousSize() As Long
            Get
                Return Me.m_continuousSize
            End Get
        End Property

        Public Overridable ReadOnly Property chunkSize() As Integer()
            Get
                Return Me.m_chunkSize
            End Get
        End Property

        Public Overridable ReadOnly Property dataSize() As Integer
            Get
                Return Me.m_dataSize
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("LayoutMessage >>>")

            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("number of dimensions : " & Me.m_numberOfDimensions)
            Console.WriteLine("type : " & Me.m_type)
            Console.WriteLine("data address : " & Me.m_dataAddress)
            Console.WriteLine("continuous size : " & Me.m_continuousSize)
            Console.WriteLine("data size : " & Me.m_dataSize)

            For i As Integer = 0 To Me.m_chunkSize.Length - 1
                Console.WriteLine("chunk size [" & i & "] : " & Me.m_chunkSize(i))
            Next

            Console.WriteLine("LayoutMessage <<<")
        End Sub
    End Class

End Namespace
