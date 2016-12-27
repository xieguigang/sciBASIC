# ColorCube
_namespace: [Microsoft.VisualBasic.Imaging](./index.md)_

Describes the RGB color space as a 3D cube with the origin at Black.

> 
>  http://social.technet.microsoft.com/wiki/contents/articles/20990.generate-color-sequences-using-rgb-color-cube-in-vb-net.aspx
>  


### Methods

#### Compare
```csharp
Microsoft.VisualBasic.Imaging.ColorCube.Compare(System.Drawing.Color,System.Drawing.Color)
```
Compares two colors according to their distance from the origin of the cube (black).

|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|target|-|


#### GetBrightness
```csharp
Microsoft.VisualBasic.Imaging.ColorCube.GetBrightness(System.Drawing.Color)
```
Returns an integer between 0 and 255 indicating the perceived brightness of the color.

|Parameter Name|Remarks|
|--------------|-------|
|target|A System.Drawing.Color instance.|


_returns: An integer indicating the brightness with 0 being dark and 255 being bright._
> 
>  Formula found using web search at:
>  http://www.nbdtech.com/Blog/archive/2008/04/27/Calculating-the-Perceived-Brightness-of-a-Color.aspx This link is external to TechNet Wiki. It will open in a new window.
>  with reference to : http://alienryderflex.com/hsp.html This link is external to TechNet Wiki. It will open in a new window.
>  Effectively the same as measuring a color's distance from black, but constrained to a 0-255 range.
>  

#### GetColorFrom
```csharp
Microsoft.VisualBasic.Imaging.ColorCube.GetColorFrom(System.Drawing.Color,System.Double,System.Double,System.Double,System.Boolean,System.Int32)
```
Gets a color from within the cube starting at the specified location and moving a given distance in the specified direction.

|Parameter Name|Remarks|
|--------------|-------|
|source|The source location within the cube from which to start moving.|
|azimuth|The side-to-side angle in degrees; 0 points toward red and 90 points toward blue.|
|elevation|The top-to-bottom angle in degrees; 0 is no green and 90 points toward full green.|
|distance|The distance to travel within the cube; the approximate distance from black to white is 500.|


_returns: The color within the cube at the given distance in the specified direction._

#### GetColorsAround
```csharp
Microsoft.VisualBasic.Imaging.ColorCube.GetColorsAround(System.Drawing.Color,System.Int32,System.Int32)
```
Creates an array of colors from a selection within a sphere around the specified color.

|Parameter Name|Remarks|
|--------------|-------|
|target|The color to select around.|
|distance|The radius of the selection sphere.|
|increment|The increment within the sphere at which a selection is taken; larger numbers result in smaller selection sets.|


_returns: An array of colors located around the specified color within the cube._

#### GetColorSequence
```csharp
Microsoft.VisualBasic.Imaging.ColorCube.GetColorSequence(System.Drawing.Color,System.Drawing.Color,System.Int32,System.Int32)
```
Creates an array of colors in a gradient sequence between two specified colors.

|Parameter Name|Remarks|
|--------------|-------|
|source|The starting color in the sequence.|
|target|The end color in the sequence.|
|increment|The increment between colors.|


_returns: A gradient array of colors._

#### GetColorSpectrum
```csharp
Microsoft.VisualBasic.Imaging.ColorCube.GetColorSpectrum(System.Int32)
```
Creates a rainbow array of colors by selecting from the edges of the cube in ROYGBIV order at the specified increment.

|Parameter Name|Remarks|
|--------------|-------|
|increment|The increment along the edges at which a selection is taken; larger numbers result in smaller selection sets.|


_returns: An array of colors in ROYGBIV order at the given increment._

#### GetDistance
```csharp
Microsoft.VisualBasic.Imaging.ColorCube.GetDistance(System.Drawing.Color,System.Drawing.Color)
```
Gets the distance between two colors within the cube.

|Parameter Name|Remarks|
|--------------|-------|
|source|The source color in the cube.|
|target|The target color in the cube.|


_returns: The distance between the source and target colors._

#### GetHSL
```csharp
Microsoft.VisualBasic.Imaging.ColorCube.GetHSL(System.Drawing.Color)
```
Converts a RGB color into its Hue, Saturation, and Luminance (HSL) values.

|Parameter Name|Remarks|
|--------------|-------|
|rgb|The color to convert.|


_returns: The HSL representation of the color._
> 
>  Source algorithm found using web search at:
>  http://geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm This link is external to TechNet Wiki. It will open in a new window.
>  (Adapted to VB)
>  


### Properties

#### InvalidRange
Value must be between 0 and 90.
