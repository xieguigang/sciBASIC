Imports Microsoft.VisualBasic.Imaging.Physics
Imports P3 = Microsoft.VisualBasic.Imaging.Physics.Vector3

Module Program
    Sub Main()
        Dim box = New P3(200, 200, 200)
        Dim engine As New FluidEngine3D(1500, box, 25)

        ' a deterministic shake disturbance
        engine.DisturbAccel = New P3(120, 0, 60)

        Dim rnd As New System.Random(123)
        Dim nanSeen As Boolean = False
        Dim maxAbs As Double = 0

        For stp As Integer = 1 To 600
            engine.RunSimulationStep()

            For Each p In engine.Entity
                If Double.IsNaN(p.Position.x) OrElse Double.IsNaN(p.Position.y) OrElse Double.IsNaN(p.Position.z) Then
                    nanSeen = True
                End If
                maxAbs = System.Math.Max(maxAbs, System.Math.Abs(p.Position.x))
                maxAbs = System.Math.Max(maxAbs, System.Math.Abs(p.Position.y))
                maxAbs = System.Math.Max(maxAbs, System.Math.Abs(p.Position.z))
            Next

            If stp = 1 OrElse stp = 300 OrElse stp = 600 Then
                Dim s = engine.Entity
                Dim avgV = 0.0
                For Each p In s : avgV += p.Velocity.Magnitude : Next
                avgV /= s.Count
                Console.WriteLine($"step {stp}: avgSpeed={avgV:F2} disturb={engine.DisturbAccel.Magnitude:F2} maxAbsPos={maxAbs:F1}")
            End If
        Next

        Console.WriteLine("NaN seen: " & nanSeen)
        Console.WriteLine("box size: " & box.ToString)
        ' verify all particles remain inside the container
        Dim outside As Integer = 0
        For Each p In engine.Entity
            If p.Position.x < -1 OrElse p.Position.x > box.x + 1 OrElse
               p.Position.y < -1 OrElse p.Position.y > box.y + 1 OrElse
               p.Position.z < -1 OrElse p.Position.z > box.z + 1 Then
                outside += 1
            End If
        Next
        Console.WriteLine("particles outside box: " & outside)
    End Sub
End Module
