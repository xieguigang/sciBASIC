
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

    Public Class ContiguousDataset
        Inherits DatasetBase

        Public Sub New(hdfFc As HdfFileChannel, address As Long, name As String, parent As Group, oh As ObjectHeader)
            MyBase.New(hdfFc, address, name, parent, oh)
        End Sub

        Public Overrides ReadOnly Property dataBuffer() As ByteBuffer
            Get
                Dim contiguousDataLayoutMessage As ContiguousDataLayoutMessage = getHeaderMessage(GetType(ContiguousDataLayoutMessage))

                ' Check for empty dataset
                If contiguousDataLayoutMessage.address = UNDEFINED_ADDRESS Then
                    Return Nothing
                End If

                Try
                    Dim data As ByteBuffer = hdfFc.map(contiguousDataLayoutMessage.address, contiguousDataLayoutMessage.size)
                    convertToCorrectEndiness(data)
                    Return data
                Catch e As Exception
                    Throw New HdfException("Failed to map data buffer for dataset '" & path & "'", e)
                End Try
            End Get
        End Property

    End Class

End Namespace
