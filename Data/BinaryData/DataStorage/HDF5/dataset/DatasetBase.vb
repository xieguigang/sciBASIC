
'*****************************************************************************
' This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
' http://jhdf.io
' 
' Copyright 2019 James Mudd
' 
' MIT License see 'LICENSE' file
' *****************************************************************************

Namespace HDF5.dataset

    Public MustInherit Class DatasetBase
        Inherits AbstractNode
        Inherits dataset

        Protected Friend ReadOnly hdfFc As HdfFileChannel
        Protected Friend ReadOnly oh As ObjectHeader

        Private ReadOnly dataType_Renamed As DataType
        Private ReadOnly dataSpace As DataSpace

        Public Sub New(hdfFc As HdfFileChannel, address As Long, name As String, parent As Group, oh As ObjectHeader)
            MyBase.New(hdfFc, address, name, parent)
            Me.hdfFc = hdfFc
            Me.oh = oh

            dataType_Renamed = getHeaderMessage(GetType(DataTypeMessage)).dataType
            dataSpace = getHeaderMessage(GetType(DataSpaceMessage)).dataSpace
        End Sub

        Public Overrides ReadOnly Property type() As NodeType Implements io.jhdf.api.Node.type
            Get
                Return NodeType.DATASET
            End Get
        End Property

        Protected Friend Overridable Sub convertToCorrectEndiness(bb As ByteBuffer)
            If TypeOf dataType_Renamed Is OrderedDataType Then
                Dim order As ByteOrder = (DirectCast(dataType_Renamed, OrderedDataType).byteOrder)
                bb.order(order)
                logger.debug("Set buffer oder of '{}' to {}", path, order)
            Else
                bb.order(LITTLE_ENDIAN)
            End If
        End Sub

        Public Overridable ReadOnly Property size() As Long Implements dataset.size
            Get
                Return dataSpace.totalLength
            End Get
        End Property

        Public Overridable ReadOnly Property diskSize() As Long Implements dataset.diskSize
            Get
                Return size * dataType_Renamed.size
            End Get
        End Property

        Public Overridable ReadOnly Property dimensions() As Integer() Implements dataset.dimensions
            Get
                Return dataSpace.dimensions
            End Get
        End Property

        Public Overridable ReadOnly Property maxSize() As Integer() Implements dataset.maxSize
            Get
                If dataSpace.maxSizesPresent Then
                    Return dataSpace.maxSizes
                Else
                    Return dimensions
                End If
            End Get
        End Property

        Public Overridable ReadOnly Property dataLayout() As DataLayout Implements dataset.dataLayout
            Get
                Return getHeaderMessage(GetType(DataLayoutMessage)).dataLayout
            End Get
        End Property

        Public Overridable ReadOnly Property javaType() As type Implements dataset.javaType
            Get
                Dim type As type = dataType_Renamed.javaType
                ' For scalar datasets the returned type will be the wrapper class because
                ' getData returns Object
                If scalar AndAlso type.IsPrimitive Then
                    Return primitiveToWrapper(type)
                End If
                Return type
            End Get
        End Property

        Protected Friend Overridable ReadOnly Property dataType() As DataType
            Get
                Return dataType_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property data() As Object Implements dataset.data
            Get
                logger.debug("Getting data for '{}'...", path)
                If empty Then
                    Return Nothing
                End If
                Dim type As DataType = dataType
                Dim bb As ByteBuffer = dataBuffer
                If TypeOf type Is VariableLength Then
                    Return VariableLengthDatasetReader.readDataset(DirectCast(type, VariableLength), bb, dimensions, hdfFc)
                Else
                    Return DatasetReader.readDataset(type, bb, dimensions)
                End If
            End Get
        End Property

        Public Overridable ReadOnly Property scalar() As Boolean Implements dataset.scalar
            Get
                Return dimensions.Length = 0
            End Get
        End Property

        Public Overridable ReadOnly Property empty() As Boolean Implements dataset.empty
            Get
                Return dataBuffer Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets the buffer that holds this datasets data. The returned buffer will be of
        ''' the correct order (endiness).
        ''' </summary>
        ''' <returns> the data buffer that holds this dataset </returns>
        Public MustOverride ReadOnly Property dataBuffer() As ByteBuffer

        Public Overridable ReadOnly Property fillValue() As Object Implements dataset.fillValue
            Get
                Dim fillValueMessage As FillValueMessage = getHeaderMessage(GetType(FillValueMessage))
                If fillValueMessage.fillValueDefined Then
                    Dim bb As ByteBuffer = fillValueMessage.fillValue
                    ' Convert to data pass zero length dims for scalar
                    Return DatasetReader.readDataset(dataType, bb, New Integer(-1) {})
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return "DatasetBase [path=" & path & "]"
        End Function

    End Class

End Namespace
