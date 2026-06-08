#Region "Microsoft.VisualBasic::51c419b393488b49c4c5a7f8f54418fe, Data_science\Graph\Analysis\DinicMaxFlow\FlowEdge.vb"

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

    '   Total Lines: 21
    '    Code Lines: 14 (66.67%)
    ' Comment Lines: 4 (19.05%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (14.29%)
    '     File Size: 812 B


    '     Structure FlowEdge
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace DinicMaxFlow

    ' ========================================================================
    ' 流量边结构体
    ' 存储每条边的终点、容量、当前流量及反向边索引
    ' ========================================================================
    Public Structure FlowEdge
        Public [To] As Integer        ' 边的终点节点编号
        Public Capacity As Integer    ' 边的最大容量
        Public Flow As Integer        ' 边的当前流量
        Public Rev As Integer         ' 反向边在邻接表中的索引

        Public Sub New(toNode As Integer, cap As Integer, revIdx As Integer)
            [To] = toNode
            Capacity = cap
            Flow = 0
            Rev = revIdx
        End Sub
    End Structure

End Namespace
