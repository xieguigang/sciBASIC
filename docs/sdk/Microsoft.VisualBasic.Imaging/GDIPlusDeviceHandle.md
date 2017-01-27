# GDIPlusDeviceHandle
_namespace: [Microsoft.VisualBasic.Imaging](./index.md)_

GDI+ device handle for encapsulates a GDI+ drawing surface.(GDI+绘图设备句柄)



### Methods

#### CreateDevice
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusDeviceHandle.CreateDevice(System.Drawing.Size,System.Drawing.Color)
```


|Parameter Name|Remarks|
|--------------|-------|
|r|-|
|filled|所填充的颜色|


#### Dispose
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusDeviceHandle.Dispose
```
Releases all resources used by this System.Drawing.Graphics.

#### DrawBézier
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusDeviceHandle.DrawBézier(System.Drawing.Pen,System.Drawing.Point,System.Drawing.Point,System.Drawing.Point,System.Drawing.Point)
```
Draws a Bézier spline defined by four System.Drawing.Point structures.

|Parameter Name|Remarks|
|--------------|-------|
|pen|System.Drawing.Pen structure that determines the color, width, and style of the
 curve.|
|pt1|System.Drawing.Point structure that represents the starting point of the curve.|
|pt2|System.Drawing.Point structure that represents the first control point for the
 curve.|
|pt3|System.Drawing.Point structure that represents the second control point for the
 curve.|
|pt4|System.Drawing.Point structure that represents the ending point of the curve.|


#### DrawLine
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusDeviceHandle.DrawLine(System.Drawing.Pen,System.Drawing.Point,System.Drawing.Point)
```
Draws a line connecting two System.Drawing.Point structures.

|Parameter Name|Remarks|
|--------------|-------|
|pen|System.Drawing.Pen that determines the color, width, and style of the line.|
|pt1|System.Drawing.Point structure that represents the first point to connect.|
|pt2|System.Drawing.Point structure that represents the second point to connect.|


#### DrawString
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusDeviceHandle.DrawString(System.String,System.Drawing.Font,System.Drawing.Brush,System.Drawing.PointF)
```
Draws the specified text string at the specified location with the specified
 System.Drawing.Brush and System.Drawing.Font objects.

|Parameter Name|Remarks|
|--------------|-------|
|s|String to draw.|
|font|System.Drawing.Font that defines the text format of the string.|
|brush|System.Drawing.Brush that determines the color and texture of the drawn text.|
|point|System.Drawing.PointF structure that specifies the upper-left corner of the drawn
 text.|


#### MeasureString
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusDeviceHandle.MeasureString(System.String,System.Drawing.Font)
```
Measures the specified string when drawn with the specified System.Drawing.Font.

|Parameter Name|Remarks|
|--------------|-------|
|text|String to measure.|
|font|System.Drawing.Font that defines the text format of the string.|


_returns: This method returns a System.Drawing.SizeF structure that represents the size,
 in the units specified by the System.Drawing.Graphics.PageUnit property, of the
 string specified by the text parameter as drawn with the font parameter._

#### Save
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusDeviceHandle.Save(System.String,System.Drawing.Imaging.ImageFormat)
```
将GDI+设备之中的图像数据保存到指定的文件路径之中，默认的图像文件的格式为PNG格式

|Parameter Name|Remarks|
|--------------|-------|
|Path|-|
|Format|默认为png格式|



### Properties

#### Center
在图象上面的中心的位置点
#### CompositingMode
Gets a value that specifies how composited images are drawn to this System.Drawing.Graphics.
#### CompositingQuality
Gets or sets the rendering quality of composited images drawn to this System.Drawing.Graphics.
#### DpiX
Gets the horizontal resolution of this System.Drawing.Graphics.
#### Graphics
GDI+ device handle.(GDI+绘图设备句柄)
#### ImageResource
GDI+ device handle memory.(GDI+设备之中的图像数据)
#### InterpolationMode
Gets or sets the interpolation mode associated with this System.Drawing.Graphics.
#### Size
Gets the width and height, in pixels, of this image.(图像的大小)
