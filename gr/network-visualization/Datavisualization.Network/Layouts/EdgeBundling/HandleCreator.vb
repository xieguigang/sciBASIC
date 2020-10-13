#Region "Microsoft.VisualBasic::8d2286cc75dd5442d235f9b50098dec6, gr\network-visualization\Datavisualization.Network\Layouts\EdgeBundling\HandleCreator.vb"

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

    '     Module HandleCreator
    ' 
    '         Function: calculateHandleLocation, defineHandle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports geometry = Microsoft.VisualBasic.Imaging.Math2D
Imports stdNum = System.Math

Namespace Layouts.EdgeBundling

    ''' <summary>
    ''' handle point creator algorithm module
    ''' </summary>
    Public Module HandleCreator

        <Extension>
        Public Function calculateHandleLocation(handle As Handle, source As PointF, target As PointF, isDirectMode As Boolean) As PointF
            Dim sX = source.X
            Dim sY = source.Y
            Dim tX = target.X
            Dim tY = target.Y

            If isDirectMode Then
                If handle.x = 0.0 AndAlso handle.y = 0.0 Then
                    ' If default, use center
                    Return New PointF(tX - sX, tY - sY)
                Else
                    Return handle.originalLocation
                End If
            Else
                Return handle.convert(sX, sY, tX, tY)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="source">边的起始位点</param>
        ''' <param name="target">边的终止位点</param>
        ''' <param name="hx">当前的这个拐点的X位置</param>
        ''' <param name="hy">当前的这个拐点的Y位置</param>
        ''' <returns></returns>
        Public Function defineHandle(source As PointF, target As PointF, hx#, hy#) As Handle
            ' Location of source node
            Dim sX As Decimal = source.X
            Dim sY As Decimal = source.Y

            ' Location of target node
            Dim tX As Decimal = target.X
            Dim tY As Decimal = target.Y

            ' Vector v1
            ' Distance from source to target (Edge length)
            Dim v1x = tX - sX
            Dim v1y = tY - sY
            ' final double dist1 = Math.sqrt(Math.pow(v1x, 2) + Math.pow(v1y, 2));
            Dim dist1 = geometry.Distance(sX, sY, tX, tY)
            Dim handle As New Handle

            If dist1 = 0.0 Then
                ' If the source And target are at the same location, use
                ' reasonable defaults.
                handle.ratio = 0
                handle.cosTheta = 0
                handle.sinTheta = 0
            Else
                ' Vector v2
                ' Distance from source to current handle
                Dim v2x As Decimal = hx - sX
                Dim v2y As Decimal = hy - sY
                ' final double dist2 = Math.sqrt(Math.pow(v2x, 2) + Math.pow(v2y, 2));
                Dim dist2 = geometry.Distance(sX, sY, hX, hY)

                ' Ratio of vector lengths
                handle.ratio = dist2 / dist1

                ' Dot product of v1 And v2
                Dim dotProduct = (v1x * v2x) + (v1y * v2y)
                handle.cosTheta = If(dotProduct = 0.0#, 0, dotProduct / (dist1 * dist2))

                ' Avoid rounding problem
                If handle.cosTheta > 1 Then
                    handle.cosTheta = 1
                End If

                ' Theta Is the angle between v1 And v2
                Dim theta = stdNum.Acos(handle.cosTheta)
                handle.sinTheta = stdNum.Sin(theta)

                Dim validate = handle.convert(sX, sY, tX, tY)

                If (stdNum.Abs(validate.X - hX) > 2 OrElse stdNum.Abs(validate.Y - hY) > 2) Then
                    handle.sinTheta = -handle.sinTheta
                End If

                ' Validate
                If (Double.IsNaN(theta) OrElse Double.IsNaN(handle.sinTheta)) Then
                    Throw New Exception($"Invalid angle: {theta}. Cuased by cos(theta) = {handle.cosTheta}")
                End If
            End If

            Return handle
        End Function
    End Module
End Namespace
