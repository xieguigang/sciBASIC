#Region "Microsoft.VisualBasic::c6818dfe6fa916242f3c167754e0b905, Data_science\Mathematica\Math\Math\Spline\CentripetalCatmullRomSpline.vb"

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

    '   Total Lines: 70
    '    Code Lines: 37 (52.86%)
    ' Comment Lines: 22 (31.43%)
    '    - Xml Docs: 90.91%
    ' 
    '   Blank Lines: 11 (15.71%)
    '     File Size: 3.28 KB


    '     Module CentripetalCatmullRomSpline
    ' 
    '         Function: CatmulRom, GetT
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Language

Namespace Interpolation

    ''' <summary>
    ''' ###### Centripetal Catmull–Rom spline
    ''' 
    ''' In computer graphics, centripetal Catmull–Rom spline is a variant form of 
    ''' Catmull-Rom spline formulated by Edwin Catmull and Raphael Rom according 
    ''' to the work of Barry and Goldman. It is a type of interpolating spline 
    ''' (a curve that goes through its control points) defined by four control points
    ''' P0, P1, P2, P3, with the curve drawn only from P1 to P2.
    ''' 
    ''' > https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline#cite_ref-1
    ''' </summary>
    Public Module CentripetalCatmullRomSpline

        ''' <summary>
        ''' In computer graphics, centripetal Catmull–Rom spline is a variant form of 
        ''' Catmull-Rom spline formulated by Edwin Catmull and Raphael Rom according 
        ''' to the work of Barry and Goldman. It is a type of interpolating spline 
        ''' (a curve that goes through its control points) defined by four control points
        ''' P0, P1, P2, P3, with the curve drawn only from P1 to P2.
        ''' </summary>
        ''' <param name="pa">four control points P0, P1, P2, P3, with the curve drawn only from P1 to P2.</param>
        ''' <param name="alpha!">set from 0-1</param>
        ''' <param name="amountOfPoints!">How many points you want on the curve</param>
        ''' <returns>points on the Catmull curve so we can visualize them</returns>
        Public Function CatmulRom(pa As PointF, pb As PointF, pc As PointF, pd As PointF, Optional alpha! = 0.5F, Optional amountOfPoints! = 10.0F) As List(Of PointF)
            Dim p0 As New Vector(shorts:={pa.X, pa.Y})
            Dim p1 As New Vector(shorts:={pb.X, pb.Y})
            Dim p2 As New Vector(shorts:={pc.X, pc.Y})
            Dim p3 As New Vector(shorts:={pd.X, pd.Y})

            Dim t0! = 0.0F
            Dim t1! = GetT(t0, alpha, p0, p1)
            Dim t2! = GetT(t1, alpha, p1, p2)
            Dim t3! = GetT(t2, alpha, p2, p3)

            Dim newPoints As New List(Of PointF)

            For t! = t1 To t2 Step ((t2 - t1) / amountOfPoints)
                Dim A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1
                Dim A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2
                Dim A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3

                Dim B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2
                Dim B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3
                Dim C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2

                newPoints += New PointF With {
                    .X = C(Scan0),
                    .Y = C(1)
                }
            Next

            Return newPoints
        End Function

        Public Function GetT(t!, alpha!, p0 As Vector, p1 As Vector) As Single
            Dim A! = (p1(Scan0) - p0(Scan0)) ^ 2.0F + (p1(1) - p0(1)) ^ 2.0F
            Dim b! = A ^ 0.5F
            Dim c! = b ^ alpha

            Return c + t
        End Function
    End Module
End Namespace
