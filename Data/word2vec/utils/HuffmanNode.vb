Imports System

Namespace utils
    ''' <summary>
    ''' Created by fangy on 13-12-17.
    ''' 哈夫曼树结点接口
    ''' </summary>
    Public Interface HuffmanNode
        Inherits IComparable(Of HuffmanNode)

        WriteOnly Property code As Integer
        Property frequency As Integer
        Property parent As HuffmanNode
        Function merge(sibling As HuffmanNode) As HuffmanNode
    End Interface
End Namespace
