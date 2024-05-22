#Region "Microsoft.VisualBasic::39db581c8cc39e6f63049ee91c588d4b, gr\network-visualization\network_layout\Cola\Models\IGraphNode.vb"

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

    '   Total Lines: 45
    '    Code Lines: 15 (33.33%)
    ' Comment Lines: 16 (35.56%)
    '    - Xml Docs: 18.75%
    ' 
    '   Blank Lines: 14 (31.11%)
    '     File Size: 1.48 KB


    '     Interface IGraphNode
    ' 
    '         Properties: bounds, fixed, fixedWeight, height, px
    '                     py, variable, width, x, y
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript

Namespace Cola

    ''' <summary>
    ''' The common type interface that will be used in <see cref="Projection"/>
    ''' </summary>
    Public Interface IGraphNode : Inherits IJavaScriptObjectAccessor
        Property bounds As Rectangle2D
        Property variable As Variable
        Property width As Double
        Property height As Double
        Property px As Double?
        Property py As Double?
        Property x As Double
        Property y As Double

        Property fixed As Boolean
        Property fixedWeight As Double?

    End Interface

    'Public Class GraphNode : Inherits Leaf
    '    Implements IGraphNode

    '    Public Property width As Double Implements IGraphNode.width
    '    Public Property height As Double Implements IGraphNode.height

    '    Public Overrides Property variable As Variable Implements IGraphNode.variable

    '    Public Overrides Property bounds As Rectangle2D Implements IGraphNode.bounds

    '    Public Property px As Double? Implements IGraphNode.px

    '    Public Property py As Double? Implements IGraphNode.py

    '    Public Property x As Double Implements IGraphNode.x

    '    Public Property y As Double Implements IGraphNode.y

    '    Public Property fixed As Boolean Implements IGraphNode.fixed

    '    Public Property fixedWeight As Double? Implements IGraphNode.fixedWeight
    'End Class
End Namespace
