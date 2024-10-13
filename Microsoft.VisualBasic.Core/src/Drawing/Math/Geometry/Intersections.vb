#Region "Microsoft.VisualBasic::7cbe2116c4c0a85a4d59fee4610999e9, Microsoft.VisualBasic.Core\src\Drawing\Math\Geometry\Intersections.vb"

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
    '    Code Lines: 8 (38.10%)
    ' Comment Lines: 12 (57.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (4.76%)
    '     File Size: 448 B


    '     Enum Intersections
    ' 
    '         Containment, Intersection, None, Tangent
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Imaging.Math2D

    ''' <summary>
    ''' 几何体之间的关系类型
    ''' </summary>
    Public Enum Intersections As Byte
        None
        ''' <summary>
        ''' 正切
        ''' </summary>
        Tangent
        ''' <summary>
        ''' 相交
        ''' </summary>
        Intersection
        ''' <summary>
        ''' 包围
        ''' </summary>
        Containment
    End Enum
End Namespace
