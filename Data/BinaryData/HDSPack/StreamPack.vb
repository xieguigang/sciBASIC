
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.FileIO.Path

''' <summary>
''' Hierarchical Data Stream Pack, A hdf5 liked file format
''' </summary>
Public Class StreamPack

    ReadOnly superBlock As StreamGroup
    ReadOnly buffer As Stream
    ReadOnly init_size As Integer

    Sub New()

    End Sub

    ''' <summary>
    ''' open a data block for read and write
    ''' 
    ''' if the target file block is missing from the tree, then this function will append a new file block
    ''' otherwise a substream object will be returns for read data
    ''' </summary>
    ''' <param name="fileName"></param>
    ''' <returns></returns>
    Public Function OpenBlock(fileName As String) As Stream
        Dim path As New FilePath("/" & fileName)
        Dim block As StreamBlock

        If path.IsDirectory Then
            Throw New Exception($"can not open a directry({fileName}) as a data block!")
        End If

        If superBlock.BlockExists(path) Then
            ' get current object data
            block = superBlock.GetDataBlock(path)
        Else
            ' create a new data object
            block = superBlock.AddDataBlock(path)
            block.size = init_size
            block.offset = buffer.Length
        End If

        Return New SubStream(buffer, block.offset, block.size)
    End Function

End Class
