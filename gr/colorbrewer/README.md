## sciBASIC#, the color artist

+ source code on github: [Microsoft.VisualBasic.Imaging](https://github.com/xieguigang/sciBASIC/tree/master/gr/Microsoft.VisualBasic.Imaging)
+ install via nuget:
  ```bash
  # https://www.nuget.org/packages/sciBASIC#
  PM> Install-Package sciBASIC -Pre
  ```

### Cubic Spline Of Colors

And here is the core algorithm function of the cubic spline calculation:

```vbnet
Public Shared Sub CalcNaturalCubic(values As IList(Of Single), cubics As ICollection(Of Cubic))
    Dim num As Integer = values.Count - 1

    Dim gamma As Double() = New Double(num) {}
    Dim delta As Double() = New Double(num) {}
    Dim D As Double() = New Double(num) {}

    Dim i As Integer

    '    We solve the equation
    '
    '	 [2 1       ] [D[0]]   [3(x[1] - x[0])  ]
    '	 |1 4 1     | |D[1]|   |3(x[2] - x[0])  |
    '	 |  1 4 1   | | .  | = |      .         |
    '	 |    ..... | | .  |   |      .         |
    '	 |     1 4 1| | .  |   |3(x[n] - x[n-2])|
    '	 [       1 2] [D[n]]   [3(x[n] - x[n-1])]
    '
    '	 by using row operations to convert the matrix to upper triangular
    '	 and then back substitution.
    '    The D[i] are the derivatives at the knots.

    gamma(0) = 1.0F / 2.0F
    For i = 1 To num - 1
        gamma(i) = 1.0F / (4.0F - gamma(i - 1))
    Next
    gamma(num) = 1.0F / (2.0F - gamma(num - 1))

    Dim p0 As Single = values(0)
    Dim p1 As Single = values(1)

    delta(0) = 3.0F * (p1 - p0) * gamma(0)
    For i = 1 To num - 1
        p0 = values(i - 1)
        p1 = values(i + 1)
        delta(i) = (3.0F * (p1 - p0) - delta(i - 1)) * gamma(i)
    Next

    p0 = values(num - 1)
    p1 = values(num)

    delta(num) = (3.0F * (p1 - p0) - delta(num - 1)) * gamma(num)

    D(num) = delta(num)
    For i = num - 1 To 0 Step -1
        D(i) = delta(i) - gamma(i) * D(i + 1)
    Next

    'now compute the coefficients of the cubics
    cubics.Clear()

    For i = 0 To num - 1
        p0 = values(i)
        p1 = values(i + 1)

        cubics.Add(New Cubic(p0, D(i), 3 * (p1 - p0) - 2 * D(i) - D(i + 1), 2 * (p0 - p1) + D(i) + D(i + 1)))
    Next
End Sub
```

### The ColorBrewer

+ in d3js javascript: [d3-scale-chromatic](https://github.com/d3/d3-scale-chromatic)
+ in R language: [RColorBrewer](https://cran.r-project.org/web/packages/RColorBrewer/)
+ in VB.NET: [sciBASIC#](https://github.com/xieguigang/sciBASIC/tree/master/gr)
+ in Mathematica: [ColorBrewer](https://github.com/wanglongqi/ColorBrewer)
+ in matlab: [cbrSelector](https://github.com/DevinCharles/cbrSelector)
+ in java: [colorbrewer](https://github.com/rcsb/colorbrewer/)
+ in python: [colorbrewer-python](https://github.com/dsc/colorbrewer-python)
+ in perl: [Color::Brewer](https://metacpan.org/pod/Color::Brewer)
+ in C++: [ofxColorBrewer](https://github.com/chparsons/ofxColorBrewer)

### Microsoft Office Color Themes

### Application 1: GIS data renderer
### Application 2: Circos Gradient Histograme
### Application 3: Heatmap
### Application 4: Chart Color System in ``sciBASIC#``