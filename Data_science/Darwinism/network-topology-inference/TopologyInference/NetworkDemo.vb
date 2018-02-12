#Region "Microsoft.VisualBasic::4d88a1a9d685614a40d97ec8d5ca62ab, Data_science\Darwinism\network-topology-inference\TopologyInference\NetworkDemo.vb"

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

    ' Class NetworkDemo
    ' 
    '     Function: y0
    ' 
    '     Sub: func
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Public Class NetworkDemo : Inherits ODEs

    Dim a, b, c, d As var

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        dy(a) = 0.01 * a + b + c
        dy(b) = c - d - 0.5 * b
        dy(c) = (c ^ -3) * d - 0.3 * c
        dy(d) = 2 * a - a ^ 0.6 - 0.2 * d
    End Sub

    Protected Overrides Function y0() As var()
        Return {
            a = 1,
            b = 1,
            c = 1,
            d = 1
        }
    End Function
End Class
