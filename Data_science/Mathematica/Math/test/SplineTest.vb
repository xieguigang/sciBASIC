#Region "Microsoft.VisualBasic::2cc647a87c1072853036d6f9bc3cba98, sciBASIC#\Data_science\Mathematica\Math\test\SplineTest.vb"

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

    '   Total Lines: 24
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 636.00 B


    ' Module SplineTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.FileIO

Module SplineTest

    Sub Main()
        Dim points = {
            New PointF(10, 10),
            New PointF(20, 20),
            New PointF(25, 30),
            New PointF(30, 60),
            New PointF(40, 50),
            New PointF(50, 10),
            New PointF(60, 1),
            New PointF(70, -5)
        }

        Dim spline = PolynomialNewton.NewtonPolynomial(points).ToArray

        Call points.DumpSerial("./in.csv")
        Call spline.DumpSerial("./out.csv")
    End Sub
End Module
