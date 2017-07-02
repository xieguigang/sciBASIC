Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Scripting.Expressions

Module Program

    Sub Main()
        Call ODEScript()
        Call Draw()
        Call DrawColors()
    End Sub

    Sub ODEScript()
        Dim x, y, z As var
        Dim sigma# = 10
        Dim rho# = 28
        Dim beta# = 8 / 3
        Dim t = (a:=0, b:=120, dt:=0.005)

        Call Let$(list:=Function() {x = 1, y = 1.5, z = 1}) ' Using Let$() for create these variables
        Call {
            x = Function() sigma * (y - x),
            y = Function() x * (rho - z) - y,
            z = Function() x * y - beta * z
        }.Solve(dt:=t) _
         .DataFrame _
         .Save($"{App.HOME}/Lorenz_system.csv")
    End Sub

    Sub Draw()
        Dim camera As New Camera With {
            .angleX = 30,
            .angleY = 30,
            .angleZ = 30,
            .fov = 1000,
            .screen = New Size(2000, 2000),
            .ViewDistance = 50
        }
        Dim result = ODEsOut.LoadFromDataFrame($"{App.HOME}/Lorenz_system.csv")
        Dim vector As Point3D() = result.x _
            .Sequence _
            .Select(Function(i)
                        With result.y
                            Dim x = (!x)(i)
                            Dim y = (!y)(i)
                            Dim z = (!z)(i)

                            Return New Point3D(x, y, z)
                        End With
                    End Function) _
            .ToArray
        Dim points As Point() = vector _
            .OffSets(vector.Center) _
            .Rotate(camera) _
            .Projection(camera)

        Using g As Graphics2D = camera.CreateCanvas2D(bg:="blue")
            Call g.DrawLines(Pens.White, points)
            Call g.Save($"{App.HOME}/Lorenz_system.png", ImageFormats.Png)
        End Using
    End Sub

    Sub DrawColors()
        Dim camera As New Camera With {
            .angleX = 30,
            .angleY = 30,
            .angleZ = 30,
            .fov = 1000,
            .screen = New Size(2000, 2000),
            .ViewDistance = 50
        }
        Dim result = ODEsOut.LoadFromDataFrame($"{App.HOME}/Lorenz_system.csv")
        Dim vector As Point3D() = result.x _
            .Sequence _
            .Select(Function(i)
                        With result.y
                            Dim x = (!x)(i)
                            Dim y = (!y)(i)
                            Dim z = (!z)(i)

                            Return New Point3D(x, y, z)
                        End With
                    End Function) _
            .ToArray
        Dim points As Point() = vector _
            .OffSets(vector.Center) _
            .Rotate(camera) _
            .Projection(camera)
        Dim colors = Designer _
            .GetColors("Paired:c12", 120) _
            .Select(Function(c) New SolidBrush(c)) _
            .ToArray
        Dim index = vector _
            .Select(Function(p) CDbl(p.Z) + p.X) _
            .RangeTransform($"0,{colors.Length - 1}")

        Using g As Graphics2D = camera.CreateCanvas2D(bg:="white")
            Call g.FillCircles(
                points,
                fill:=Function(i, point)
                          Return colors(index(i))
                      End Function,
                radius:=4)
            Call g.Save($"{App.HOME}/Lorenz_system.Z-colors.png", ImageFormats.Png)
        End Using
    End Sub
End Module
