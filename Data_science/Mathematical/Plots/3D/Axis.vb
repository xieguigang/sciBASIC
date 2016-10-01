Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Plot3D

    Public Module AxisDraw

        <Extension>
        Public Sub DrawAxis(ByRef g As Graphics, data As Point3D(), camera As Camera, font As Font)
            Dim x = data.ToArray(Function(o) o.X)
            Dim y = data.ToArray(Function(o) o.Y)
            Dim z = data.ToArray(Function(o) o.Z)

            Call g.DrawAxis(camera,
                            font,
                            x:=New DoubleRange(x.Min, x.Max),
                            y:=New DoubleRange(y.Min, y.Max),
                            z:=New DoubleRange(z.Min, z.Max))
        End Sub

        <Extension>
        Public Sub DrawAxis(ByRef g As Graphics,
                            camera As Camera,
                            font As Font,
                            x As DoubleRange,
                            y As DoubleRange,
                            z As DoubleRange)

            Dim axis As New Axis With {
                .x1 = New Point3D With {.X = x.Min},
                .x2 = New Point3D With {.X = x.Max},
                .y1 = New Point3D With {.Y = y.Min},
                .y2 = New Point3D With {.Y = y.Max},
                .z1 = New Point3D With {.Z = z.Min},
                .z2 = New Point3D With {.Z = z.Max},
                .penX = Pens.Black,
                .penY = Pens.Black,
                .penZ = Pens.Black
            }

            Call g.DrawAxis(camera, font, axis)
        End Sub

        <Extension>
        Public Sub DrawAxis(ByRef g As Graphics, camera As Camera, font As Font, axis As Axis)
            With camera
                Call g.DrawLine(axis.penX, .Project(.Rotate(axis.x1)).PointXY, .Project(.Rotate(axis.x2)).PointXY)
                Call g.DrawLine(axis.penY, .Project(.Rotate(axis.y1)).PointXY, .Project(.Rotate(axis.y2)).PointXY)
                Call g.DrawLine(axis.penZ, .Project(.Rotate(axis.z1)).PointXY, .Project(.Rotate(axis.z2)).PointXY)
            End With
        End Sub
    End Module

    Public Structure Axis

        Dim x1, x2 As Point3D
        Dim y1, y2 As Point3D
        Dim z1, z2 As Point3D

        Dim penX, penY, penZ As Pen

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace