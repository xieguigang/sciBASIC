Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports names = Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic.NameOf
Imports r = System.Text.RegularExpressions.Regex

Namespace Styling

    Module SizeExpression

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
        Public Function Evaluate(expression As String) As GetSize
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
                                                   .maps = sizeList(map.Maps)
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