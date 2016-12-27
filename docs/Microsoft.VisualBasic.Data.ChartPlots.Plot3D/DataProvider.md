# DataProvider
_namespace: [Microsoft.VisualBasic.Data.ChartPlots.Plot3D](./index.md)_

Data provider



### Methods

#### Evaluate
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Plot3D.DataProvider.Evaluate(System.Func{System.Double,System.Double,System.Double},Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,System.Single,System.Single,System.Boolean,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.Data.csv.DocumentStream.DataSet})
```
Data provider base on the two variable.(这个函数可以同时为3D绘图或者ScatterHeatmap提供绘图数据)

|Parameter Name|Remarks|
|--------------|-------|
|f|``z = f(x,y)``|
|x|x取值范围|
|y|y取值范围|
|xsteps!|-|
|ysteps!|-|
|matrix|假若想要获取得到原始的矩阵数据，这个列表对象还需要实例化之后再传递进来|


_returns: Populate data by x steps.(即每一次输出的一组数据的X都是相同的)_

#### Grid
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Plot3D.DataProvider.Grid(System.Func{System.Double,System.Double,System.Double},Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,System.String,System.Single,System.Single)
```
Grid generator for function plot

|Parameter Name|Remarks|
|--------------|-------|
|f|-|
|x|-|
|y|-|
|xsteps!|-|
|ysteps!|-|
|mapNameZ|-|


#### Surface
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Plot3D.DataProvider.Surface(System.Func{System.Double,System.Double,System.},Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,System.Single,System.Single,System.Boolean,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.Data.csv.DocumentStream.EntityObject})
```
生成函数计算结果的三维表面

|Parameter Name|Remarks|
|--------------|-------|
|f|-|
|x|-|
|y|-|
|xsteps!|-|
|ysteps!|-|
|parallel|-|
|matrix|-|



