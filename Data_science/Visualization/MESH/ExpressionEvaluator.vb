#Region "Microsoft.VisualBasic::6e37cdfcfd4adc108acd774b275d6bec, Data_science\Visualization\Expression3DPlotter\ExpressionEvaluator.vb"

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

    '   Total Lines: 35
    '    Code Lines: 17 (48.57%)
    ' Comment Lines: 13 (37.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (14.29%)
    '     File Size: 1.20 KB


    ' Class ExpressionEvaluator
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) Evaluate
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression

''' <summary>
''' 对用户输入的数学表达式进行预编译，并在网格/参数采样点上安全求值。
''' 解析失败会向上抛出异常，由界面捕获并提示用户。
''' </summary>
Public Class ExpressionEvaluator

    Private ReadOnly engine As New ExpressionEngine()
    Private ReadOnly expr As Impl.Expression

    ''' <summary>
    ''' 预编译一个数学表达式字符串（仅解析一次）。
    ''' </summary>
    Public Sub New(expression As String)
        Me.expr = ExpressionEngine.Parse(expression)
    End Sub

    ''' <summary>
    ''' 计算 f(x, y)，用于三维曲面 z=f(x,y) 的网格采样。
    ''' </summary>
    Public Function Evaluate(x As Double, y As Double) As Double
        engine.SetSymbol("x", x)
        engine.SetSymbol("y", y)
        Return engine.Evaluate(expr)
    End Function

    ''' <summary>
    ''' 计算 f(t)，用于参数化三维曲线的参数采样。
    ''' </summary>
    Public Function Evaluate(t As Double) As Double
        engine.SetSymbol("t", t)
        Return engine.Evaluate(expr)
    End Function
End Class

