Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Styling

    Module SizeExpression

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expression$">
        ''' + propertyName
        ''' + 数字
        ''' + map表达式：
        '''    + ``map(propertyName, [min, max])`` 区间映射
        '''    + ``map(propertyName, val1=size1, val2=size2, val3=size3, ...)`` 离散映射
        ''' </param>
        ''' <returns></returns>
        Public Function Evaluate(expression As String) As GetSize
            If expression.MatchPattern(Casting.RegexpDouble) Then
                Return expression.unifySize
            ElseIf IsMapExpression(expression) Then
                Return expression.mappingSize
            Else
                Return expression.passthroughSize
            End If
        End Function

        ''' <summary>
        ''' 离散映射或者区间映射
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        <Extension>
        Private Function mappingSize(expression As String) As GetSize
            Dim t = expression.MapExpressionParser

            If t.type = MapperTypes.Continuous Then
                Dim range As DoubleRange = $"{t.values(0)},{t.values(1)}"
                Dim selector = t.propertyName.SelectNodeValue
                Dim getValue = Function(node As Node) Val(selector(node))

                Return Function(nodes)
                           Return nodes.ValDegreeAsSize(getValue, range)
                       End Function
            Else
                Dim sizeList#() = t.values _
                    .Select(AddressOf Val) _
                    .ToArray

                Return Function(nodes)
                           Dim maps = nodes.DiscreteMapping(t.propertyName)
                           Dim out = maps _
                               .Select(Function(map)
                                           Return New Map(Of Node, Double) With {
                                               .Key = map.Key,
                                               .Maps = sizeList(map.Maps)
                                           }
                                       End Function) _
                               .ToArray
                           Return out
                       End Function
            End If
        End Function

        ''' <summary>
        ''' 从节点的给定属性之中得到对应的节点大小值
        ''' </summary>
        ''' <param name="expression">这个表达式应该是属性名称</param>
        ''' <returns></returns>
        <Extension>
        Private Function passthroughSize(expression As String) As GetSize
            ' 单词
            Dim selector = expression.SelectNodeValue

            Return Iterator Function(nodes)
                       For Each n As Node In nodes
                           Yield New Map(Of Node, Single) With {
                               .Key = n,
                               .Maps = Val(selector(n))
                           }
                       Next
                   End Function
        End Function

        ''' <summary>
        ''' 所有的节点都统一大小
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        <Extension>
        Private Function unifySize(expression As String) As GetSize
            Dim r! = Val(expression)

            Return Iterator Function(nodes)
                       For Each n As Node In nodes
                           Yield New Map(Of Node, Single) With {
                               .Key = n,
                               .Maps = r
                           }
                       Next
                   End Function
        End Function
    End Module
End Namespace