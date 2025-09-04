﻿#Region "Microsoft.VisualBasic::f24a3f591ef93d38be70aa5fa1f21a88, gr\network-visualization\network_layout\EdgeBundling\Mingle\InternalMath.vb"

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

    '   Total Lines: 49
    '    Code Lines: 39 (79.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (20.41%)
    '     File Size: 1.53 KB


    '     Module InternalMath
    ' 
    '         Function: dist, lerp, norm
    ' 
    '         Sub: (+2 Overloads) [Each], eachEdge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports number = System.Double
Imports std = System.Math

Namespace EdgeBundling.Mingle

    Public Module InternalMath

        Public ReadOnly MINGLE_PHI As Double = (1 + std.Sqrt(5)) / 2

        Public Function dist(a As number(), b As number()) As number
            Dim diffX = a(0) - b(0)
            Dim diffY = a(1) - b(1)

            Return std.Sqrt(diffX * diffX + diffY * diffY)
        End Function

        Public Function lerp(a As Vector, b As Vector, delta As Double) As Vector
            Return a * (1 - delta) + b * delta
        End Function

        Public Function norm(a As number()) As number
            Return std.Sqrt(a(0) * a(0) + a(1) * a(1))
        End Function

        <Extension>
        Public Sub [Each](g As NetworkGraph, apply As Action(Of Node))
            For Each v As Node In g.vertex
                Call apply(v)
            Next
        End Sub

        <Extension>
        Public Sub [Each](g As NetworkGraph, apply As Action(Of Edge))
            For Each link As Edge In g.graphEdges
                Call apply(link)
            Next
        End Sub

        <Extension>
        Public Sub eachEdge(n As Node, apply As Action(Of Edge))
            For Each link As Edge In n.directedVertex
                Call apply(link)
            Next
        End Sub
    End Module
End Namespace
