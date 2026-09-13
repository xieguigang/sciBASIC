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
' 线性回归的 demo
'
'   数据集 → 统一二维表(NumericTable) → LinearFit 拟合 → 预测/残差写回标签列
'     → 绘图 → 导出 csv
'
' 其中回归算法的输入约定为：X = 表的全部特征列，y = y 参数指定的标签列
' ---------------------------------------------------------------------------

' ---------------------------------------------------------------------------
' 1. 构造一个带噪声的线性数据集：y = 2x + 3 + noise
'
'    噪声使用确定性的锯齿函数，保证每次运行的拟合结果完全一致
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
' 2. 线性回归建模
'
'    LinearFit 返回 FitResult 模型对象（斜率/截距/R2/RMSE/残差等都在模型对象上）
' ---------------------------------------------------------------------------
dim model = table.LinearFit(y := "y")

call console.WriteLine($"linear fit  : y = {model.Slope} * x + {model.Intercept}")
call console.WriteLine($"R2 = {model.R_square}, adjust R2 = {model.AdjustR_square}, RMSE = {model.RMSE}")

' ---------------------------------------------------------------------------
' 3. 用二次多项式回归做对比
' ---------------------------------------------------------------------------
dim quad = table.PolyFit(poly_n := 2)

call console.WriteLine($"poly fit(2) : R2 = {quad.R_square}, RMSE = {quad.RMSE}")

' ---------------------------------------------------------------------------
' 4. 把预测值与残差写回到表的标签矩阵之中
'
'    SetPrediction 不会修改源表，而是返回写入预测列之后的新表
' ---------------------------------------------------------------------------
dim result = table.SetPrediction(model, withResidual := True)

call console.WriteLine($"prediction labels: {String.Join(", ", result.labelNames)}")
call console.WriteLine($"first row: y = {result.GetLabel("y")(0)}, prediction = {result.GetLabel("prediction")(0)}, residual = {result.GetLabel("residual")(0)}")

' ---------------------------------------------------------------------------
' 5. 绘制观测散点 + 拟合直线
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
    plt.SavePng("Z:/linear-regression.png", 300)
End Using

' ---------------------------------------------------------------------------
' 6. 导出结果表（行名 + x 特征列 + label:y / label:prediction / label:residual）
' ---------------------------------------------------------------------------
call result.WriteCsv("Z:/linear-regression.csv")

call console.WriteLine("done: Z:/linear-regression.png")
call console.WriteLine("done: Z:/linear-regression.csv")
