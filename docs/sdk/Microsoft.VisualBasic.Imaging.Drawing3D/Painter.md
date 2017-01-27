# Painter
_namespace: [Microsoft.VisualBasic.Imaging.Drawing3D](./index.md)_

``PAINTERS ALGORITHM`` provider



### Methods

#### OrderProvider``1
```csharp
Microsoft.VisualBasic.Imaging.Drawing3D.Painter.OrderProvider``1(System.Collections.Generic.IEnumerable{``0},System.Func{``0,System.Double})
```
``PAINTERS ALGORITHM`` kernel

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|z|计算出z轴的平均数据|


#### SurfacePainter
```csharp
Microsoft.VisualBasic.Imaging.Drawing3D.Painter.SurfacePainter(System.Drawing.Graphics@,Microsoft.VisualBasic.Imaging.Drawing3D.Camera,System.Collections.Generic.IEnumerable{Microsoft.VisualBasic.Imaging.Drawing3D.Surface})
```
请注意，这个并没有rotate，只会利用camera进行project

|Parameter Name|Remarks|
|--------------|-------|
|canvas|-|
|camera|-|
|surfaces|-|



