Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes

Module LineTest

    Sub Main()
        Dim line As New Line(0, 0, 100, 0)
        Dim down = line.ParallelShift(20)

        line = New Line(0, 0, 0, 100)
        Dim left = line.ParallelShift(20)

        line = New Line(0, 0, 45, 45)
        Dim cor = line.ParallelShift(50)

        Pause()
    End Sub
End Module
