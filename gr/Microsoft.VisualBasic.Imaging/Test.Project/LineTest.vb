Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Imaging

Module LineTest

    Sub Main()
        Using g = New Size(1600, 900).CreateGDIDevice
            Dim line As New Line(0, 0, 100, 0)
            Dim down = line.ParallelShift(20)


            Call line.Draw(g)
            Call down.Draw(g)

            line = New Line(0, 0, 0, 100)
            Dim left = line.ParallelShift(20)

            Call line.Draw(g)
            Call left.Draw(g)


            line = New Line(200, 200, 450, 450)
            Dim cor = line.ParallelShift(150)
            cor.Stroke.Color = Color.Red
            cor.Stroke.Width = 5

            Call line.Draw(g)
            Call cor.Draw(g)

            Call g.Save("./test.png", ImageFormats.Png)
        End Using



        Pause()
    End Sub
End Module
