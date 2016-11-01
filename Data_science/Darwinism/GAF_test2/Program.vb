Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Mathematical.Plots
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Data.csv.Extensions

Module Program

    Sub Main()
        Call ODEsSolverTest()
        Call BuildFakeObservationForTest()
        Call GAF_estimates()
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

    Public Sub BuildFakeObservationForTest()
        Dim result As ODEsOut = ODEsOut _
            .LoadFromDataFrame("./Kinetics_of_influenza_A_virus_infection_in_humans.csv")
        Dim sampleSize% = 100
        Dim xlabels#() = result.x.Split(sampleSize).ToArray(Function(block) block.Average)
        Dim samples As NamedValue(Of Double())() =
            LinqAPI.Exec(Of NamedValue(Of Double())) <=
 _
            From y As NamedValue(Of Double())
            In result.y.Values
            Let sample As Double() = y.x _
                .Split(sampleSize) _
                .ToArray(Function(block) block.Average)
            Select New NamedValue(Of Double()) With {
                .Name = y.Name,
                .x = sample
            }

        Call samples.SaveTo(
            path:="./Kinetics_of_influenza_A_virus_infection_in_humans-fake-observation.csv",
            xlabels:=xlabels)
    End Sub

    Public Sub GAF_estimates()

    End Sub
End Module
