#Region "Microsoft.VisualBasic::cfe26d161fb4ac62892a3783de487a62, gr\network-visualization\network_layout\ForceDirected\ForceDirectedParameters.vb"

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

    '   Total Lines: 40
    '    Code Lines: 24 (60.00%)
    ' Comment Lines: 4 (10.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (30.00%)
    '     File Size: 1.86 KB


    '     Class ForceDirectedParameters
    ' 
    '         Properties: CanvasHeight, CanvasWidth, CondenseFactor, DistThreshold, EjectFactor
    '                     Iterations, MaxTx, MaxTy
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace ForceDirected

    ''' <summary>
    ''' Force-Directed 力导向布局参数，可在 PropertyGrid 中编辑。
    ''' 与 <see cref="Planner"/> 构造函数参数一一对应。
    ''' </summary>
    Public Class ForceDirectedParameters

        <Category("受力"), DisplayName("排斥因子"), Description("节点间排斥力强度，值越大节点越分散")>
        Public Property EjectFactor As Integer = 6

        <Category("受力"), DisplayName("吸引因子"), Description("边两端节点的吸引力强度")>
        Public Property CondenseFactor As Integer = 3

        <Category("位移"), DisplayName("最大位移X"), Description("单步迭代中节点的水平位移上限")>
        Public Property MaxTx As Integer = 4

        <Category("位移"), DisplayName("最大位移Y"), Description("单步迭代中节点的垂直位移上限")>
        Public Property MaxTy As Integer = 3

        <Category("距离"), DisplayName("距离阈值"), Description("排斥力生效的距离区间，格式如 30,250")>
        Public Property DistThreshold As String = "30,250"

        <Category("画布"), DisplayName("画布宽度"), Description("布局画布尺寸（像素），影响 ideal 距离 k")>
        Public Property CanvasWidth As Integer = 1000

        <Category("画布"), DisplayName("画布高度"), Description("布局画布尺寸（像素），影响 ideal 距离 k")>
        Public Property CanvasHeight As Integer = 1000

        <Category("迭代"), DisplayName("迭代次数"), Description("调用 Collide 的步数")>
        Public Property Iterations As Integer = 300

        Public Overrides Function ToString() As String
            Return $"Force-Directed (iter={Iterations})"
        End Function
    End Class

End Namespace

