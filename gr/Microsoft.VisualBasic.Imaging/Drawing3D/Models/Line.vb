#Region "Microsoft.VisualBasic::2f5536584b129f25cdf42389d5b5cc67, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Line.vb"

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

    '   Total Lines: 41
    '    Code Lines: 32
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.33 KB


    '     Structure Line3D
    ' 
    '         Function: Copy, GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.Models

    Public Structure Line3D
        Implements IEnumerable(Of Point3D)
        Implements I3DModel

        Public a, b As Point3D
        Public pen As Pen

        Public Sub Draw(ByRef canvas As IGraphics, camera As Camera) Implements I3DModel.Draw
            Dim pts As PointF() = camera _
                .Project(Me) _
                .Select(Function(pt) pt.PointXY(camera.screen)) _
                .ToArray

            Call canvas.DrawLine(pen, pts(0), pts(1))
        End Sub

        Public Function Copy(data As IEnumerable(Of Point3D)) As I3DModel Implements I3DModel.Copy
            Dim array As Point3D() = data.ToArray

            Return New Line3D With {
                .a = data(0),
                .b = data(1),
                .pen = pen
            }
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Point3D) Implements IEnumerable(Of Point3D).GetEnumerator
            Yield a
            Yield b
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Structure
End Namespace
