Imports Microsoft.VisualBasic.Imaging.Physics
Imports System.Math

Public Module Program

    Sub Main()

        Dim f1 As New Force(100, 0)
        Dim f2 As New Force(100, 1 / 2 * PI)

        Call $"{f1.ToString} + {f2.ToString} => {(f1 + f2).ToString}".__DEBUG_ECHO

        Pause()

    End Sub
End Module
