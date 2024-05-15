#Region "Microsoft.VisualBasic::f8f2444cf2296bbe95a601dfb460a552, gr\network-visualization\Visualizer\Styling\MapperProcessor.vb"

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

    '   Total Lines: 31
    '    Code Lines: 12
    ' Comment Lines: 15
    '   Blank Lines: 4
    '     File Size: 1.13 KB


    '     Module MapperProcessor
    ' 
    '         Function: WritePropertyValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling.CSS

Namespace Styling

    ''' <summary>
    ''' Do style mapping from the parsed css file at here
    ''' </summary>
    Module MapperProcessor

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g">use the clone data</param>
        ''' <param name="styles"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WritePropertyValue(g As NetworkGraph, styles As StyleMapper) As NetworkGraph
            ' 因为在这个模块之中会涉及到修改原来的属性值
            ' 所以为了不影响原始的数据,
            ' 在这里必须要在新建的复制副本上来完成操作
            ' 因为edge中的node对象还是原来的node对象
            ' 所以在这里会需要进行重新赋值来解除对原始数据的引用
            g = g.Copy

            ' assign style data
            Return g
        End Function
    End Module
End Namespace
