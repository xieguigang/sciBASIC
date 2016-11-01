# Genetic Algorithm

> Installing VisualBasic for data science via nuget:
> ```
> PM> Install-Package VB_AppFramework -Pre
> ```
> and then add reference to these dll modules:
> + Microsoft.VisualBasic.Data.Csv.dll
> + Microsoft.VisualBasic.DataMining.Framework.dll
> + Microsoft.VisualBasic.Mathematical.dll
> + Microsoft.VisualBasic.Mathematical.ODEsSolver.dll
> + Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a.dll
> + Microsoft.VisualBasic.Data.Bootstrapping.dll

### RK4 ODEs solver in VisualBasic

![](./ODE_Trapezoidal.png)

```
y(tm+1) =y(tm)  + (k1 + 2k2 +2k3 + k4) / 6 * h     (1)
k1 =f( y(tm), tm)                                  (2)
k2 =f( y(tm)  + k1 / 2 * h, tm + h / 2)            (3)
k3 =f( y(tm)  + k2 / 2 * h, tm + h / 2)            (4)
k4 =f( y(tm) + h * k3, tm + h)                     (5)
```

```vbnet
''' <summary>
''' RK4 ODEs solver
''' </summary>
''' <param name="dxn">The x initial value.(x初值)</param>
''' <param name="dyn">The y initial value.(初值y(n))</param>
''' <param name="dh">Steps delta.(步长)</param>
''' <param name="dynext">
''' Returns the y(n+1) result from this parameter.(下一步的值y(n+1))
''' </param>
Private Sub __rungeKutta(dxn As Double,
                         ByRef dyn As Vector,
                         dh As Double,
                         ByRef dynext As Vector)
    Call ODEs(dxn, dyn, K1)                             ' 求解K1
    Call ODEs(dxn + dh / 2, dyn + dh / 2 * K1, K2)      ' 求解K2
    Call ODEs(dxn + dh / 2, dyn + dh / 2 * K2, K3)      ' 求解K3
    Call ODEs(dxn + dh, dyn + dh * K3, K4)              ' 求解K4

    dynext = dyn + (K1 + K2 + K3 + K4) * dh / 6.0  ' 求解下一步的值y(n+1)
End Sub
```

#### RK4 solver Code Usage

1. Inherits the abstract ODEs model: ``Microsoft.VisualBasic.Mathematical.Calculus.ODEs``
2. Declaring of the y variables: ``Dim <y_name> As var``
3. Other parameter just declared as constant or normal fields: ``Const param# = value``
4. Specific the ``y0`` by using inline value assign: ``var = value``
5. Then at last, you can using ``ODEs.Solve(Integer, Double, Double, Boolean) As Microsoft.VisualBasic.Mathematical.Calculus.ODEsOut`` to solve your equations.

Here represents a simple example of construct an ODEs model in VisualBasic:

```vbnet
''' <summary>
''' ##### Kinetics of influenza A virus infection in humans
'''
''' > **DOI** 10.3390/v7102875
''' </summary>
''' <remarks>假设为实验观测数据</remarks>
Public Class Kinetics_of_influenza_A_virus_infection_in_humans : Inherits ODEs

    Dim T As var
    Dim I As var
    Dim V As var

    Const p# = 3 * 10 ^ -2
    Const c# = 2
    Const beta# = 8.8 * 10 ^ -6
    Const delta# = 2.6

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        dy(T) = -beta * T * V
        dy(I) = beta * T * V - delta * I
        dy(V) = p * I - c * V
    End Sub

    Protected Overrides Function y0() As var()
        Return {
            V = 1.4 * 10 ^ -2,
            T = 4 * 10 ^ 8,
            I = 0
        }
    End Function
End Class
```

Running the solver and plotting the numerics result of your equations:

```vbnet
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
```

![](./Kinetics_of_influenza_A_virus_infection_in_humans-TI.png)
![](./Kinetics_of_influenza_A_virus_infection_in_humans-V.png)

### GAF Core

### GAF Parallel computing

Enable the GAF parallel computing is super easy, just needs specific the Parallel property its value to ``TRUE``, And then before the fitness sorts, A parallel Linq will be call to boost the entire ODEs fitness evaluation process.

```vbnet
Population(Of chr).Parallel As Boolean

LQuery = From x As NamedValue(Of chr)
         In source.AsParallel
         Let fit As T = GA._fitnessFunc.Calculate(x.x)
         Select New NamedValue(Of T) With {
             .Name = x.Name,
             .x = fit
         }
```

## Testing

##### Problem & Goal

![](./U.png)
![](./I.png)
![](./V.png)

> Kinetics of Influenza A Virus Infection in Humans. **DOI: 10.1128/JVI.01623-05**

##### ODEs Model

We want to estimates the kinetics parameters (p, c, beta and delta) in the equations using GAF method, so that we just define a ODEs model and leaves the parameter blank or assign any value, wait for the estimates, and here is the code example:

```vbnet
Public Class Kinetics_of_influenza_A_virus_infection_in_humans_Model : Inherits GAF.Model

    Dim T As var
    Dim I As var
    Dim V As var

    Dim p As Double = Integer.MaxValue
    Dim c As Double = Integer.MaxValue
    Dim beta As Double = Integer.MaxValue
    Dim delta As Double = Integer.MaxValue

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        dy(T) = -beta * T * V
        dy(I) = beta * T * V - delta * I
        dy(V) = p * I - c * V
    End Sub
End Class
```

##### Observation Data Example

For this testing demo, I using the exists model output as the biological experiment observation reference for the GAF estimates' fitness calculation. And using this method to creates a fake experiment data:

```vbnet
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
```

###### Preprocessing

##### GAF Estimates

```vbnet
Dim prints As List(Of outPrint) = Nothing
Dim estimates As var() = observations _
    .Fitting(Of Kinetics_of_influenza_A_virus_infection_in_humans_Model)(
    x#:=observations.First.Description.LoadObject(Of Double()),
    popSize:=1000,
    outPrint:=prints)

Call prints _
    .SaveTo("./Kinetics_of_influenza_A_virus_infection_in_humans-iterations.csv")

Dim result = MonteCarlo.Model.RunTest(
    GetType(Kinetics_of_influenza_A_virus_infection_in_humans_Model),
    observations.y0,
    estimates,
    10000, 0, 10)

Call result.DataFrame("#TIME") _
    .Save("./Kinetics_of_influenza_A_virus_infection_in_humans-GAF_estimates.csv", Encodings.ASCII)
```

## Testing On Linux and Super Computer
This demo has been tested successfully on a Dell 40 CPU core server running CentOS 7 and China TianHe 1 Super Computer.