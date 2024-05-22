#Region "Microsoft.VisualBasic::d483a35aba5e29c5aa40b2694a3c8bb3, Data_science\Visualization\Plots\Contour\HeatMap\EvaluatePoints.vb"

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

    '   Total Lines: 26
    '    Code Lines: 11 (42.31%)
    ' Comment Lines: 9 (34.62%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (23.08%)
    '     File Size: 761 B


    '     Class EvaluatePoints
    ' 
    ' 
    ' 
    '     Class FormulaEvaluate
    ' 
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Contour.HeatMap

    Public MustInherit Class EvaluatePoints

        Public MustOverride Function Evaluate(x As Double, y As Double) As Double

    End Class

    Public Class FormulaEvaluate : Inherits EvaluatePoints

        ''' <summary>
        ''' evaluate of the z from [x, y]
        ''' </summary>
        Public formula As Func(Of Double, Double, Double)

        ''' <summary>
        ''' 得到通过计算返回来的数据
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(x As Double, y As Double) As Double
            Return formula(x, y)
        End Function
    End Class
End Namespace
