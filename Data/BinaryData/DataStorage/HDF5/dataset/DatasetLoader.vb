
''' <summary>
'''*****************************************************************************
''' This file is part of jHDF. A pure Java library for accessing HDF5 files.
''' 
''' http://jhdf.io
''' 
''' Copyright 2019 James Mudd
''' 
''' MIT License see 'LICENSE' file
''' *****************************************************************************
''' </summary>
Namespace HDF5.dataset

    Public NotInheritable Class DatasetLoader

        ' No instances
        Private Sub New()
        End Sub

        Public Shared Function createDataset(hdfFc As HdfFileChannel, oh As ObjectHeader, name As String, parent As Group) As dataset

            'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: The original Java variable was marked 'final':
            'ORIGINAL LINE: final long address = oh.getAddress();
            Dim address As Long = oh.address
            Try
                ' Determine the type of dataset to make
                'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: The original Java variable was marked 'final':
                'ORIGINAL LINE: final io.jhdf.object.message.DataLayoutMessage dlm = oh.getMessageOfType(io.jhdf.object.message.DataLayoutMessage.class);
                Dim dlm As DataLayoutMessage = oh.getMessageOfType(GetType(DataLayoutMessage))

                If TypeOf dlm Is DataLayoutMessage.CompactDataLayoutMessage Then

                    Return New CompactDataset(hdfFc, address, name, parent, oh)
                ElseIf TypeOf dlm Is DataLayoutMessage.ContiguousDataLayoutMessage Then

                    Return New ContiguousDataset(hdfFc, address, name, parent, oh)
                ElseIf TypeOf dlm Is DataLayoutMessage.ChunkedDataLayoutMessageV3 Then

                    Return New ChunkedDatasetV3(hdfFc, address, name, parent, oh)
                ElseIf TypeOf dlm Is DataLayoutMessage.ChunkedDataLayoutMessageV4 Then

                    Throw New UnsupportedHdfException("Chunked V4 dataset not supported")
                Else
                    'JAVA TO C# CONVERTER CRACKED BY X-CRACKER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
                    Throw New HdfException("Unrecognized Dataset layout type: " & dlm.[GetType]().FullName)

                End If
            Catch e As Exception
                Throw New HdfException("Failed to read dataset '" & name & "' at address '" & address & "'", e)
            End Try
        End Function

    End Class

End Namespace
