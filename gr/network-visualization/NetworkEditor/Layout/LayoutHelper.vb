#Region "Microsoft.VisualBasic::3918c6b751e106f87c2687f3ca3ca075, gr\network-visualization\NetworkEditor\Layout\LayoutHelper.vb"

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
    '    Code Lines: 13 (61.90%)
    ' Comment Lines: 3 (14.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (23.81%)
    '     File Size: 735 B


    '     Module LayoutHelper
    ' 
    '         Sub: EnsurePositions
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Namespace NetworkEditor.Layout

    ''' <summary>
    ''' 布局前的公共准备：保证每个节点都有初始位置，避免算法读取 .x/.y 时空引用
    ''' </summary>
    Public Module LayoutHelper

        Public Sub EnsurePositions(g As NetworkGraph, Optional width% = 1000, Optional height% = 1000)
            For Each n As Node In g.vertex
                If n.data.initialPostion Is Nothing Then
                    n.data.initialPostion = New FDGVector2(Rnd() * width, Rnd() * height)
                End If
            Next
        End Sub

    End Module

End Namespace

