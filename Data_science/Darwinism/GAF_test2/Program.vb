#Region "Microsoft.VisualBasic::e5f8ae72c2ae98d959e791c85f72ce6d, ..\sciBASIC#\Data_science\Darwinism\GAF_test2\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.Protocol
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv.DocumentExtensions
Imports Microsoft.VisualBasic.Data.csv.Extensions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Mathematical.Interpolation
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Module Program

    Sub Main()
        Call ODEsSolverTest()
        Call BuildFakeObservationForTest()
        Call GAF_estimates()

        Pause()

        Call App.Exit()
    End Sub

    Public Sub ODEsSolverTest()
        Dim model As New Kinetics_of_influenza_A_virus_infection_in_humans
        Dim result As ODEsOut = model.Solve(10000, 0, 10)

        Call result.DataFrame("#TIME") _
            .Save("./Kinetics_of_influenza_A_virus_infection_in_humans.csv", Encodings.ASCII)

        Dim sT = result.y("T").Value.SeqIterator.ToArray(Function(i) New PointF(result.x(i), +i))
        Dim sI = result.y("I").Value.SeqIterator.ToArray(Function(i) New PointF(result.x(i), +i))
        Dim sV = result.y("V").Value.SeqIterator.ToArray(Function(i) New PointF(result.x(i), +i))

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
            Let sample As Double() = y.Value _
                .Split(sampleSize) _
                .ToArray(Function(block) block.Average)
            Select New NamedValue(Of Double()) With {
                .Name = y.Name,
                .Value = sample
            }

        Call samples.SaveTo(
            path:="./Kinetics_of_influenza_A_virus_infection_in_humans-fake-observation.csv",
            xlabels:=xlabels)
    End Sub

    Public Sub GAF_estimates()
        Dim samples = "./Kinetics_of_influenza_A_virus_infection_in_humans-fake-observation.csv".LoadData.ToDictionary
        Dim x As Double() = samples("X").Value

        Call samples.Remove("X")

        Dim observations As NamedValue(Of Double())() =
            LinqAPI.Exec(Of NamedValue(Of Double())) <=
 _
            From sample As NamedValue(Of Double())
            In samples.Values
            Let raw As PointF() = x _
                .SeqIterator _
                .ToArray(Function(xi) New PointF(+xi, y:=sample.Value(xi)))
            Let cubicInterplots = CubicSpline.RecalcSpline(raw, 50).ToArray
            Let newData As Double() = cubicInterplots _
                .ToArray(Function(pt) CDbl(pt.Y))
            Select New NamedValue(Of Double()) With {
                .Name = sample.Name,
                .Value = newData,
                .Description = cubicInterplots _
                    .ToArray(Function(pt) pt.X) _
                    .GetJson  ' just needs the x value for the test
            }

        Call observations.SaveTo("./Kinetics_of_influenza_A_virus_infection_in_humans-samples.csv")

        Dim prints As List(Of outPrint) = Nothing
        Dim estimates As var() = observations _
            .Fitting(Of Kinetics_of_influenza_A_virus_infection_in_humans_Model)(
            x#:=observations.First.Description.LoadObject(Of Double()),
            popSize:=500,
            outPrint:=prints)

        Call prints.SaveTo("./Kinetics_of_influenza_A_virus_infection_in_humans-iterations.csv")

        Dim result = MonteCarlo.Model.RunTest(
            GetType(Kinetics_of_influenza_A_virus_infection_in_humans_Model),
            observations.y0,
            estimates,
            10000, 0, 10)

        Call result.DataFrame("#TIME") _
            .Save("./Kinetics_of_influenza_A_virus_infection_in_humans-GAF_estimates.csv", Encodings.ASCII)
    End Sub
End Module
