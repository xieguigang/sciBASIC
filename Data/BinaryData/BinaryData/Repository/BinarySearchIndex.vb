''' <summary>
''' Using the binary tree as the search index.
''' (将二叉树序列化为二进制文件作为索引文件)
''' </summary>
Public Class BinarySearchIndex

End Class

''' <summary>
''' The node of the binary search tree
''' </summary>
Public Class Index

    ''' <summary>
    ''' 索引关键词
    ''' </summary>
    ''' <returns></returns>
    Public Property Key As String
    ''' <summary>
    ''' 在数据文件之中的偏移量
    ''' </summary>
    ''' <returns></returns>
    Public Property Offset As Long
End Class
