
Imports System.IO

''' <summary>
''' Hierarchical Data Stream Pack, A hdf5 liked file format
''' </summary>
Public Class StreamPack

    ReadOnly superBlock As StreamGroup

    ''' <summary>
    ''' open a data block for read and write
    ''' 
    ''' if the target file block is missing from the tree, then this function will append a new file block
    ''' otherwise a substream object will be returns for read data
    ''' </summary>
    ''' <param name="fileName"></param>
    ''' <returns></returns>
    Public Function OpenBlock(fileName As String) As Stream

    End Function

End Class
