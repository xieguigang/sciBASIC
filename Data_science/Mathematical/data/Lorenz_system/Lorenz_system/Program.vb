Imports Microsoft.VisualBasic.Mathematical.Calculus

Module Program

    Sub Main()

        Dim x, y, z As var
        Dim sigma#
        Dim rho#
        Dim beta#
        Dim t = (a:=0, b:=1, dt:=0.01)

        Call lapply(Function() {x = 1, y = 2, z = 3})
        Call {
            x = Function() sigma * (y - x),
            y = Function() x * (rho - z) - y,
            z = Function() x * y - beta * z
        }.Solve(t) _
         .DataFrame _
         .Save("./Lorenz_system.csv")

    End Sub
End Module
