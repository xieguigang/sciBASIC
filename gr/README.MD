# VisualBasic Data Graphics System

### Color Sequence

+ ``[Microsoft.VisualBasic.Imaging.ColorMap](../Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/ColorMap.vb)``
+ ``Microsoft.VisualBasic.Imaging.ColorMapsExtensions.ColorSequence(Microsoft.VisualBasic.Imaging.ColorMapsExtensions.MapsFunc, Microsoft.VisualBasic.Imaging.ColorMap, Integer) As System.Drawing.Color()``


> ![](../etc/scales.png)

From left to right, the map names are:

```vbnet
''' <summary>
''' Autumn
''' </summary>
Public Const PatternAutumn As String = "Autumn"
''' <summary>
''' Cool
''' </summary>
Public Const PatternCool As String = "Cool"
''' <summary>
''' Gray
''' </summary>
Public Const PatternGray As String = "Gray"
''' <summary>
''' Hot
''' </summary>
Public Const PatternHot As String = "Hot"
''' <summary>
''' Jet
''' </summary>
Public Const PatternJet As String = "Jet"
''' <summary>
''' Spring
''' </summary>
Public Const PatternSpring As String = "Spring"
''' <summary>
''' Summer
''' </summary>
Public Const PatternSummer As String = "Summer"
''' <summary>
''' Winter
''' </summary>
Public Const PatternWinter As String = "Winter"
```

###### Usage

```vbnet
Imports Microsoft.VisualBasic.Imaging

Dim maps As New ColorMap(mapLevels)
Dim clSequence As Color() = ColorSequence(maps.GetMaps("Jet"), maps).Reverse.ToArray
```