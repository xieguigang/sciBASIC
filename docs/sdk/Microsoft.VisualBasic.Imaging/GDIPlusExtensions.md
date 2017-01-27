# GDIPlusExtensions
_namespace: [Microsoft.VisualBasic.Imaging](./index.md)_

GDI+



### Methods

#### Area
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.Area(System.Drawing.Rectangle)
```
这个方形区域的面积

|Parameter Name|Remarks|
|--------------|-------|
|rect|-|


#### CorpBlank
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.CorpBlank(System.Drawing.Image,System.Int32,System.Drawing.Color)
```
确定边界，然后进行剪裁

|Parameter Name|Remarks|
|--------------|-------|
|res|-|
|margin|-|


#### CreateGDIDevice
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.CreateGDIDevice(System.Drawing.Size,System.Drawing.Color,System.String)
```
创建一个GDI+的绘图设备

|Parameter Name|Remarks|
|--------------|-------|
|r|-|
|filled|默认的背景填充颜色为白色|


#### EntireImage
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.EntireImage(System.Drawing.Image)
```
返回整个图像的区域

|Parameter Name|Remarks|
|--------------|-------|
|img|-|


#### GdiFromImage
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.GdiFromImage(System.Drawing.Image,System.String)
```
无需处理图像数据，这个函数已经自动克隆了该对象，不会影响到原来的对象

|Parameter Name|Remarks|
|--------------|-------|
|res|-|


#### GDIPlusDeviceHandleFromImageFile
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.GDIPlusDeviceHandleFromImageFile(System.String)
```
从指定的文件之中加载GDI+设备的句柄

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### GetRawStream
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.GetRawStream(System.Drawing.Image)
```
将图片对象转换为原始的字节流

|Parameter Name|Remarks|
|--------------|-------|
|image|-|


#### ImageAddFrame
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.ImageAddFrame(Microsoft.VisualBasic.Imaging.GDIPlusDeviceHandle,System.Drawing.Pen,System.Int32)
```
Adding a frame box to the target image source.(为图像添加边框)

|Parameter Name|Remarks|
|--------------|-------|
|Handle|-|
|pen|Default pen width is 1px and with color @``P:System.Drawing.Color.Black``.(默认的绘图笔为黑色的1个像素的边框)|


#### ImageCrop
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.ImageCrop(System.Drawing.Image,System.Drawing.Point,System.Drawing.Size)
```
图片剪裁小方块区域

|Parameter Name|Remarks|
|--------------|-------|
|pos|左上角的坐标位置|
|size|剪裁的区域的大小|


#### LoadImage
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.LoadImage(System.String)
```
Load image from a file and then close the file handle.
 (使用@``M:System.Drawing.Image.FromFile(System.String)``函数在加载完成图像到Dispose这段之间内都不会释放文件句柄，
 则使用这个函数则没有这个问题，在图片加载之后会立即释放掉文件句柄)

|Parameter Name|Remarks|
|--------------|-------|
|path|-|


#### MeasureString
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.MeasureString(System.String,System.Drawing.Font,System.Single,System.Single)
```
Measures the specified string when drawn with the specified System.Drawing.Font.

|Parameter Name|Remarks|
|--------------|-------|
|s|String to measure.|
|Font|System.Drawing.Font that defines the text format of the string.|
|XScaleSize|-|
|YScaleSize|-|


_returns: This method returns a System.Drawing.SizeF structure that represents the size,
 in the units specified by the System.Drawing.Graphics.PageUnit property, of the
 string specified by the text parameter as drawn with the font parameter.
 _

#### TrimRoundAvatar
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.TrimRoundAvatar(System.Drawing.Image,System.Int32)
```
图片剪裁为圆形的头像

|Parameter Name|Remarks|
|--------------|-------|
|resAvatar|要求为正方形或者近似正方形|
|OutSize|-|


#### Vignette
```csharp
Microsoft.VisualBasic.Imaging.GDIPlusExtensions.Vignette(System.Drawing.Image,System.Int32,System.Int32,System.Drawing.Color)
```
羽化

|Parameter Name|Remarks|
|--------------|-------|
|Image|-|
|y1|-|
|y2|-|



