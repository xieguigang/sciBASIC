# ScatterHeatmap
_namespace: [Microsoft.VisualBasic.Data.ChartPlots.Plot3D](./index.md)_





### Methods

#### GetPlotFunction
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Plot3D.ScatterHeatmap.GetPlotFunction(System.Func{System.Double,System.Double,System.},Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,System.Int32,System.Int32,System.String,System.String,System.Int32,System.String,System.Boolean,System.Collections.Generic.List{Microsoft.VisualBasic.Data.csv.DocumentStream.EntityObject},System.String,System.Drawing.Font,System.Boolean)
```
DEBUG模式之下会将网格给绘制出来，这个在Release模式之下是不会出现的。

|Parameter Name|Remarks|
|--------------|-------|
|f|-|
|xrange|-|
|yrange|-|
|xn%|-|
|yn%|-|
|legendTitle$|-|
|mapName$|-|
|mapLevels%|-|
|bg$|-|
|parallel|-|
|matrix|-|
|axisFont$|-|
|legendFont|-|
|showLegend|-|


#### Plot
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Plot3D.ScatterHeatmap.Plot(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.csv.DocumentStream.EntityObject},Microsoft.VisualBasic.Imaging.Drawing3D.Camera,System.String,System.String,System.Int32,System.String,System.String,System.Drawing.Font)
```
3D heatmap plot from matrix data

|Parameter Name|Remarks|
|--------------|-------|
|matrix|-|
|Camera|-|
|legendTitle$|-|
|mapName$|-|
|mapLevels%|-|
|bg$|-|
|axisFont$|-|
|legendFont|-|



