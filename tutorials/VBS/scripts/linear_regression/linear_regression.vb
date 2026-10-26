#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ---------------------------------------------------------------------------
' Linear regression demo (standard library edition)
'
'   synthetic dataset -> NumericTable -> fit.linear / fit.poly
'    -> prediction + residual written back as label columns
'    -> plot.scatter -> stdio.write.csv -> report.render
'
' The script only uses the two standard libraries (stdlib / stdio): every
' analysis, plotting and reporting call below is a single standard api entry,
' so the very same logic runs on the NJS engine (see linear_regression.js).
' ---------------------------------------------------------------------------

' ---------------------------------------------------------------------------
' 1. Build a noisy linear dataset: y = 2x + 3 + noise
'
'    The noise is a deterministic sawtooth function so that every run produces
'    exactly the same fitting result
' ---------------------------------------------------------------------------
dim n = 100
dim features(n - 1)() as double
dim y(n - 1) as double

for i = 0 to n - 1
    dim x = i * 0.1
    dim noise = ((i mod 7) - 3) * 0.1

    features(i) = New Double() {x}
    y(i) = 2.0 * x + 3.0 + noise
next

dim dataset = table.fromRows(table.names("sample", n), features, New String() {"x"})
call table.setLabel(dataset, "y", y)

call console.WriteLine($"dataset: {dataset.nsamples} samples x {dataset.nfeatures} feature")

' ---------------------------------------------------------------------------
' 2. Fit a linear model and a quadratic model for comparison
' ---------------------------------------------------------------------------
dim model = fit.linear(dataset, "y")

call console.WriteLine($"linear fit  : y = {model.slope} * x + {model.intercept}")
call console.WriteLine($"R2 = {model.r2}, RMSE = {model.rmse}")

dim quad = fit.poly(dataset, 2, "y")
call console.WriteLine($"poly fit(2) : R2 = {quad.r2}, RMSE = {quad.rmse}")

' ---------------------------------------------------------------------------
' 3. Write the prediction and the residual back into the label matrix
' ---------------------------------------------------------------------------
dim result = fit.setPrediction(table.clone(dataset), model, withResidual:=True, y:="y")

call console.WriteLine($"prediction labels: {String.Join(", ", result.labelNames)}")
call console.WriteLine($"first row: y = {table.label(result, "y")(0)}, prediction = {table.label(result, "prediction")(0)}, residual = {table.label(result, "residual")(0)}")

' ---------------------------------------------------------------------------
' 4. Draw the observed scatter points together with the fitted line
'
'    plot.scatter groups the samples by the class id vector and colours every
'    group automatically, so the two series share one call
' ---------------------------------------------------------------------------
dim line_size = 50
dim total = dataset.nsamples + line_size
dim xs(total - 1) as double
dim ys(total - 1) as double
dim class_id(total - 1) as string
dim x_values = table.feature(dataset, "x")

for i = 0 to dataset.nsamples - 1
    xs(i) = x_values(i)
    ys(i) = y(i)
    class_id(i) = "observed"
next

for i = 0 to line_size - 1
    dim px = i * 0.2

    xs(dataset.nsamples + i) = px
    ys(dataset.nsamples + i) = model.GetY(px)
    class_id(dataset.nsamples + i) = "fitted"
next

dim figure = plot.scatter(
    xs, ys,
    classId:=class_id,
    title:="Linear regression of a synthetic dataset",
    subtitle:=$"y = {model.slope} * x + {model.intercept}, R2 = {model.r2}",
    xLabel:="x", yLabel:="y", size:="1200,800")

call console.WriteLine("plot: " & plot.save(figure, here("linear-regression.png")))

' ---------------------------------------------------------------------------
' 5. Export the result table and a formatted data report
'    (row names + x feature column + label:y / label:prediction / label:residual)
' ---------------------------------------------------------------------------
call console.WriteLine("csv : " & write.csv(result, here("linear-regression.csv")))

dim doc = report.document("Linear regression report", "sciBASIC standard library")
call doc.add(report.heading("1. Synthetic dataset"))
call doc.add(report.paragraph($"The dataset contains {dataset.nsamples} samples of one feature column x and the label y = 2x + 3 + noise."))
call doc.add(report.heading("2. Fitted models"))
call doc.add(report.table(New String() {"model", "R2", "RMSE"}, New String()() {
    New String() {"linear", model.r2.ToString("F6"), model.rmse.ToString("F6")},
    New String() {"poly(2)", quad.r2.ToString("F6"), quad.rmse.ToString("F6")}
}))
call doc.add(report.heading("3. Observation and fitted line"))
call doc.add(report.image(here("linear-regression.png"), "linear regression"))
call doc.add(report.heading("4. First 5 prediction rows"))
call doc.add(report.tableOf(table.slice(result, 0, 5)))

call console.WriteLine("md  : " & doc.save(here("linear-regression.md")))
call console.WriteLine("html: " & doc.save(here("linear-regression.html")))
call console.WriteLine("docx: " & doc.save(here("linear-regression.docx")))
call console.WriteLine("pdf : " & doc.save(here("linear-regression.pdf")))
call console.WriteLine("rtf : " & doc.save(here("linear-regression.rtf")))
call console.WriteLine("xps : " & doc.save(here("linear-regression.oxps")))

call console.WriteLine("done: linear-regression")
