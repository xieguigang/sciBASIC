#Region "Microsoft.VisualBasic::5a643ab7c3309d790881b89a644455af, Data_science\Visualization\DataPlot\Engine\Series.vb"

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

    '   Total Lines: 13
    '    Code Lines: 11 (84.62%)
    ' Comment Lines: 1 (7.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (7.69%)
    '     File Size: 484 B


    ' Class Series
    ' 
    '     Properties: Color, LineStyle, MarkerShape, Name, Visible
    '                 X, Y
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

''' <summary>图表数据系列</summary>
Public Class Series

    Public Property Name As String = ""
    Public Property Color As Color? = Nothing
    Public Property X As Double() = {}
    Public Property Y As Double() = {}
    Public Property MarkerShape As MarkerShape = MarkerShape.Circle
    Public Property LineStyle As DashStyle = DashStyle.Solid
    Public Property Visible As Boolean = True

    ''' <summary>每个点的尺寸因子（气泡图用），留空时所有点同尺寸</summary>
    Public Property Size As Double() = Nothing

    ''' <summary>每个点的负向误差（与 <see cref="Y"/> 同值域），可为 Nothing</summary>
    Public Property ErrorMinus As Double() = Nothing

    ''' <summary>每个点的正向误差，可为 Nothing</summary>
    Public Property ErrorPlus As Double() = Nothing

    ''' <summary>点标记尺寸，小于等于 0 时取主题的 <c>MarkerSize</c></summary>
    Public Property PointSize As Single = 0

    ''' <summary>面积图 / 柱状图的填充色，留空时取 <see cref="Color"/> 的半透明版本</summary>
    Public Property FillColor As Color? = Nothing

    ''' <summary>每个点的标签（可为 Nothing；只有少数图表会用到）</summary>
    Public Property PointLabels As String() = Nothing

    ''' <summary>填充透明度（面积图、置信带等场景）</summary>
    Public Property FillAlpha As Integer = 120

    Public Overrides Function ToString() As String
        Return $"{Name} {CType(Color, Color).ToHtmlColor}"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function FromPoints(pts As IEnumerable(Of Point), color As String, title As String) As Series
        Return FromPoints(pts.PointF, color, title)
    End Function

    Public Shared Function FromPoints(pts As IEnumerable(Of PointF), color As String, title As String) As Series
        Dim ptVec = pts.SafeQuery.ToArray

        Return New Series With {
            .Color = color.TranslateColor,
            .LineStyle = DashStyle.Solid,
            .MarkerShape = MarkerShape.Circle,
            .Name = title,
            .Visible = True,
            .X = ptVec.X,
            .Y = ptVec.Y
        }
    End Function
End Class
