# Isometric

> Imports from this original java works: [**Isometric drawing library for Android**](https://github.com/FabianTerhorst/Isometric)

```vbnet
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Isometric
Imports IsometricView = Microsoft.VisualBasic.Imaging.Drawing3D.IsometricEngine

Dim isometricView As New IsometricView

' Add 3D models
' blablabla
Call isometricView.Add(...)

Using g As Graphics2D = New Size(width, height).CreateGDIDevice
    Call isometricView.Draw(g)
    Call g.ImageResource.SaveAs($"./{filename}.png")
End Using
```

### Drawing a simple cube

```vbnet
Call isometricView.add(
	New Prism(
		New Point3D(0, 0, 0),
		1, 1, 1
	),
	Color.FromArgb(33, 150, 243)
)
```

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotOne.png)

### Drawing multiple Shapes

There are 3 basic components: ``points``, ``paths`` and ``shapes``. A shape needs an origin point and 3 measurements for the x, y and z axes. The default Prism constructor is setting all measurements to 1.

```vbnet
isometricView.Add(new Prism(new Point3D(0, 0, 0)), Color.FromArgb(33, 150, 243))
isometricView.Add(new Prism(new Point3D(-1, 1, 0), 1, 2, 1), Color.FromArgb(33, 150, 243))
isometricView.Add(new Prism(new Point3D(1, -1, 0), 2, 1, 1), Color.FromArgb(33, 150, 243))
```

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotTwo.png?raw=true)

### Drawing multiple Paths

Paths are two dimensional. You can draw and color paths the same as shapes.

```vbnet
isometricView.add(new Prism(Math3D.ORIGIN, 3, 3, 1), Color.FromArgb(50, 60, 160))
isometricView.add(new Path3D({
    new Point3D(1, 1, 1),
    new Point3D(2, 1, 1),
    new Point3D(2, 2, 1),
    new Point3D(1, 2, 1)
}), Color.FromArgb(50, 160, 60))
```

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotPath.png?raw=true)

### The grid

Here you can see how the grid looks like. The blue grid is the xy-plane. The z-line is the z-axis.

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotGrid.png?raw=true)

### Supports complex structures

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotThree.png?raw=true)

### Translate

Traslate is translating an point, path or shape to the given x, y and z distance. Translate is returning a new point, path or shape.

```vbnet
Dim prism As New Prism(new Point3D(0, 0, 0))
isometricView.add(prism, Color.FromArgb(33, 150, 243))
isometricView.add(prism.translate(0, 0, 1.1), Color.FromArgb(33, 150, 243))
```

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotTranslate.png?raw=true)

### Scale

Scale is scaling an point, path or shape with the given x, y and z scaling factors. Scale is returning a new point, path or shape.

```vbnet
Dim blue = Color.FromArgb(50, 60, 160)
Dim red = Color.FromArgb(160, 60, 50)
Dim cube As new Prism(Math3D.ORIGIN)
isometricView.add(cube.scale(Math3D.ORIGIN, 3.0, 3.0, 0.5), red)
isometricView.add(cube
	.scale(Math3D.ORIGIN, 3.0, 3.0, 0.5)
	.translate(0, 0, 0.6), blue)
```

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotScale.png?raw=true)

### RotateZ

RotateZ is rotating an point, path or shape with the given angle in radians on the xy-plane (where an angle of 0 runs along the position x-axis). RotateZ is returning a new point, path or shape.

```vbnet
Dim blue = Color.FromArgb(50, 60, 160)
Dim red = Color.FromArgb(160, 60, 50)
Dim cube As new Prism(Math3D.ORIGIN, 3, 3, 1)
isometricView.add(cube, red);
isometricView.add(cube
	' /* (1.5, 1.5) is the center of the prism */
	.rotateZ(new Point3D(1.5, 1.5, 0), Math.PI / 12)
	.translate(0, 0, 1.1), blue)
```

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotRotateZ.png?raw=true)

### Shapes from Paths

The method ```Shape.extrude``` allows you to create a 3D model by popping out a 2D path along the z-axis.

```vbnet
Dim blue = Color.FromArgb(50, 60, 160)
Dim red = Color.FromArgb(160, 60, 50)

Call isometricView.add(new Prism(Math3D.ORIGIN, 3, 3, 1), blue)
Call isometricView.add(Shape.extrude(new Path3D({
	new Point3D(1, 1, 1),
	new Point3D(2, 1, 1),
	new Point3D(2, 3, 1)
}), 0.3), red)
```

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotExtrude.png?raw=true)

### Available Shapes

###### Cylinder

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotCylinder.png?raw=true)

###### Knot

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotKnot.png?raw=true)

###### Octahedron

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotOctahedron.png?raw=true)

###### Prism

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotPrism.png?raw=true)

###### Pyramid

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotPyramid.png?raw=true)

###### Stairs

![Image](./screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotStairs.png?raw=true)