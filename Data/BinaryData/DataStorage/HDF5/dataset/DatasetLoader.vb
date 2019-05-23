
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

    <HideModuleName> Public Module DatasetLoader

        Sub New()
        End Sub

        Public Function createDataset(hdfFc As HdfFileChannel, oh As ObjectHeader, name As String, parent As Group) As dataset
            Dim address As Long = oh.address

            ' Determine the type of dataset to make
            Dim dlm As DataLayoutMessage = oh.getMessageOfType(GetType(DataLayoutMessage))

            If TypeOf dlm Is DataLayoutMessage.CompactDataLayoutMessage Then
                Return New CompactDataset(hdfFc, address, name, parent, oh)
            ElseIf TypeOf dlm Is DataLayoutMessage.ContiguousDataLayoutMessage Then
                Return New ContiguousDataset(hdfFc, address, name, parent, oh)
            ElseIf TypeOf dlm Is DataLayoutMessage.ChunkedDataLayoutMessageV3 Then
                Return New ChunkedDatasetV3(hdfFc, address, name, parent, oh)
            ElseIf TypeOf dlm Is DataLayoutMessage.ChunkedDataLayoutMessageV4 Then
                Throw New NotSupportedException("Chunked V4 dataset not supported")
            Else
                Throw New Exception("Unrecognized Dataset layout type: " & dlm.[GetType]().FullName)
            End If
        End Function
    End Module

End Namespace
