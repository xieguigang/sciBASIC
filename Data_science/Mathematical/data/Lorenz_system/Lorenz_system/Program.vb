Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Mathematical.Calculus

Module Program

    Sub Main()
        Call ODEScript()
        Call Draw()
    End Sub

    Sub ODEScript()
        Dim x, y, z As var
        Dim sigma# = 10
        Dim rho# = 28
        Dim beta# = 8 / 3
        Dim t = (a:=0, b:=120, dt:=0.005)

        Call Let$(list:=Function() {x = 1, y = 1, z = 1})
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
End Module
