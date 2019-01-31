#Region "Microsoft.VisualBasic::7bea1ca07eea63cb5c177327511268e4, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout3D\Module1.vb"

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

    '     Class IConstraint
    ' 
    '         Properties: axis, gap, left, right
    ' 
    '     Interface DirectedEdgeConstraints
    ' 
    '         Properties: axis, gap
    ' 
    '     Class LinkSepAccessor
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Properties: getMinSeparation
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Layouts.Cola

    Public Class IConstraint
        Public Property axis As String
        Public Property left() As Double
        Public Property right() As Double
        Public Property gap() As Double
    End Class


    Public Interface DirectedEdgeConstraints
        Property axis() As String
        Property gap() As Double
    End Interface

    Public Class LinkSepAccessor(Of Link)
        Inherits LinkAccessor(Of Link)

        Public Delegate Function IGetMinSeperation(l As Link) As Double

        Public Property getMinSeparation As IGetMinSeperation
    End Class
End Namespace
