# ScatterHeatmap
_namespace: [Microsoft.VisualBasic.Data.ChartPlots](./index.md)_

和普通的heatmap相比，这里的坐标轴是连续的数值变量，而普通的heatmap，其坐标轴都是离散的分类变量



### Methods

#### __getData
```csharp
Microsoft.VisualBasic.Data.ChartPlots.ScatterHeatmap.__getData(System.Func{System.Double,System.Double,System.Double},System.Drawing.Size,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,System.Single@,System.Single@,System.Boolean,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.Data.csv.DocumentStream.DataSet}@,System.Int32)
```
返回去的数据是和**`size`**每一个像素点相对应的

|Parameter Name|Remarks|
|--------------|-------|
|fun|-|
|size|-|
|xrange|-|
|yrange|-|
|xsteps!|-|
|ysteps!|-|
|parallel|
 对于例如ODEs计算这类比较重度的计算，可以考虑在这里使用并行
 |


#### Plot
```csharp
Microsoft.VisualBasic.Data.ChartPlots.ScatterHeatmap.Plot(System.Func{System.Double,System.Double,System.Double},Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,Microsoft.VisualBasic.ComponentModel.Ranges.DoubleRange,System.String,System.Int32,System.String,System.Drawing.Size,System.Int32,System.String,System.Drawing.Font,System.Single,System.Single,System.Boolean,Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.Data.csv.DocumentStream.DataSet}@,System.Double,System.Double,System.String,System.String,System.Double)
```
steps步长值默认值为长度平分到每一个像素点

|Parameter Name|Remarks|
|--------------|-------|
|fun|-|
|xrange|-|
|yrange|-|
|colorMap$|
 Default using colorbrewer ``Spectral:c10`` schema.
 |
|size|3000, 2400|
|xsteps!|-|
|matrix|
 请注意：假若想要获取得到原始的矩阵数据，这个列表对象还需要实例化之后再传递进来，否则仍然会返回空集合
 |



