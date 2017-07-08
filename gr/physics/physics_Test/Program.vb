Imports Microsoft.VisualBasic.Imaging.Physics
Imports System.Math
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Public Module Program



    Sub Main()

        Call g(100)

        Call repl({0, 10}, {0, -10})   ' Y 轴，  90度
        Call repl({100, 0}, {-100, 0})  ' X 轴  0度

        Call reverse(New Force(100, 0))
        Call reverse(New Force(100, PI / 3))


        Call add(New Force(100, 0), New Force(100, 1 / 2 * PI))

        Call add(New Force(100, 0), New Force(100, PI))
        Call add(New Force(100, 0), New Force(100, PI * 2))

        Call add(New Force(100, 0), New Force(100, PI + 1 / 2 * PI))

        Pause()

    End Sub

    Sub g(m#)
        Call $"gravity is {Math.Gravity(New MassPoint With {.Mass = m}).ToString }".__DEBUG_ECHO
    End Sub

    Sub repl(a As Vector, b As Vector)
        Call Math.RepulsiveForce(100, a, b).__DEBUG_ECHO
    End Sub

    Sub reverse(f As Force)
        Call $"ReverseOf({f.ToString}) is {-f}".__DEBUG_ECHO
    End Sub

    Sub add(f1 As Force, f2 As Force)
        Call $"||{f1.ToString}|| + ||{f2.ToString}||  =>  ||{(f1 + f2).ToString}||".__DEBUG_ECHO
    End Sub
End Module
