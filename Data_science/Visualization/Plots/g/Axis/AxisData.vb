#Region "Microsoft.VisualBasic::9277150f9916539941eca1b4d89b1948, Data_science\Visualization\Plots\g\Axis\AxisData.vb"

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

    '   Total Lines: 45
    '    Code Lines: 19 (42.22%)
    ' Comment Lines: 18 (40.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (17.78%)
    '     File Size: 1.17 KB


    '     Class AxisValue
    ' 
    '         Properties: Font, Range, Tick, Title
    ' 
    '         Function: ToString
    ' 
    '     Structure AxisData
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Axis

    ''' <summary>
    ''' 手工指定Axis的数据
    ''' </summary>
    Public Class AxisValue

        ''' <summary>
        ''' 最大值和最小值
        ''' </summary>
        ''' <returns></returns>
        Public Property Range As DoubleRange
        ''' <summary>
        ''' 数值标签出现的间隔
        ''' </summary>
        ''' <returns></returns>
        Public Property Tick As Double
        ''' <summary>
        ''' 坐标轴的标题
        ''' </summary>
        ''' <returns></returns>
        Public Property Title As String
        Public Property Font As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

    ''' <summary>
    ''' 横纵坐标轴的画图数据
    ''' </summary>
    Public Structure AxisData

        Dim X, Y As AxisValue

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
