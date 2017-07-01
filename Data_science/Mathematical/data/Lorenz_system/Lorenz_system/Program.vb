Imports Microsoft.VisualBasic.Mathematical.Calculus

Module Program

    Sub Main()

        Dim x, y, z As var
        Dim sigma# = 10
        Dim rho# = 28
        Dim beta# = 8 / 3
        Dim t = (a:=0, b:=10, dt:=0.0001)

        Call Let$(list:=Function() {x = 1, y = 1, z = 1})
        Call {
            x = Function() sigma * (y - x),
            y = Function() x * (rho - z) - y,
            z = Function() x * y - beta * z
        }.Solve(dt:=t) _
         .DataFrame _
         .Save("./Lorenz_system.csv")

    End Sub
End Module
