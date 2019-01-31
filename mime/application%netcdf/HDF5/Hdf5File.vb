Imports Microsoft.VisualBasic.MIME.application.netCDF.org.renjin.hdf5.chunked
Imports Microsoft.VisualBasic.MIME.application.netCDF.org.renjin.hdf5.message

Namespace org.renjin.hdf5

    Public Class Hdf5File

        Public Const UNDEFINED_ADDRESS As Long = &HFFFFFFFFFFFFFFFFL

        Private ReadOnly file As Hdf5Data
        Private ReadOnly rootObject As DataObject

        Public Sub New(file As java.io.File)
            Me.file = New Hdf5Data(file)
            Me.rootObject = New DataObject(Me.file, Me.file.Superblock.RootGroupObjectHeaderAddress)
        End Sub

        Public Overridable Function getObject(ParamArray path() As String) As DataObject

            Dim node As DataObject = rootObject

            For i As Integer = 0 To path.Length - 1
                Dim groupIndex As org.renjin.hdf5.groups.GroupIndex = readGroupIndex(node)
                node = groupIndex.getObject(path(i))
            Next i
            Return node
        End Function

        Private Function readGroupIndex([object] As DataObject) As org.renjin.hdf5.groups.GroupIndex
            If [object].hasMessage(GetType(SymbolTableMessage)) Then
                Dim symbolTable As SymbolTableMessage = [object].getMessage(GetType(SymbolTableMessage))
                Return New org.renjin.hdf5.groups.GroupBTree(Me.file, symbolTable)

            ElseIf [object].hasMessage(GetType(LinkInfoMessage)) Then
                Dim linkInfo As LinkInfoMessage = [object].getMessage(GetType(LinkInfoMessage))
                If linkInfo.hasFractalHeap() Then
                    Return New org.renjin.hdf5.groups.FractalHeapGroupIndex(Me.file, linkInfo.FractalHeapAddress)
                Else
                    Return New org.renjin.hdf5.groups.SimpleGroupIndex(Me.file, [object].getMessages(GetType(LinkMessage)))
                End If
            Else
                Throw New System.NotSupportedException("TODO: cannot construct group index")
            End If
        End Function

        Public Overridable Function openChunkIndex([object] As DataObject) As ChunkIndex

            Dim dataspace As DataspaceMessage = [object].getMessage(GetType(DataspaceMessage))
            Dim layout As DataLayoutMessage = [object].getMessage(GetType(DataLayoutMessage))
            Dim datatype As DatatypeMessage = [object].getMessage(GetType(DatatypeMessage))
            Dim storage As org.renjin.repackaged.guava.base.Optional(Of DataStorageMessage) = [object].getMessageIfPresent(GetType(DataStorageMessage))

            Dim decoderFactory As New ChunkDecoderFactory(file)
            Dim chunkDecoder As ChunkDecoder = decoderFactory.create(datatype, layout, storage)

            Select Case layout.ChunkIndexingType
                Case ChunkIndexingType.BTREE
                    Return New BTreeChunkIndex(file, layout, chunkDecoder)
                Case ChunkIndexingType.FIXED_ARRAY
                    Return New FixedArrayChunkIndex(file, dataspace, layout, chunkDecoder)
                Case ChunkIndexingType.EXTENSIBLE_ARRAY
                    Return New ExtensibleArrayChunkIndex(file, dataspace, layout, decoderFactory.createFactory(datatype))
                Case Else
                    Throw New NotSupportedException("indexing type: " & layout.ChunkIndexingType)
            End Select
        End Function
    End Class

End Namespace