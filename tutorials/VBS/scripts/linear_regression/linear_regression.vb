#include "Microsoft.VisualBasic.Data.Bootstrapping.Fittings.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Drawing.dll"

imports Microsoft.VisualBasic.Data
imports Microsoft.VisualBasic.Data.Bootstrapping
imports Microsoft.VisualBasic.Data.Framework
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

' ---------------------------------------------------------------------------
' Linear regression demo
'
'   dataset -> unified 2D table (NumericTable) -> LinearFit
'    -> prediction / residual written back to label columns
'    -> plot -> export csv
'
' The regression input convention is: X = all feature columns of the table,
' y = the label column named by the y parameter
' ---------------------------------------------------------------------------

' ---------------------------------------------------------------------------
' 1. Build a noisy linear dataset: y = 2x + 3 + noise
'
'    The noise is a deterministic sawtooth function so that every run produces
'    exactly the same fitting result
' ---------------------------------------------------------------------------
dim n = 100
dim features As Double()() = New Double(n - 1)() {}
dim y(n - 1) as double

for i = 0 to n - 1
    dim x = i * 0.1
    dim noise = ((i mod 7) - 3) * 0.1

    features(i) = New Double() {x}
    y(i) = 2.0 * x + 3.0 + noise
next

dim table = NumericTable.FromRows(Nothing, features, New String() {"x"})

call table.SetLabel("y", y)

call console.WriteLine($"dataset: {table.nsamples} samples x {table.nfeatures} feature")

' ---------------------------------------------------------------------------
' 2. Build the linear regression model
'
'    LinearFit returns a FitResult model object (slope/intercept/R2/RMSE/
'    residuals and so on are all carried on the model object)
' ---------------------------------------------------------------------------
dim model = table.LinearFit(y := "y")

call console.WriteLine($"linear fit  : y = {model.Slope} * x + {model.Intercept}")
call console.WriteLine($"R2 = {model.R_square}, adjust R2 = {model.AdjustR_square}, RMSE = {model.RMSE}")

' ---------------------------------------------------------------------------
' 3. Use a quadratic polynomial regression for comparison
' ---------------------------------------------------------------------------
dim quad = table.PolyFit(poly_n := 2)

call console.WriteLine($"poly fit(2) : R2 = {quad.R_square}, RMSE = {quad.RMSE}")

' ---------------------------------------------------------------------------
' 4. Write the prediction and the residual back into the label matrix of the table
'
'    SetPrediction does not modify the source table; it returns a new table with
'    the prediction columns written in
' ---------------------------------------------------------------------------
dim result = table.SetPrediction(model, withResidual := True)

call console.WriteLine($"prediction labels: {String.Join(", ", result.labelNames)}")
call console.WriteLine($"first row: y = {result.GetLabel("y")(0)}, prediction = {result.GetLabel("prediction")(0)}, residual = {result.GetLabel("residual")(0)}")

' ---------------------------------------------------------------------------
' 5. Draw the observed scatter points together with the fitted line
' ---------------------------------------------------------------------------
dim lineSize = 50
dim lineX(lineSize - 1) as double
dim lineY(lineSize - 1) as double
dim total = table.nsamples + lineSize
dim xs(total - 1) as double
dim ys(total - 1) as double
dim class_id(total - 1) as string

for i = 0 to lineSize - 1
    lineX(i) = i * 0.2
    lineY(i) = model.GetY(lineX(i))
next

for i = 0 to table.nsamples - 1
    xs(i) = table.Feature("x")(i)
    ys(i) = y(i)
    class_id(i) = "observed"
next

for i = 0 to lineSize - 1
    xs(table.nsamples + i) = lineX(i)
    ys(table.nsamples + i) = lineY(i)
    class_id(table.nsamples + i) = "fitted"
next

call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "Linear regression of a synthetic dataset"
    plt.SubTitle = $"y = {model.Slope} * x + {model.Intercept}, R2 = {model.R_square}"
    plt.XLabel = "x"
    plt.YLabel = "y"
    plt.Plot(DataSerials(xs, ys, class_id).tolist())
    plt.SavePng(here("linear-regression.png"), 300)
End Using

' ---------------------------------------------------------------------------
' 6. Export the result table
'    (row names + x feature column + label:y / label:prediction / label:residual)
' ---------------------------------------------------------------------------
call result.WriteCsv(here("linear-regression.csv"))

call console.WriteLine("done: linear-regression.png")
call console.WriteLine("done: linear-regression.csv")
