#Region "Microsoft.VisualBasic::2bd69b43c521ebf92a04606d63293e46, gr\network-visualization\Datavisualization.Network\Layouts\EdgeBundling\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: lerp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports stdNum = System.Math

Namespace Layouts.EdgeBundling

    <HideModuleName> Module Extensions

        Public ReadOnly PHI As Double = (1 + stdNum.Sqrt(5)) / 2

        Public Function lerp(a As PointF, b As PointF, delta As Double) As PointF
            Return New PointF With {
                .X = a.X * (1 - delta) + b.X * delta,
                .Y = a.Y * (1 - delta) + b.Y * delta
            }
        End Function
    End Module
End Namespace
