Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Program



    Sub Main()

        'Call cl({210, 42}, {539, 72}) ' 265???
        'Call cl({0, 0}, {100, -5})
        'Call cl({100, -5}, {0, 0})

        'Call cl({100, 0}, {-100, 0})  ' X 轴， 0度
        'Call cl({0, 100}, {0, -100})   ' Y 轴，  90度
        'Call cl({100, 100}, {0, 0})  ' 45度

        'Call cl({-100, 100}, {0, 0}) '135
        'Call cl({-100, -100}, {0, 0}) '180+45

        'Call cl({100, -100}, {0, 0}) '360-45

        'Call cl({100, 100}, {-100, -100})
        'Call cl({-100, -100}, {100, 100})

        'Call g(100)

        'Call repl({0, 10}, {0, -10})   ' Y 轴，  90度
        'Call repl({100, 0}, {-100, 0})  ' X 轴  0度

        'Call reverse(New Force(100, 0))
        'Call reverse(New Force(100, PI / 3))


        Call add(New Force(1000, 1 / 2 * PI / 2), New Force(100, PI + 1 / 2 * PI / 2))  ' 45  225  -> 45

        Call add(New Force(100, 0), New Force(100, 1 / 2 * PI))  ' 45

        Call add(New Force(100, 0), New Force(100, PI)) ' 0
        Call add(New Force(100, 0), New Force(100, PI * 2)) ' 0

        Call add(New Force(100, 0), New Force(100, PI + 1 / 2 * PI)) ' 270+45

        Pause()

    End Sub


    Sub cl(a As Vector, b As Vector)
        Dim m1 As New MassPoint With {.Charge = 1, .Point = a}
        Dim m2 As New MassPoint With {.Charge = 1, .Point = b}
        Dim f = Math.CoulombsLaw(m1, m2)

        Call $"Coulombs force between the {m1.Point.ToString} and {m2.Point.ToString} is {f.ToString}".__DEBUG_ECHO
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
