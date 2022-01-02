
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Styling.Numeric

    Public Interface IGetSize

        ''' <summary>
        ''' 这个函数描述了这样的一个过程：
        ''' 
        ''' 对一个节点集合进行成员的枚举，然后将每一个成员映射为一个大小数值，并返回这些映射集合
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <returns></returns>
        Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single))

    End Interface

    ''' <summary>
    ''' 从节点的给定属性之中得到对应的节点大小值
    ''' </summary>
    Public Class PassthroughNumber : Implements IGetSize

        ''' <summary>
        ''' 单词
        ''' </summary>
        ReadOnly expression As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expression">这个表达式应该是属性名称</param>
        Sub New(expression As String)
            Me.expression = expression
        End Sub

        Public Iterator Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single)) Implements IGetSize.GetSize
            ' 单词
            Dim selector = expression.SelectNodeValue

            For Each n As Node In nodes
                Yield New Map(Of Node, Single) With {
                    .Key = n,
                    .Maps = Val(selector(n))
                }
            Next
        End Function
    End Class
End Namespace