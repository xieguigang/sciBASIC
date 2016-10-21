Imports Microsoft.VisualBasic.Mathematical.LP
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main()
        Call Test()
        Call Test2()
    End Sub

    Sub Test()
        Dim min As New ObjectiveFunction With {
            .type = OptimizationType.MIN,
            .xyz = {-2, -3, -4},
            .Z = 1
        }
        Dim matrix = {
            New Equation With {
                .xyz = {3, 2, 1},
                .c = 10
            },
            New Equation With {
                .xyz = {2, 5, 3},
                .c = 15
            }
        }
        Dim result#() = min.Solve(matrix)

        ' x=y=z=0,s=10,t=15
        Call result.GetJson.__DEBUG_ECHO

        Pause()
    End Sub

    Public Sub Test2()
        Dim min As New ObjectiveFunction With {
            .type = OptimizationType.MAX,
            .xyz = {6, 5, 4},
            .Z = 1
        }
        Dim matrix = {
            New Equation With {.xyz = {2, 1, 1}, .c = 180},
            New Equation With {.xyz = {1, 3, 2}, .c = 300},
            New Equation With {.xyz = {2, 1, 2}, .c = 240}
        }
        Dim result#() = min.Solve(matrix)

        ' {708.0, 48.0, 84.0, 0.0, 0.0, 0.0, 60.0, 0.0}
        Call result.GetJson.__DEBUG_ECHO

        Pause()
    End Sub
End Module
