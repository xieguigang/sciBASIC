#Region "Microsoft.VisualBasic::b2599edd7853feacef4a7f18ae681b01, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\3D\Axis.vb"

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

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Plot3D

    Public Module AxisDraw

        ''' <summary>
        ''' 绘制3D空间之中的xyz坐标轴
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="data"></param>
        ''' <param name="camera"></param>
        ''' <param name="font"></param>
        ''' <param name="axisStroke$"></param>
        <Extension>
        Public Sub DrawAxis(ByRef g As IGraphics,
                            data As Point3D(),
                            camera As Camera,
                            font As Font,
                            Optional axisStroke$ = Stroke.AxisStroke)

            Dim x = data.ToArray(Function(o) o.X)
            Dim y = data.ToArray(Function(o) o.Y)
            Dim z = data.ToArray(Function(o) o.Z)

            Call g.DrawAxis(camera,
                            font,
                            x:=New DoubleRange(x.Min, x.Max),
                            y:=New DoubleRange(y.Min, y.Max),
                            z:=New DoubleRange(z.Min, z.Max),
                            axisStroke:=axisStroke)
        End Sub

        <Extension>
        Public Sub DrawAxis(ByRef g As IGraphics,
                            camera As Camera,
                            font As Font,
                            x As DoubleRange,
                            y As DoubleRange,
                            z As DoubleRange,
                            Optional axisStroke$ = Stroke.AxisStroke)

            Dim pen As Pen = Stroke.TryParse(axisStroke).GDIObject
            Dim axis As New Axis With {
                .x1 = x.Min,
                .x2 = x.Max,
                .y1 = y.Min,
                .y2 = y.Max,
                .z1 = z.Min,
                .z2 = z.Max,
                .penX = pen,
                .penY = pen,
                .penZ = pen
            }

            Call g.DrawAxis(camera, font, axis)
        End Sub

        <Extension>
        Public Sub DrawAxis(ByRef g As IGraphics, camera As Camera, font As Font, axis As Axis)
            With camera
                Dim a As New Point3D(axis.x1, axis.y1, axis.z1)
                Dim b As New Point3D(axis.x1, axis.y2, axis.z1)
                Dim c As New Point3D(axis.x2, axis.y1, axis.z1)
                Dim d As New Point3D(axis.x2, axis.y2, axis.z1)
                Dim a1 As New Point3D(axis.x1, axis.y1, axis.z2)
                Dim b1 As New Point3D(axis.x1, axis.y2, axis.z2)
                Dim c1 As New Point3D(axis.x2, axis.y1, axis.z2)
                Dim d1 As New Point3D(axis.x2, axis.y2, axis.z2)

                Call g.DrawLine(axis.penX, .Project(.Rotate(a)).PointXY(.screen), .Project(.Rotate(b)).PointXY(.screen))
                Call g.DrawLine(axis.penY, .Project(.Rotate(a)).PointXY(.screen), .Project(.Rotate(c)).PointXY(.screen))
                Call g.DrawLine(axis.penZ, .Project(.Rotate(b)).PointXY(.screen), .Project(.Rotate(d)).PointXY(.screen))
                Call g.DrawLine(axis.penZ, .Project(.Rotate(c)).PointXY(.screen), .Project(.Rotate(d)).PointXY(.screen))

                Call g.DrawLine(axis.penX, .Project(.Rotate(a1)).PointXY(.screen), .Project(.Rotate(b1)).PointXY(.screen))
                Call g.DrawLine(axis.penY, .Project(.Rotate(a1)).PointXY(.screen), .Project(.Rotate(c1)).PointXY(.screen))
                Call g.DrawLine(axis.penZ, .Project(.Rotate(b1)).PointXY(.screen), .Project(.Rotate(d1)).PointXY(.screen))
                Call g.DrawLine(axis.penZ, .Project(.Rotate(c1)).PointXY(.screen), .Project(.Rotate(d1)).PointXY(.screen))

                Call g.DrawLine(axis.penX, .Project(.Rotate(a)).PointXY(.screen), .Project(.Rotate(a1)).PointXY(.screen))
                Call g.DrawLine(axis.penY, .Project(.Rotate(b)).PointXY(.screen), .Project(.Rotate(b1)).PointXY(.screen))
                Call g.DrawLine(axis.penZ, .Project(.Rotate(c)).PointXY(.screen), .Project(.Rotate(c1)).PointXY(.screen))
                Call g.DrawLine(axis.penZ, .Project(.Rotate(d)).PointXY(.screen), .Project(.Rotate(d1)).PointXY(.screen))
            End With
        End Sub
    End Module

    Public Structure Axis

        Dim x1, x2 As Single
        Dim y1, y2 As Single
        Dim z1, z2 As Single

        Dim penX, penY, penZ As Pen

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
