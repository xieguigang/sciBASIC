#Region "Microsoft.VisualBasic::50aa33ad0b965a7cb1a69a00a5cca9aa, gr\network-visualization\NetworkEditor\Layout\ILayoutRunner.vb"

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

    '   Total Lines: 15
    '    Code Lines: 9 (60.00%)
    ' Comment Lines: 3 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (20.00%)
    '     File Size: 497 B


    '     Interface ILayoutRunner
    ' 
    '         Properties: Name
    ' 
    '         Function: GetParameters
    ' 
    '         Sub: Apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace NetworkEditor.Layout

    ''' <summary>
    ''' 统一布局接口：名称 / 参数对象(绑定 PropertyGrid) / 应用
    ''' </summary>
    Public Interface ILayoutRunner
        ReadOnly Property Name As String
        Function GetParameters() As Object
        Sub Apply(g As NetworkGraph, params As Object, Optional progress As Action(Of String) = Nothing)
    End Interface

End Namespace

