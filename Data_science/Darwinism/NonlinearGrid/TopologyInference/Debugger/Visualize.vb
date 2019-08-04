#Region "Microsoft.VisualBasic::d08a87ebc22bc46412d23db125271d88, Data_science\Darwinism\NonlinearGrid\TopologyInference\Debugger\Visualize.vb"

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

    ' Module Visualize
    ' 
    '     Function: CreateGraph, NodeCorrelation, NodeImpacts, ROC
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module Visualize

    <Extension>
    Public Function ROC(result As IEnumerable(Of FittingValidation), Optional positiveRange As Boolean = True) As IEnumerable(Of Validation)
        With result.ToArray
            Dim range As New DoubleRange(.Select(Function(d) d.actual))
            Dim seq As New Sequence With {
                .range = range,
                .n = 100
            }
            Dim getPredicts As Func(Of FittingValidation, Double, Boolean) =
                Function(x, t)
                    If x.predicts < 0 AndAlso positiveRange Then
                        Return 0 >= t
                    Else
                        Return x.predicts >= t
                    End If
                End Function

            Return Validation.ROC(Of FittingValidation)(
                entity:= .ByRef,
                getValidate:=Function(x, t) x.actual >= t,
                getPredict:=getPredicts,
                threshold:=seq
            )
        End With
    End Function

    ''' <summary>
    ''' 只计算指数部分
    ''' </summary>
    ''' <param name="grid"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' impact影响系数只是用来描述指标对目标函数的值的改变的影响程度
    ''' 当impact为正实数的时候, 目标指标会对目标函数产生实际影响
    ''' 当impact为零的时候,指数项计算结果等于1,目标指标对目标函数的结果值产生有限的影响
    ''' 当impact小于零的时候,指数项计算结果趋向于零,目标指标对目标函数的结果值无影响
    ''' </remarks>
    <Extension>
    Public Iterator Function NodeImpacts(grid As GridMatrix) As IEnumerable(Of NamedValue(Of Double))
        For i As Integer = 0 To grid.correlations.Length - 1
            Dim factor As NumericVector = grid.correlations(i)
            Dim c As Double = grid.const.B(i)
            Dim impact As Double = c + factor.vector.Sum

            Yield New NamedValue(Of Double) With {
               .Name = factor.name,
               .Value = impact
            }
        Next
    End Function

    ''' <summary>
    ''' 相关性则是计算 a * E ^ P 整个表达式
    ''' </summary>
    ''' <param name="grid"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 含义与相关性系数相似,正实数表示正相关,负实数表示负相关,零表示不相关
    ''' </remarks>
    <Extension>
    Public Iterator Function NodeCorrelation(grid As GridMatrix) As IEnumerable(Of NamedValue(Of Double))
        Dim impacts = grid.NodeImpacts.ToArray

        For i As Integer = 0 To grid.correlations.Length - 1
            Dim A As Double = grid.direction(i)
            Dim B As Double = Math.E ^ impacts(i).Value

            Yield New NamedValue(Of Double) With {
                .Name = impacts(i).Name,
                .Value = B * A,
                .Description = impacts(i).Value
            }
        Next
    End Function

    ''' <summary>
    ''' Create network graph model from the grid system status.
    ''' </summary>
    ''' <param name="grid"></param>
    ''' <param name="cutoff">
    ''' 系统中的变量与结果之间的相关度因子的阈值，低于这个阈值的边都会被删掉，也就是只会留下相关度较高的边
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 对于一个网格系统而言, 其计算格式为: ``Xi ^ (c * Xj)``. 因为对于c * Xj而言:
    ''' 
    ''' + 乘积结果肯定是越大,则Xi越大, 此时Xi项目对系统输出的结果的影响也越大
    ''' + 假若c为负数的时候,则Xi指数计算结果接近于零,c越小Xi项目越接近于零,此时Xi项目对系统输出的结果的影响也越小
    ''' + 当c接近于零的时候,其对Xi没有影响度,因为Xi^0 =1, 影响整个Xi的只能够从其他的系统变量因子获取
    ''' 
    ''' 所以c因子可以看作为Xj与Xi之间的相关度, 只不过这个相关度是位于整个[负无穷, 正无穷]之间的
    ''' </remarks>
    <Extension>
    Public Function CreateGraph(grid As GridMatrix, Optional cutoff# = 1, Optional nameTitles As Dictionary(Of String, String) = Nothing) As NetworkGraph
        Dim g As New NetworkGraph
        Dim node As Node
        Dim variableNames As New List(Of String)
        Dim edge As EdgeData
        Dim importance As Dictionary(Of String, Double) = grid _
            .NodeImpacts _
            .ToDictionary(Function(n) n.Name,
                          Function(n)
                              Return n.Value
                          End Function)

        Dim impacts As Vector = importance.Values.AsVector
        Dim posValues As DoubleRange = impacts(impacts > 0)
        Dim negValues As DoubleRange = impacts(impacts < 0)
        ' get color sequence and then removes white colors
        Dim JetColors As Color() = Imaging.ColorSequence(, name:="Jet").Where(Function(c) Not c.IsBlackOrWhite(offset:=20)).ToArray
        Dim posColor As Color() = Imaging.ColorSequence(, name:=ColorMap.PatternHot).Where(Function(c) Not c.IsBlackOrWhite(offset:=20)).ToArray
        Dim negColor As Color() = Imaging.ColorSequence(, name:=ColorMap.PatternWinter).Where(Function(c) Not c.IsBlackOrWhite(offset:=20)).ToArray
        Dim getColor = Function(impact As Double) As Color
                           Dim index As Integer
                           Dim colors As Color()

                           If impact > 0 Then
                               index = posValues.ScaleMapping(impact, {0, posColor.Length - 1})
                               colors = posColor
                           Else
                               index = negValues.ScaleMapping(impact, {0, negColor.Length - 1})
                               index = (negColor.Length - 1) - index
                               colors = negColor
                           End If

                           Return colors(index)
                       End Function

        If nameTitles Is Nothing Then
            nameTitles = New Dictionary(Of String, String)
        End If

        For Each factor As NumericVector In grid.correlations
            node = New Node With {
                .data = New NodeData With {
                    .label = factor.name,
                    .origID = factor.name,
                    .mass = importance(factor.name),
                    .radius = importance(factor.name),
                    .Properties = New Dictionary(Of String, String) From {
                        {"impacts", importance(factor.name)},
                        {"color", getColor(importance(factor.name)).ToHtmlColor},
                        {"size", Math.Abs(importance(factor.name))},
                        {"title", nameTitles.TryGetValue(factor.name, [default]:=factor.name)}
                    }
                },
                .Label = factor.name,
                .ID = 0
            }

            variableNames += factor.name
            g.AddNode(node)
        Next

        Dim correlations As DoubleRange = grid _
            .correlations _
            .Select(Function(c) c.vector) _
            .IteratesALL _
            .ToArray

        For Each factor As NumericVector In grid.correlations
            For i As Integer = 0 To factor.Length - 1
                ' PCC/SPCC相关度等，位于[-1,1]之间
                ' 而这个的相关度则是位于[负无穷, 正无穷]之间
                If factor.name <> variableNames(i) AndAlso Math.Abs(factor(i)) >= cutoff Then
                    ' 跳过自己和自己的链接
                    ' 以及低相关度的节点链接
                    edge = New EdgeData With {
                        .label = $"{factor.name} ^ {variableNames(i)}",
                        .weight = factor(i),
                        .Properties = New Dictionary(Of String, String) From {
                            {"correlation", factor(i)},
                            {"dash", If(factor(i) > 0, "solid", "dash")},
                            {"width", Math.Abs(factor(i))},
                            {"color", JetColors(correlations.ScaleMapping(factor(i), {0, JetColors.Length - 1})).ToHtmlColor}
                        }
                    }
                    g.CreateEdge(factor.name, variableNames(i), edge)
                End If
            Next
        Next

        Return g
    End Function
End Module
