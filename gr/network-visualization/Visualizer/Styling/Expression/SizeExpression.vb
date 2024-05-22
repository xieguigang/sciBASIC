#Region "Microsoft.VisualBasic::40486af6a63ebeb87d20bed9a37c0695, gr\network-visualization\Visualizer\Styling\Expression\SizeExpression.vb"

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

    '   Total Lines: 49
    '    Code Lines: 28 (57.14%)
    ' Comment Lines: 16 (32.65%)
    '    - Xml Docs: 93.75%
    ' 
    '   Blank Lines: 5 (10.20%)
    '     File Size: 1.75 KB


    '     Module SizeExpression
    ' 
    '         Function: Evaluate, mappingSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling.Numeric
Imports Microsoft.VisualBasic.Imaging.d3js.scale
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
        Public Function Evaluate(expression As String) As IGetSize
            If expression.IsPattern(Casting.RegexpDouble) Then
                Return New UnifyNumber(expression)
            ElseIf IsMapExpression(expression) Then
                Return expression.mappingSize
            ElseIf expression.TextEquals("system.object") Then
                Return New UnifyNumber(12)
            Else
                Return New PassthroughNumber(expression)
            End If
        End Function

        ''' <summary>
        ''' 离散映射或者区间映射
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        <Extension>
        Private Function mappingSize(expression As String) As IGetSize
            Dim t As MapExpression = expression.MapExpressionParser

            If t.type = MapperTypes.Continuous Then
                Return New ContinuousNumber(map:=t)
            Else
                Return New DiscreteNumber(map:=t)
            End If
        End Function
    End Module
End Namespace
