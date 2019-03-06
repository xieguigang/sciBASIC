
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO
Imports BinaryReader = Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    Public Class DataspaceMessage
        Private m_address As Long
        Private m_version As Integer
        Private m_numberOfDimensions As Integer
        Private m_flags As SByte
        Private m_type As Integer
        Private m_dimensionLength As Integer()
        Private m_maxDimensionLength As Integer()

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Me.m_version = [in].readByte()

            If Me.m_version = 1 Then
                Me.m_numberOfDimensions = [in].readByte()
                Me.m_flags = [in].readByte()
                Me.m_type = If(Me.m_numberOfDimensions = 0, 0, 1)
                [in].skipBytes(5)
            ElseIf Me.m_version = 2 Then
                Me.m_numberOfDimensions = [in].readByte()
                Me.m_flags = [in].readByte()
                Me.m_type = [in].readByte()
            Else
                Throw New IOException("unknown version")
            End If

            Me.m_dimensionLength = New Integer(Me.m_numberOfDimensions - 1) {}
            For i As Integer = 0 To Me.m_numberOfDimensions - 1
                Me.m_dimensionLength(i) = CInt(ReadHelper.readL([in], sb))
            Next

            Dim hasMax As Boolean = ((Me.m_flags And &H1) <> 0)
            Me.m_maxDimensionLength = New Integer(Me.m_numberOfDimensions - 1) {}
            If hasMax Then
                For i As Integer = 0 To Me.m_numberOfDimensions - 1
                    Me.m_maxDimensionLength(i) = CInt(ReadHelper.readL([in], sb))
                Next
            Else
                For i As Integer = 0 To Me.m_numberOfDimensions - 1
                    Me.m_maxDimensionLength(i) = Me.m_dimensionLength(i)
                Next
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

        Public Overridable ReadOnly Property flags() As SByte
            Get
                Return Me.m_flags
            End Get
        End Property

        Public Overridable ReadOnly Property type() As Integer
            Get
                Return Me.m_type
            End Get
        End Property

        Public Overridable ReadOnly Property dimensionLength() As Integer()
            Get
                Return Me.m_dimensionLength
            End Get
        End Property

        Public Overridable ReadOnly Property maxDimensionLength() As Integer()
            Get
                Return Me.m_maxDimensionLength
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("DataspaceMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("version : " & Me.m_version)
            Console.WriteLine("number of dimensions : " & Me.m_numberOfDimensions)
            Console.WriteLine("flags : " & Me.m_flags)
            Console.WriteLine("type : " & Me.m_type)

            Console.WriteLine("DataspaceMessage <<<")
        End Sub
    End Class

End Namespace
