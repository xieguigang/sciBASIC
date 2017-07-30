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


            For Each line In {
                New Line(200, 200, 450, 450),
                New Line(300, 532, 1026, 663),
                New Line(1200, 635, 1999, 99)
            }

                Dim cor = line.ParallelShift(-150)
                cor.Stroke.Color = Color.Red
                cor.Stroke.Width = 5

                Call line.Draw(g)
                Call cor.Draw(g)
            Next

            Call g.Save("./test.png", ImageFormats.Png)
        End Using



        Pause()
    End Sub
End Module
