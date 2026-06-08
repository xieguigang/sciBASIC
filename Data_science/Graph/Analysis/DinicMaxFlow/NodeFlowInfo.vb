#Region "Microsoft.VisualBasic::f9ad3606819f6a535dee5d6c1d9e86df, Data_science\Graph\Analysis\DinicMaxFlow\NodeFlowInfo.vb"

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

    '   Total Lines: 24
    '    Code Lines: 18 (75.00%)
    ' Comment Lines: 4 (16.67%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 2 (8.33%)
    '     File Size: 1.10 KB


    '     Class NodeFlowInfo
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DinicMaxFlow

    ' ========================================================================
    ' 节点流量分布信息
    ' 记录每个节点的流入、流出、净流量等信息
    ' ========================================================================
    Public Class NodeFlowInfo
        Public NodeIndex As Integer       ' 节点编号
        Public TotalInFlow As Integer     ' 总流入量
        Public TotalOutFlow As Integer    ' 总流出量
        Public NetFlow As Integer         ' 净流量 (流入 - 流出)
        Public IsSource As Boolean        ' 是否为源点
        Public IsSink As Boolean          ' 是否为汇点

        Public Overrides Function ToString() As String
            Dim role As String = ""
            If IsSource Then role = " [源点]"
            If IsSink Then role = " [汇点]"
            Return String.Format(
                "节点{0}: 流入={1}, 流出={2}, 净流量={3}{4}",
                NodeIndex, TotalInFlow, TotalOutFlow, NetFlow, role)
        End Function
    End Class
End Namespace
