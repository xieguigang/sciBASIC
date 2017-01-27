# ColorExtensions
_namespace: [Microsoft.VisualBasic.Imaging](./index.md)_





### Methods

#### __getColorHash
```csharp
Microsoft.VisualBasic.Imaging.ColorExtensions.__getColorHash
```
Reads all of the color property from @``T:System.Drawing.Color`` and then creates the color dictionary based on the property name.

#### ARGBExpression
```csharp
Microsoft.VisualBasic.Imaging.ColorExtensions.ARGBExpression(System.Drawing.Color)
```
``rgb(a,r,g,b)``

|Parameter Name|Remarks|
|--------------|-------|
|c|-|


#### Equals
```csharp
Microsoft.VisualBasic.Imaging.ColorExtensions.Equals(System.Drawing.Color,System.Drawing.Color)
```
分别比较A,R,G,B这些属性值来判断这样个颜色对象值是否相等

|Parameter Name|Remarks|
|--------------|-------|
|a|-|
|b|-|


#### IsNullOrEmpty
```csharp
Microsoft.VisualBasic.Imaging.ColorExtensions.IsNullOrEmpty(System.Drawing.Color)
```
Determine that the target color value is a empty variable.(判断目标颜色值是否为空值)

#### RGBExpression
```csharp
Microsoft.VisualBasic.Imaging.ColorExtensions.RGBExpression(System.Drawing.Color)
```
``rgb(r,g,b)``

|Parameter Name|Remarks|
|--------------|-------|
|c|-|


#### ToColor
```csharp
Microsoft.VisualBasic.Imaging.ColorExtensions.ToColor(System.String,System.Drawing.Color,System.Boolean)
```
@``T:System.Drawing.Color``.Name, rgb(a,r,g,b)

|Parameter Name|Remarks|
|--------------|-------|
|str|颜色表达式或者名称|


#### TranslateColor
```csharp
Microsoft.VisualBasic.Imaging.ColorExtensions.TranslateColor(System.String)
```
这个函数会尝试用不同的模式来解析颜色表达式

|Parameter Name|Remarks|
|--------------|-------|
|exp$|-|



### Properties

#### __allDotNETPrefixColors
Key都是小写的
#### AllDotNetPrefixColors
Gets all of the known name color from the Color object its shared property.
#### ChartColors
经过人工筛选的颜色，不会出现过白或者过黑，过度相似的情况
#### RGB_EXPRESSION
Regex expression for parsing the rgb(a,r,g,b) expression of the color.(解析颜色表达式里面的RGB的正则表达式)
