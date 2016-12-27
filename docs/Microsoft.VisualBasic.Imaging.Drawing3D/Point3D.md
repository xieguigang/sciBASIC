# Point3D
_namespace: [Microsoft.VisualBasic.Imaging.Drawing3D](./index.md)_

Defines the Point3D class that represents points in 3D space.
 Developed by leonelmachava <leonelmachava@gmail.com>
 http://codentronix.com

 Copyright (c) 2011 Leonel Machava



### Methods

#### Project
```csharp
Microsoft.VisualBasic.Imaging.Drawing3D.Point3D.Project(System.Int32,System.Int32,System.Int32,System.Single,System.Drawing.PointF)
```
Project the 3D point to the 2D screen. By using the projection result, 
 just read the property @``P:Microsoft.VisualBasic.Imaging.Drawing3D.Point3D.PointXY(System.Drawing.Size)``.
 (将3D投影为2D，所以只需要取结果之中的@``P:Microsoft.VisualBasic.Imaging.Drawing3D.Point3D.X``和@``P:Microsoft.VisualBasic.Imaging.Drawing3D.Point3D.Y``就行了)

|Parameter Name|Remarks|
|--------------|-------|
|viewWidth|-|
|viewHeight|-|
|fov|256默认值|
|viewDistance|-|


#### RotateX
```csharp
Microsoft.VisualBasic.Imaging.Drawing3D.Point3D.RotateX(System.Single)
```


|Parameter Name|Remarks|
|--------------|-------|
|angle|Degree.(度，函数里面会自动转换为三角函数所需要的弧度的)|



