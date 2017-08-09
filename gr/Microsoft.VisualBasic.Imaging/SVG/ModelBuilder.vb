#Region "Microsoft.VisualBasic::bbe1630622d64d4102884f55b48387ea, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\ModelBuilder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports sys = System.Math

Namespace SVG

    Public Module ModelBuilder

        Public Function PiePath(x!, y!, width!, height!, startAngle!, sweepAngle!) As path
            Dim endAngle! = startAngle + sweepAngle
            Dim rX = width / 2
            Dim rY = height / 2
            Dim x1 = x + rX * sys.Cos(Math.PI * startAngle / 180)
            Dim y1 = y + rY * sys.Sin(Math.PI * startAngle / 180)
            Dim x2 = x + rX * sys.Cos(Math.PI * endAngle / 180)
            Dim y2 = y + rY * sys.Sin(Math.PI * endAngle / 180)
            Dim d = $"M{x},{y}  L{x1},{y1}  A{rX},{rY} 0 0,1 {x2},{y2} z" ' 1 means clockwise

            Return New path With {
                .d = d
            }
        End Function
    End Module
End Namespace
