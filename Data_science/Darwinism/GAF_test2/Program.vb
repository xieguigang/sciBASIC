Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Linq
Imports System.Drawing
Imports Microsoft.VisualBasic.Mathematical.Plots
Imports Microsoft.VisualBasic.Imaging

Module Program

    Sub Main()
        Call ODEsSolverTest()
    End Sub

    Public Sub ODEsSolverTest()
        Dim model As New Kinetics_of_influenza_A_virus_infection_in_humans
        Dim result As ODEsOut = model.Solve(10000, 0, 10)

        Call result.DataFrame("#TIME") _
            .Save("./Kinetics_of_influenza_A_virus_infection_in_humans.csv", Encodings.ASCII)

        Dim sT = result.y("T").x.SeqIterator.ToArray(Function(i) New PointF(result.x(i), +i))
        Dim sI = result.y("I").x.SeqIterator.ToArray(Function(i) New PointF(result.x(i), +i))
        Dim sV = result.y("V").x.SeqIterator.ToArray(Function(i) New PointF(result.x(i), +i))

        Call {
            Scatter.FromPoints(sT, "red", "Susceptible Cells"),
            Scatter.FromPoints(sI, "lime", "Infected Cells")
        }.Plot(fill:=False) _
         .SaveAs("./Kinetics_of_influenza_A_virus_infection_in_humans-TI.png")
        Call {
            Scatter.FromPoints(sV, "skyblue", "Virus Load")
        }.Plot(fill:=False) _
         .SaveAs("./Kinetics_of_influenza_A_virus_infection_in_humans-V.png")
    End Sub
End Module
