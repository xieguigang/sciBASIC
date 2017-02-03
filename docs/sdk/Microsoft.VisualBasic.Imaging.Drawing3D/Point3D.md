# Point3D
_namespace: [Microsoft.VisualBasic.Imaging.Drawing3D](./index.md)_

Defines the Point3D class that represents points in 3D space.
 Developed by leonelmachava <leonelmachava@gmail.com>
 http://codentronix.com

 Copyright (c) 2011 Leonel Machava



### Methods

#### Project
```csharp
Microsoft.VisualBasic.Imaging.Drawing3D.Point3D.Project(System.Single@,System.Single@,System.Single,System.Int32,System.Int32,System.Int32,System.Int32)
```
Project the 3D point to the 2D screen.

|Parameter Name|Remarks|
|--------------|-------|
|x!|-|
|y!|-|
|z!|Using for the painter algorithm.|
|viewWidth%|-|
|viewHeight%|-|
|fov%|-|
|viewDistance%|View distance to the model from the view window.|


#### RotateX
```csharp
Microsoft.VisualBasic.Imaging.Drawing3D.Point3D.RotateX(System.Single)
```


|Parameter Name|Remarks|
|--------------|-------|
|angle|Degree.(度，函数里面会自动转换为三角函数所需要的弧度的)|



### Properties

#### PointXY
Gets the projection 2D point result from this readonly property
