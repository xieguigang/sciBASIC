# PieChart
_namespace: [Microsoft.VisualBasic.Data.ChartPlots](./index.md)_





### Methods

#### FromData
```csharp
Microsoft.VisualBasic.Data.ChartPlots.PieChart.FromData(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.Int32}},System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|data|每个标记的数量，函数会自动根据这些数量计算出百分比|
|schema|-|


#### FromPercentages
```csharp
Microsoft.VisualBasic.Data.ChartPlots.PieChart.FromPercentages(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.Double}},System.Drawing.Color[])
```


|Parameter Name|Remarks|
|--------------|-------|
|data|手工计算出来的百分比|
|colors|Default is using schema of @``P:Microsoft.VisualBasic.Imaging.Drawing2D.Colors.OfficeColorThemes.Office2016``|


#### Plot
```csharp
Microsoft.VisualBasic.Data.ChartPlots.PieChart.Plot(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.ChartPlots.Fractions},System.Drawing.Size,System.Drawing.Size,System.String,System.Boolean,Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes.Border,System.Single,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|size|-|
|margin|-|
|bg|-|
|legend|-|
|legendBorder|-|
|minRadius|
 当这个参数值大于0的时候，除了扇形的面积会不同外，半径也会不同，这个参数指的是最小的半径
 |
|reorder|
 是否按照数据比例重新对数据排序？
 +  0 : 不需要
 +  1 : 从小到大排序
 + -1 : 从大到小排序 
 |



