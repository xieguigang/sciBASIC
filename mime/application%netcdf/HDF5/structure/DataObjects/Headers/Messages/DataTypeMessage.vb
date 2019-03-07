
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 

Imports Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO
Imports BinaryReader = Microsoft.VisualBasic.MIME.application.netCDF.HDF5.IO.BinaryReader

Namespace HDF5.[Structure]

    ''' <summary>
    ''' The datatype message defines the datatype for each element of a dataset or 
    ''' a common datatype for sharing between multiple datasets. A datatype can 
    ''' describe an atomic type like a fixed- or floating-point type or more complex 
    ''' types like a C struct (compound datatype), array (array datatype), or C++ 
    ''' vector (variable-length datatype).
    '''
    ''' Datatype messages that are part Of a dataset Object Do Not describe how 
    ''' elements are related To one another; the dataspace message Is used For that 
    ''' purpose. Datatype messages that are part Of a committed datatype (formerly 
    ''' named datatype) message describe a common datatype that can be Shared by 
    ''' multiple datasets In the file.
    ''' </summary>
    Public Class DataTypeMessage

        ''' <summary>
        ''' Fixed-Point
        ''' </summary>
        Public Const DATATYPE_FIXED_POINT As Integer = 0
        ''' <summary>
        ''' Floating-point
        ''' </summary>
        Public Const DATATYPE_FLOATING_POINT As Integer = 1
        ''' <summary>
        ''' Time
        ''' </summary>
        Public Const DATATYPE_TIME As Integer = 2
        ''' <summary>
        ''' String
        ''' </summary>
        Public Const DATATYPE_STRING As Integer = 3
        ''' <summary>
        ''' Bit field
        ''' </summary>
        Public Const DATATYPE_BIT_FIELD As Integer = 4
        ''' <summary>
        ''' Opaque
        ''' </summary>
        Public Const DATATYPE_OPAQUE As Integer = 5
        ''' <summary>
        ''' Compound
        ''' </summary>
        Public Const DATATYPE_COMPOUND As Integer = 6
        ''' <summary>
        ''' Reference
        ''' </summary>
        Public Const DATATYPE_REFERENCE As Integer = 7
        ''' <summary>
        ''' Enumerated
        ''' </summary>
        Public Const DATATYPE_ENUMS As Integer = 8
        ''' <summary>
        ''' Variable-Length
        ''' </summary>
        Public Const DATATYPE_VARIABLE_LENGTH As Integer = 9
        ''' <summary>
        ''' Array
        ''' </summary>
        Public Const DATATYPE_ARRAY As Integer = 10

        ''' <summary>
        ''' 当前的这个对象在文件之中的起始位置
        ''' </summary>
        Private m_address As Long
        Private m_type As Integer
        Private m_version As Integer
        Private m_flags As Byte()
        Private m_byteSize As Integer
        Private m_littleEndian As Boolean

        Private m_unsigned As Boolean
        Private m_timeTypeByteSize As Integer
        Private m_opaqueDesc As String
        Private m_referenceType As Integer
        Private m_members As List(Of StructureMember)
        Private m_isOK As Boolean

        Private m_base As DataTypeMessage

        Public Sub New([in] As BinaryReader, sb As Superblock, address As Long)
            [in].offset = address

            Me.m_address = address

            Dim tandv As Byte = [in].readByte()
            Me.m_type = (tandv And &HF)
            Me.m_version = ((tandv And &HF0) >> 4)

            Me.m_flags = [in].readBytes(3)
            Me.m_byteSize = [in].readInt()
            Me.m_littleEndian = ((Me.m_flags(0) And &H1) = 0)
            Me.m_timeTypeByteSize = 4

            Me.m_isOK = True

            If Me.m_type = DATATYPE_FIXED_POINT Then
                Me.m_unsigned = ((Me.m_flags(0) And &H8) = 0)

                Dim bitOffset As Short = [in].readShort()
                Dim bitPrecision As Short = [in].readShort()

                Me.m_isOK = (bitOffset = 0) AndAlso (bitPrecision Mod 8 = 0)
            ElseIf Me.m_type = DATATYPE_FLOATING_POINT Then
                Dim bitOffset As Short = [in].readShort()
                Dim bitPrecision As Short = [in].readShort()
                Dim expLocation As Byte = [in].readByte()
                Dim expSize As Byte = [in].readByte()
                Dim manLocation As Byte = [in].readByte()
                Dim manSize As Byte = [in].readByte()
                Dim expBias As Integer = [in].readInt()
            ElseIf Me.m_type = DATATYPE_TIME Then
                Dim bitPrecision As Short = [in].readShort()
                Me.m_timeTypeByteSize = bitPrecision \ 8
            ElseIf Me.m_type = DATATYPE_STRING Then
                Dim ptype As Integer = Me.m_flags(0) And &HF
            ElseIf Me.m_type = DATATYPE_BIT_FIELD Then
                Dim bitOffset As Short = [in].readShort()
                Dim bitPrecision As Short = [in].readShort()
            ElseIf Me.m_type = DATATYPE_OPAQUE Then
                Dim len As Byte = Me.m_flags(0)
                Me.m_opaqueDesc = If((len > 0), [in].readASCIIString(len).Trim(), Nothing)
            ElseIf Me.m_type = DATATYPE_COMPOUND Then
                Dim nmembers As Integer = (Me.m_flags(1) * 256) + Me.m_flags(0)
                Me.m_members = New List(Of StructureMember)()

                For i As Integer = 0 To nmembers - 1
                    Me.m_members.Add(New StructureMember([in], sb, [in].offset, Me.m_version, Me.m_byteSize))
                Next
            ElseIf Me.m_type = DATATYPE_REFERENCE Then
                Me.m_referenceType = Me.m_flags(0) And &HF
            ElseIf Me.m_type = DATATYPE_ENUMS Then
                ' throw new IOException( "data type enums is not implemented" );

                Dim nmembers As Integer = ReadHelper.bytesToUnsignedInt(Me.m_flags(1), Me.m_flags(0))
                Me.m_base = New DataTypeMessage([in], sb, [in].offset)
                ' base type
                ' read the enums
                Dim enumName As [String]() = New [String](nmembers - 1) {}
                For i As Integer = 0 To nmembers - 1
                    If Me.m_version < 3 Then
                        enumName(i) = ReadHelper.readString8([in])
                    Else
                        ' padding
                        enumName(i) = [in].readASCIIString()
                        ' no padding
                    End If
                Next

                ' read the values; must switch to base byte order (!)
                If Not Me.m_base.m_littleEndian Then
                    [in].setBigEndian()
                End If

                Dim enumValue As Integer() = New Integer(nmembers - 1) {}
                For i As Integer = 0 To nmembers - 1
                    enumValue(i) = CInt(ReadHelper.readVariableSizeUnsigned([in], Me.m_base.m_byteSize))
                Next
                ' assume size is 1, 2, or 4

                'enumTypeName = objectName;
                'map = new TreeMap<Integer, String>();
                'for (int i = 0; i < nmembers; i++)
                '    map.put(enumValue[i], enumName[i]);

                [in].setLittleEndian()
            ElseIf Me.m_type = DATATYPE_VARIABLE_LENGTH Then
                Throw New Exception("data type variable length is not implemented")
            ElseIf Me.m_type = DATATYPE_ARRAY Then
                Throw New Exception("data type array is not implemented")
            End If
        End Sub

        Public Overridable ReadOnly Property address() As Long
            Get
                Return Me.m_address
            End Get
        End Property

        Public Overridable ReadOnly Property isLittleEndian() As Boolean
            Get
                Return Me.m_littleEndian
            End Get
        End Property

        Public Overridable ReadOnly Property isBigEndian() As Boolean
            Get
                Return Not Me.m_littleEndian
            End Get
        End Property

        Public Overridable ReadOnly Property type() As Integer
            Get
                Return Me.m_type
            End Get
        End Property

        Public Overridable ReadOnly Property byteSize() As Integer
            Get
                Return Me.m_byteSize
            End Get
        End Property

        Public Overridable ReadOnly Property structureMembers() As List(Of StructureMember)
            Get
                Return Me.m_members
            End Get
        End Property

        Public Overridable Sub printValues()
            Console.WriteLine("DataTypeMessage >>>")
            Console.WriteLine("address : " & Me.m_address)
            Console.WriteLine("data type : " & Me.m_type)
            Console.WriteLine("byteSize : " & Me.m_byteSize)

            If Me.m_members IsNot Nothing Then
                For Each mem As StructureMember In Me.m_members
                    mem.printValues()
                Next
            End If
            Console.WriteLine("DataTypeMessage <<<")
        End Sub

    End Class

End Namespace
