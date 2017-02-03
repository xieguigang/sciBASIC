# hBitmap
_namespace: [Microsoft.VisualBasic.Imaging](./index.md)_

线程不安全的图片数据对象



### Methods

#### FromImage
```csharp
Microsoft.VisualBasic.Imaging.hBitmap.FromImage(System.Drawing.Image)
```
这个函数会自动复制原始图片数据里面的东西的

|Parameter Name|Remarks|
|--------------|-------|
|res|-|


#### GetImage
```csharp
Microsoft.VisualBasic.Imaging.hBitmap.GetImage
```
Gets a copy of the original raw image value that which constructed this bitmap object class

#### GetIndex
```csharp
Microsoft.VisualBasic.Imaging.hBitmap.GetIndex(System.Int32,System.Int32)
```
返回第一个元素的位置

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|


_returns: B, G, R_

#### GetPixel
```csharp
Microsoft.VisualBasic.Imaging.hBitmap.GetPixel(System.Int32,System.Int32)
```
Gets the color of the specified pixel in this System.Drawing.Bitmap.

|Parameter Name|Remarks|
|--------------|-------|
|x|The x-coordinate of the pixel to retrieve.|
|y|The y-coordinate of the pixel to retrieve.|


_returns: A System.Drawing.Color structure that represents the color of the specified pixel._

#### SetPixel
```csharp
Microsoft.VisualBasic.Imaging.hBitmap.SetPixel(System.Int32,System.Int32,System.Drawing.Color)
```
Sets the color of the specified pixel in this System.Drawing.Bitmap.(这个函数线程不安全)

|Parameter Name|Remarks|
|--------------|-------|
|x|The x-coordinate of the pixel to set.|
|y|The y-coordinate of the pixel to set.|
|color|
 A System.Drawing.Color structure that represents the color to assign to the specified
 pixel.|



