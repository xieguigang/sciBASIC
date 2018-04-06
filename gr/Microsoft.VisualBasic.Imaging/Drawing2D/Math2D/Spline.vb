#Region "Microsoft.VisualBasic::70941426519c48ee2fea5493ec40f84f, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Spline.vb"

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

    '     Module Spline
    ' 
    '         Function: (+2 Overloads) BezierCurve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Math.Interpolation

Namespace Drawing2D.Math2D

    Public Module Spline

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function BezierCurve(points As IEnumerable(Of Point), Optional iteration% = 10) As IEnumerable(Of PointF)
            Return points.PointF.BezierCurve(iteration)
        End Function

        <Extension>
        Public Iterator Function BezierCurve(points As IEnumerable(Of PointF), Optional iteration% = 10) As IEnumerable(Of PointF)
            Dim smooth As IEnumerable(Of PointF)
            Dim pointData = points.ToArray
            Dim bezier As BezierCurve

            For Each block In pointData.Join({pointData.First, pointData(1)}).SlideWindows(3)
                bezier = New BezierCurve(block(0), block(1), block(2), iteration)
                smooth = bezier.BezierPoints

                For Each p As PointF In smooth
                    Yield p
                Next
            Next
        End Function
    End Module
End Namespace
