#Region "Microsoft.VisualBasic::f5754cbc26cca5ff85a3c17f25939e13, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Stairs.vb"

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

    '     Class Stairs
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.Models.Isometric.Shapes


    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>
    Public Class Stairs : Inherits Shape3D

        Public Sub New(origin As Point3D, stepCount As Double)
            Dim paths As Path3D() = New Path3D(CInt(Fix(stepCount)) * 2 + 2 - 1) {}
            Dim zigzag As New Path3D
            Dim points As Point3D() = New Point3D(CInt(Fix(stepCount)) * 2 + 2 - 1) {}
            Dim count As Integer = 1

            points(0) = origin

            For i As Integer = 0% To stepCount - 1
                Dim stepCorner As Point3D = origin.Translate(0, i / stepCount, (i + 1) / stepCount)

                paths(count - 1) = {
                    stepCorner,
                    stepCorner.Translate(0, 0, -1 / stepCount),
                    stepCorner.Translate(1, 0, -1 / stepCount),
                    stepCorner.Translate(1, 0, 0)
                }
                points(count) = stepCorner
                count += 1
                paths(count - 1) = {
                    stepCorner,
                    stepCorner.Translate(1, 0, 0),
                    stepCorner.Translate(1, 1 / stepCount, 0),
                    stepCorner.Translate(0, 1 / stepCount, 0)
                }
                points(count) = stepCorner.Translate(0, 1 / stepCount, 0)
                count += 1
            Next

            points(count) = origin.Translate(0, 1, 0)
            zigzag.Points = points.AsList
            paths(count - 1) = zigzag
            count += 1
            paths(count - 1) = zigzag.Reverse().TranslatePoints(1, 0, 0)
            Me.paths = paths.AsList
        End Sub
    End Class
End Namespace
