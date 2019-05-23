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

    Public Class CompactDataset
        Inherits DatasetBase

        Public Sub New(hdfFc As HdfFileChannel, address As Long, name As String, parent As Group, oh As ObjectHeader)
            MyBase.New(hdfFc, address, name, parent, oh)
        End Sub

        Public Overrides ReadOnly Property dataBuffer() As ByteBuffer
            Get
                Dim data As ByteBuffer = getHeaderMessage(GetType(CompactDataLayoutMessage)).dataBuffer
                convertToCorrectEndiness(data)
                Return data
            End Get
        End Property

    End Class

End Namespace
