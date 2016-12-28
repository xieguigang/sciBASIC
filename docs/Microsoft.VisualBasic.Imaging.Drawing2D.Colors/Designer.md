# Designer
_namespace: [Microsoft.VisualBasic.Imaging.Drawing2D.Colors](./index.md)_

Generate color sequence



### Methods

#### Colors
```csharp
Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.Colors(System.Drawing.Color[],System.Int32,System.Int32)
```
**@``M:Microsoft.VisualBasic.Imaging.ColorCube.GetColorSequence(System.Drawing.Color,System.Drawing.Color,System.Int32,System.Int32)``**
 
 Some useful color tables for images and tools to handle them.
 Several color scales useful for image plots: a pleasing rainbow style color table patterned after 
 that used in Matlab by Tim Hoar and also some simple color interpolation schemes between two or 
 more colors. There is also a function that converts between colors and a real valued vector.

|Parameter Name|Remarks|
|--------------|-------|
|col|A list of colors (names or hex values) to interpolate|
|n%|Number of color levels. The setting n=64 is the orignal definition.|
|alpha%|
 The transparency of the color – 255 is opaque and 0 is transparent. This is useful for 
 overlays of color and still being able to view the graphics that is covered.
 |


_returns: 
 A vector giving the colors in a hexadecimal format, two extra hex digits are added for the alpha channel.
 _

#### CubicSpline
```csharp
Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.CubicSpline(System.Collections.Generic.IEnumerable{System.Drawing.Color},System.Int32,System.Int32)
```
这个函数并不会计算alpha的值

|Parameter Name|Remarks|
|--------------|-------|
|colors|-|
|n|所期望的颜色的数量|


#### FromNames
```csharp
Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.FromNames(System.String[],System.Int32)
```
相对于@``M:Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.GetColors(System.String)``函数而言，这个函数是返回非连续的颜色谱，假若数量不足，会重新使用开头的起始颜色连续填充

|Parameter Name|Remarks|
|--------------|-------|
|colors$|-|
|n%|-|


#### FromSchema
```csharp
Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.FromSchema(System.String,System.Int32)
```
@``M:Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.FromSchema(System.String,System.Int32)``和@``M:Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.FromNames(System.String[],System.Int32)``适用于函数绘图之类需要区分数据系列的颜色谱的生成

|Parameter Name|Remarks|
|--------------|-------|
|term$|-|
|n%|-|


#### GetBrushes
```csharp
Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.GetBrushes(System.String,System.Int32,System.Int32)
```
``New @``T:System.Drawing.SolidBrush``(@``M:Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.GetColors(System.String,System.Int32,System.Int32)``)``

|Parameter Name|Remarks|
|--------------|-------|
|term$|-|
|n%|-|
|alpha%|-|


#### GetColors
```csharp
Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.GetColors(System.String,System.Int32,System.Int32)
```
这个函数是获取得到一个连续的颜色谱

|Parameter Name|Remarks|
|--------------|-------|
|term$|-|
|n%|-|
|alpha%|-|



### Properties

#### AvailableInterpolates
{ 
 "Color [PapayaWhip]": [
 {
 "knownColor": 93,
 "name": null,
 "state": 1,
 "value": 0
 },
 {
 "knownColor": 119,
 "name": null,
 "state": 1,
 "value": 0
 },
 {
 "knownColor": 30,
 "name": null,
 "state": 1,
 "value": 0
 },
 {
 "knownColor": 165,
 "name": null,
 "state": 1,
 "value": 0
 },
 {
 "knownColor": 81,
 "name": null,
 "state": 1,
 "value": [rest of string was truncated]";.
