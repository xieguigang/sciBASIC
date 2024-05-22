#Region "Microsoft.VisualBasic::3eb010f2816cbc69ad911197957bff86, gr\network-visualization\Visualizer\Styling\Expression\BrushExpression.vb"

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


    ' Code Statistics:

    '   Total Lines: 80
    '    Code Lines: 44 (55.00%)
    ' Comment Lines: 29 (36.25%)
    '    - Xml Docs: 79.31%
    ' 
    '   Blank Lines: 7 (8.75%)
    '     File Size: 3.56 KB


    '     Module BrushExpression
    ' 
    '         Function: brushMapper, discreteMapper, Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling.FillBrushes
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

Namespace Styling

    Module BrushExpression

        ''' <summary>
        ''' 通过这个函数来进行颜色填充以及图案填充的画笔对象的统一构建
        ''' </summary>
        ''' <param name="expression">
        ''' #### 使用图片作为填充材质
        ''' 
        ''' + url(filepath/data uri) 所有节点都使用统一的图案
        ''' + map(propertyName, val1=url1, val2=url2, val3=url3)  离散映射
        ''' + map(propertyName, [url(directory), *.png])  passthrough映射，属性值作为文件名，从directory之中读取图像文件，后面必须要跟文件拓展名
        ''' + rgb(x,x,x,x)|#xxxxx|xxxxx 颜色表达式，所有的节点都使用相同的颜色
        ''' + map(propertyName, val1=color1, val2=color2) 离散映射
        ''' + map(propertyName, [patternName, n]) 区间映射
        ''' </param>
        ''' <returns></returns>
        Public Function Evaluate(expression As String) As IGetBrush
            If UrlEvaluator.IsURLPattern(expression) Then
                Return New UnifyImageBrush(expression)
            ElseIf IsMapExpression(expression) Then
                ' 可能是离散图案映射，颜色映射
                Return expression.brushMapper
            ElseIf expression.IsColorExpression Then
                ' 可能是使用相同的颜色
                Return New UnifyColorBrush(expression)
            Else
                ' passthrough映射，即expression为属性名称，属性值就是颜色值
                Return New PassthroughBrush(expression)
            End If
        End Function

        ''' <summary>
        ''' 在这里处理图案映射和颜色映射，对于图案映射而言，是没有区间映射的
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        <Extension>
        Private Function brushMapper(expression As String) As IGetBrush
            Dim model As MapExpression = expression.MapExpressionParser

            If model.type = MapperTypes.Continuous Then
                ' 区间映射只能够是颜色映射了
                If model.values(1).TextEquals("category") Then
                    Return New CategoryBrush(model)
                ElseIf UrlEvaluator.IsURLPattern(model.values.First) Then
                    Return New ImagePassthroughBrush(model)
                Else
                    Return New ColorRangeBrush(model)
                End If
            Else
                ' 颜色和图案都可以具有离散映射
                Return model.discreteMapper
            End If
        End Function

        ''' <summary>
        ''' 在离散映射之中，颜色和图案可能会进行混用
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        <Extension>
        Private Function discreteMapper(model As MapExpression) As IGetBrush
            Dim data = model.values

            If data.Length = 1 AndAlso Not data(Scan0).Contains("="c) Then
                Return New DiscreteSequenceBrush(model)
            Else
                Return New DiscreteBrush(model)
            End If
        End Function
    End Module
End Namespace
