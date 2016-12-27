# Heatmap
_namespace: [Microsoft.VisualBasic.Data.ChartPlots](./index.md)_





### Methods

#### __plotInterval
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Heatmap.__plotInterval(System.Action{System.Drawing.Graphics,Microsoft.VisualBasic.Imaging.Drawing2D.GraphicsRegion,Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.Collections.Generic.Dictionary{System.String,System.Double}}[],Microsoft.VisualBasic.Language.Value{System.Single},System.Drawing.Font,System.Single,System.Collections.Generic.Dictionary{System.Double,System.Int32},Microsoft.VisualBasic.Language.Value{System.Single},System.Drawing.Color[]},Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.Collections.Generic.Dictionary{System.String,System.Double}}[],System.Drawing.Color[],System.Int32,System.String,System.Drawing.Size,System.Drawing.Size,System.String,System.String,System.String,System.Drawing.Font,System.Double,System.Double,System.String,System.Drawing.Font)
```
一些共同的绘图元素过程

#### CorrelatesNormalized
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Heatmap.CorrelatesNormalized(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Data.csv.DocumentStream.DataSet},Microsoft.VisualBasic.Mathematical.Correlations.Correlations.ICorrelation)
```
相比于@``M:Microsoft.VisualBasic.Data.ChartPlots.Heatmap.LoadDataSet(System.String,System.String,System.Boolean,Microsoft.VisualBasic.Mathematical.Correlations.Correlations.ICorrelation)``函数，这个函数处理的是没有经过归一化处理的原始数据

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|correlation|假若这个参数为空，则默认使用@``M:Microsoft.VisualBasic.Mathematical.Correlations.Correlations.GetPearson(System.Double[],System.Double[])``|


#### DrawString
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Heatmap.DrawString(System.Drawing.Graphics,System.String,System.Drawing.Font,System.Drawing.Brush,System.Single,System.Single,System.Single)
```
绘制按照任意角度旋转的文本

|Parameter Name|Remarks|
|--------------|-------|
|g|-|
|text|-|
|font|-|
|brush|-|
|x!|-|
|y!|-|
|angle!|-|


#### LoadDataSet
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Heatmap.LoadDataSet(System.String,System.String,System.Boolean,Microsoft.VisualBasic.Mathematical.Correlations.Correlations.ICorrelation)
```
(这个函数是直接加在已经计算好了的相关度数据).假若使用这个直接加载数据来进行heatmap的绘制，
 请先要确保数据集之中的所有数据都是经过归一化的，假若没有归一化，则确保函数参数
 **`normalization`**的值为真

|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|uidMap$|-|
|normalization|是否对输入的数据集进行归一化处理？|
|correlation|
 默认为@``M:Microsoft.VisualBasic.Mathematical.Correlations.Correlations.GetPearson(System.Double[],System.Double[])``方法
 |


#### Plot
```csharp
Microsoft.VisualBasic.Data.ChartPlots.Heatmap.Plot(System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.ComponentModel.DataSourceModel.NamedValue{System.Collections.Generic.Dictionary{System.String,System.Double}}},System.Drawing.Color[],System.Int32,System.String,Microsoft.VisualBasic.Data.ChartPlots.Heatmap.ReorderProvider,System.Drawing.Size,System.Drawing.Size,System.String,System.String,System.String,System.Drawing.Font,System.Double,System.Double,System.String,System.Drawing.Font,System.Boolean,System.Boolean,System.Drawing.Font)
```
可以用来表示任意变量之间的相关度

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|customColors|
 可以使用这一组颜色来手动自定义heatmap的颜色，也可以使用**`mapName`**来获取内置的颜色谱
 |
|mapLevels%|-|
|mapName$|The color map name. @``T:Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer``|
|kmeans|Reorder datasets by using kmeans clustering|
|size|-|
|margin|-|
|bg$|-|



