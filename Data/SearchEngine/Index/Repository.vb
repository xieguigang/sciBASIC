Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

''' <summary>
''' 
''' </summary>
''' <typeparam name="K"></typeparam>
''' <typeparam name="V"></typeparam>
''' <remarks>
''' 如果直接将二叉树序列化为XML或者json的话，会因为树枝过多而导致文档的格式比较难看
''' 所以需要在这里使用一个数组来优化输出的文档格式
''' 
''' 另外可能会因为CPU的平台的不同，导致在直接序列化二叉树的时候有些运行时环境无法进行过多的递归
''' 导致程序出错，所以在这里需要使用数组格式来避免这些问题
''' </remarks>
<XmlRoot("Repository", [Namespace]:="http://schema.sciBASIC.net/xml/Data/Index/Repository.xst")>
Public Class Repository(Of K, V)

    <XmlAttribute("root")>
    Public Property Root As Integer
    <XmlElement>
    Public Property Index As BinaryTreeIndex(Of K, V)()

    Sub New()
    End Sub

    Sub New(index As BinaryTreeIndex(Of K, V)())
        Me.Index = index
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetRootNode() As BinaryTreeIndex(Of K, V)
        Return Index(Root)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return Index(Root).ToString
    End Function
End Class
