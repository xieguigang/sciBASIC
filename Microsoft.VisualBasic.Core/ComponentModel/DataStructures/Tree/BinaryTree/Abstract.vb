Namespace ComponentModel.DataStructures.BinaryTree

    ''' <summary>
    ''' The generic object comparer model:
    ''' 
    ''' + ``> 0`` means ``a > b``
    ''' + ``= 0`` means ``a = 0``
    ''' + ``&lt;0`` means ``a &lt; b``
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    Public Delegate Function CompareOf(Of T)(a As T, b As T) As Integer

End Namespace