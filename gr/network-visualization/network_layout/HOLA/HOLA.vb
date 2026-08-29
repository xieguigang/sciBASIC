#Region "Microsoft.VisualBasic::dfa687390641e59911c0ca4ece0dccdb, gr\network-visualization\network_layout\HOLA\HOLA.vb"

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

    '   Total Lines: 23
    '    Code Lines: 10 (43.48%)
    ' Comment Lines: 10 (43.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (13.04%)
    '     File Size: 969 B


    '     Module [HOLA]
    ' 
    '         Function: DoLayout
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Namespace Hola

    ''' <summary>
    ''' HOLA 网络布局算法的静态入口。供 test 项目与其它调用方直接使用：
    ''' <code>Call HOLA.DoLayout(g)</code>
    ''' </summary>
    Public Module [HOLA]

        ''' <summary>
        ''' 对网络图执行 HOLA 正交布局，并把结果写回到节点的 <see cref="NodeData.initialPostion"/>。
        ''' </summary>
        ''' <param name="graph">要布局的网络图</param>
        ''' <param name="opts">可选算法参数</param>
        ''' <returns>已布局的同一图实例</returns>
        Public Function DoLayout(graph As NetworkGraph, Optional opts As HolaOptions = Nothing) As NetworkGraph
            Return New HolaLayouter().Layout(graph, opts)
        End Function
    End Module
End Namespace
