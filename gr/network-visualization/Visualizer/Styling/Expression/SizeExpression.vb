#Region "Microsoft.VisualBasic::60ddeaadfd4b9a545c087a3af0200aeb, gr\network-visualization\Visualizer\Styling\Expression\SizeExpression.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

'     Module SizeExpression
' 
'         Function: Evaluate, mappingSize, passthroughSize, unifySize
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling.CSS
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
                Dim range As DoubleRange = {t.values(0), t.values(1)}
                Dim selector = t.propertyName.SelectNodeValue
                Dim getValue = Function(node As Node) Val(selector(node))

                Return Function(nodes)
                           Return nodes.RangeTransform(getValue, range)
                       End Function
            Else
                Dim sizeList As Dictionary(Of String, Single) = t _
                    .values _
                    .Select(Function(s) s.GetTagValue("=", trim:=True)) _
                    .ToDictionary(Function(p) p.Name,
                                  Function(p) CSng(Val(p.Value)))
                Dim selector = t.propertyName.SelectNodeValue
                Dim key$
                Dim value#

                Return Iterator Function(nodes)
                           For Each n As Node In nodes
                               key = selector(n)
                               value = sizeList.TryGetValue(key)

                               Yield New Map(Of Node, Single) With {
                                   .Key = n,
                                   .Maps = value
                               }
                           Next
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
