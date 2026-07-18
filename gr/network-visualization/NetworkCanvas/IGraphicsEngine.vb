#Region "Microsoft.VisualBasic::53ca2b234d8f673c311e0d0ac5656ee8, gr\network-visualization\NetworkCanvas\IGraphicsEngine.vb"

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

    '   Total Lines: 8
    '    Code Lines: 4 (50.00%)
    ' Comment Lines: 3 (37.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (12.50%)
    '     File Size: 281 B


    ' Interface IGraphicsEngine
    ' 
    '     Properties: ShowLabels, View
    ' 
    ' /********************************************************************************/

#End Region

Public Interface IGraphicsEngine

    Property ShowLabels As Boolean
    ''' <summary>
    ''' 渲染视图状态（视口、悬停/选中高亮、LOD 与网格）。由 Canvas 在每帧绘制前下发。
    ''' </summary>
    Property View As CanvasViewState
End Interface
