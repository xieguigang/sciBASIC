# VisualBasic Data Graphics System

### Color Sequence

+ **[Microsoft.VisualBasic.Imaging.ColorMap](../Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/ColorMap.vb)**
+ **[Microsoft.VisualBasic.Imaging.ColorMap](../Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/)**.**[ColorMapsExtensions](../Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/ColorMap.vb)**.``ColorSequence(Microsoft.VisualBasic.Imaging.ColorMapsExtensions.MapsFunc, Microsoft.VisualBasic.Imaging.ColorMap, Integer) As System.Drawing.Color()``


> ![](../etc/scales.png)

From left to right, the map names are:

```vbnet
''' <summary>
''' Spring
''' </summary>
Public Const PatternSpring As String = "Spring"
''' <summary>
''' Summer
''' </summary>
Public Const PatternSummer As String = "Summer"
''' <summary>
''' Autumn
''' </summary>
Public Const PatternAutumn As String = "Autumn"
''' <summary>
''' Winter
''' </summary>
Public Const PatternWinter As String = "Winter"
''' <summary>
''' Jet
''' </summary>
Public Const PatternJet As String = "Jet"
''' <summary>
''' Gray
''' </summary>
Public Const PatternGray As String = "Gray"
''' <summary>
''' Hot
''' </summary>
Public Const PatternHot As String = "Hot"
''' <summary>
''' Cool
''' </summary>
Public Const PatternCool As String = "Cool"
```

###### Usage

```vbnet
Imports Microsoft.VisualBasic.Imaging

Dim maps As New ColorMap(mapLevels)
Dim clSequence As Color() = ColorSequence(maps.GetMaps("Jet"), maps).Reverse.ToArray
```
