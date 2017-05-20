#Region "Microsoft.VisualBasic::0865f3be7f9d5c216039d775f8658e40, ..\sciBASIC#\gr\Datavisualization.Network\NetworkCanvas\Styling\NodeStyles.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NameOf
Imports r = System.Text.RegularExpressions.Regex

Namespace Styling

    Public Module NodeStyles

        <Extension> Public Function DegreeAsSize(nodes As IEnumerable(Of Node),
                                                 getDegree As Func(Of Node, Double),
                                                 sizeRange As DoubleRange) As Map(Of Node, Double)()
            Return nodes.ValDegreeAsSize(getDegree, sizeRange)
        End Function

        <Extension> Public Function DegreeAsSize(nodes As IEnumerable(Of Node), sizeRange As DoubleRange, Optional degree$ = names.REFLECTION_ID_MAPPING_DEGREE) As Map(Of Node, Double)()
            Dim valDegree = Function(node As Node)
                                Return node.Data(degree).ParseDouble
                            End Function
            Return nodes.DegreeAsSize(
                getDegree:=valDegree,
                sizeRange:=sizeRange)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="nodes"></param>
        ''' <param name="getDegree"></param>
        ''' <param name="sizeRange">将节点的大小映射到这个半径大小的区间之内</param>
        ''' <returns></returns>
        <Extension> Public Function ValDegreeAsSize(Of T)(nodes As IEnumerable(Of T),
                                                          getDegree As Func(Of T, Double),
                                                          sizeRange As DoubleRange) As Map(Of T, Double)()
            Dim array As T() = nodes.ToArray
            Dim degrees#() = array.Select(getDegree).ToArray
            Dim size#() = degrees.RangeTransform([to]:=sizeRange)
            Dim out As Map(Of T, Double)() =
                array _
                .SeqIterator _
                .Select(Function(x)
                            Return New Map(Of T, Double) With {
                                .Key = x.value,
                                .Maps = size(x)
                            }
                        End Function) _
                .ToArray
            Return out
        End Function

        ''' <summary>
        ''' 根据节点类型来赋值颜色值
        ''' </summary>
        ''' <param name="nodes">
        ''' 要求节点对象模型之中必须要具备有<see cref="names.REFLECTION_ID_MAPPING_NODETYPE"/>这个动态属性值
        ''' </param>
        ''' <param name="schema$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorFromTypes(nodes As IEnumerable(Of Node), schema$) As Map(Of Node, Color)()
            Dim data As Node() = nodes.ToArray
            Dim nodeTypes$() = data _
                .Select(Function(o) o.Data(names.REFLECTION_ID_MAPPING_NODETYPE)) _
                .ToArray
            Dim types$() = nodeTypes _
                .Distinct _
                .ToArray
            Dim colors As Dictionary(Of String, Color) =
                Designer _
                .GetColors(term:=schema, n:=types.Length) _
                .SeqIterator _
                .ToDictionary(Function(i) types(i),
                              Function(color) color.value)
            Dim out As Map(Of Node, Color)() =
                nodeTypes _
                .SeqIterator _
                .Select(Function(type)
                            Return New Map(Of Node, Color) With {
                                .Key = data(type),
                                .Maps = colors(type.value)
                            }
                        End Function) _
                .ToArray

            Return out
        End Function

        Public Function ColorExpression(expression$) As Func(Of Node(), Map(Of Node, Color)())
            If expression.IsColorExpression Then
                Dim color As Color = expression.TranslateColor
                Return Function(nodes)
                           Return nodes _
                               .Select(Function(n)
                                           Return New Map(Of Node, Color) With {
                                               .Key = n,
                                               .Maps = color
                                           }
                                       End Function) _
                               .ToArray
                       End Function
            ElseIf expression.MatchPattern("map\(.+\)", RegexICSng) Then
                ' 先match rgb表达式，再执行替换之后，再正常的解析
                ' 网络之中的graph模型对象的颜色映射有三种类型：
                ' map(property, Continuous, schemaName, 250)，连续的数值型的映射
                ' map(property, Continuous, levels, startColor, endColor), 连续数值型的渐变映射
                ' map(property, Discrete, color1, color2, color3, color4, ...)，分类型的颜色离散映射
                '
                Dim rgbs = r.Matches(expression, rgbExpr, RegexICSng) _
                    .ToArray _
                    .Distinct _
                    .ToDictionary(Function(key) key.MD5)
                For Each hashValue In rgbs
                    With hashValue
                        expression = expression.Replace(.Value, .Key)
                    End With
                Next

                Dim t = expression.MapExpressionParser ' 解析映射表达式字符串

                If t.type.TextEquals("Continuous") Then
                    Dim colors As Color()

                    If (Not t.values(0).IsColorExpression) AndAlso t.values(1).MatchPattern(RegexpDouble) Then
                        ' map(property, Continuous, schemaName, 250)
                        ' 使用colorbrewer生成颜色谱
                        colors = Designer.GetColors(t.values(Scan0), Val(t.values(1)))
                    Else
                        Dim colorValues$() = t _
                            .values _
                            .Select(Function(c)
                                        Return If(rgbs.ContainsKey(c), rgbs(c), c)
                                    End Function) _
                            .ToArray
                        Dim min$ = colorValues(1), max$ = colorValues(2)  ' 和graph对象的属性值等级相关的连续渐变映射
                        Dim levels% = Val(colorValues(0))
                        Dim startColor = min.TranslateColor
                        Dim endColor = max.TranslateColor
                        Dim middle = GDIColors.Middle(startColor, endColor)

                        ' 进行颜色三次插值获取渐变结果
                        colors = {startColor, middle, endColor}.CubicSpline(levels)
                    End If

                    Dim range As DoubleRange = $"0,{colors.Length}"
                    Dim selector = t.var.SelectNodeValue
                    Dim getValue = Function(node As Node) Val(selector(node))
                    Return Function(nodes)
                               Dim index = nodes.ValDegreeAsSize(getValue, range) ' 在这里将属性值映射为等级的index，后面就可以直接引用颜色谱之中的结果了
                               Dim out = index _
                                   .Select(Function(map)
                                               Return New Map(Of Node, Color) With {
                                                   .Key = map.Key,
                                                   .Maps = colors(map.Maps) ' 将等级映射为网络之中的节点或者边的颜色
                                               }
                                           End Function) _
                                   .ToArray
                               Return out
                           End Function
                Else
                    Dim colorValues = t _
                        .values _
                        .Select(Function(c)
                                    Return If(rgbs.ContainsKey(c), rgbs(c), c).TranslateColor
                                End Function) _
                        .ToArray
                    Return Function(nodes)
                               Dim maps = nodes.DiscreteMapping(t.var)
                               Dim out = maps _
                                   .Select(Function(map)
                                               Return New Map(Of Node, Color) With {
                                                   .Key = map.Key,
                                                   .Maps = colorValues(map.Maps)
                                               }
                                           End Function) _
                                   .ToArray
                               Return out
                           End Function
                End If
            Else
                ' 使用单词进行直接映射
                Dim selector = expression.SelectNodeValue
                Return Function(nodes)
                           Return nodes _
                               .Select(Function(n)
                                           Return New Map(Of Node, Color) With {
                                               .Key = n,
                                               .Maps = CStrSafe(selector(n)).TranslateColor
                                           }
                                       End Function) _
                               .ToArray
                       End Function
            End If
        End Function

        ''' <summary>
        ''' 表达式之中的值不可以有逗号或者括号
        ''' </summary>
        ''' <param name="expression$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MapExpressionParser(expression$) As (var$, type$, values As String())
            Dim t$() = expression _
                .GetStackValue("(", ")") _
                .Trim("("c, ")"c) _
                .Split(","c)
            Return (t(0), t(1), t.Skip(2).ToArray)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expression$">
        ''' + 单词
        ''' + 数字
        ''' + map表达式：
        '''    + ``map(单词, Continuous, min, max)``
        '''    + ``map(单词, Discrete, size1, size2, size3, ...)``
        ''' </param>
        ''' <returns></returns>
        Public Function SizeExpression(expression$) As Func(Of Node(), Map(Of Node, Double)())
            If expression.MatchPattern(Casting.RegexpDouble) Then
                Dim r# = Val(expression)
                Return Function(nodes)
                           Return nodes _
                               .Select(Function(n)
                                           Return New Map(Of Node, Double) With {
                                               .Key = n,
                                               .Maps = r
                                           }
                                       End Function) _
                               .ToArray
                       End Function
            ElseIf expression.MatchPattern("map\(.+\)", RegexICSng) Then
                Dim t = expression.MapExpressionParser

                If t.type.TextEquals("Continuous") Then
                    Dim range As DoubleRange = $"{t.values(0)},{t.values(1)}"
                    Dim selector = t.var.SelectNodeValue
                    Dim getValue = Function(node As Node) Val(selector(node))
                    Return Function(nodes)
                               Return nodes.ValDegreeAsSize(getValue, range)
                           End Function
                Else
                    Dim sizeList#() = t.values _
                        .Select(AddressOf Val) _
                        .ToArray
                    Return Function(nodes)
                               Dim maps = nodes.DiscreteMapping(t.var)
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

            Else
                ' 单词
                Dim selector = expression.SelectNodeValue
                Return Function(nodes)
                           Return nodes _
                               .Select(Function(n)
                                           Return New Map(Of Node, Double) With {
                                               .Key = n,
                                               .Maps = Val(selector(n))
                                           }
                                       End Function) _
                               .ToArray
                       End Function
            End If
        End Function
    End Module
End Namespace
