#Region "Microsoft.VisualBasic::64a5fe31a6117149ee8a64b05e925c63, Data_science\Visualization\DataPlot\Engine\MarkerShape.vb"

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

    '   Total Lines: 18
    '    Code Lines: 13 (72.22%)
    ' Comment Lines: 5 (27.78%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 393 B


    ' Enum MarkerShape
    ' 
    '     Circle, Cross, Diamond, Hexagon, InvertedTriangle
    '     None, Pentagon, Plus, Square, Star
    '     Triangle
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>标记形状</summary>
Public Enum MarkerShape
    Circle
    Square
    Triangle
    Diamond
    Cross
    Plus
    ''' <summary>倒置三角形（尖端朝下）</summary>
    InvertedTriangle
    ''' <summary>正六边形</summary>
    Hexagon
    ''' <summary>正五边形</summary>
    Pentagon
    ''' <summary>五角星</summary>
    Star
    None
End Enum
