Imports Microsoft.VisualBasic.Imaging.Physics
Imports System.Math

Public Module Program

    Sub Main()

        Call add(New Force(100, 0), New Force(100, 1 / 2 * PI))

        Call add(New Force(100, 0), New Force(100, PI))
        Call add(New Force(100, 0), New Force(100, PI * 2))

        Call add(New Force(100, 0), New Force(100, PI + 1 / 2 * PI))

        Pause()

    End Sub

    Sub add(f1 As Force, f2 As Force)
        Call $"||{f1.ToString}|| + ||{f2.ToString}||  =>  ||{(f1 + f2).ToString}||".__DEBUG_ECHO
    End Sub
End Module
