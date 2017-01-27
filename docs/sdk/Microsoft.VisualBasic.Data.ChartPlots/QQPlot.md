# QQPlot
_namespace: [Microsoft.VisualBasic.Data.ChartPlots](./index.md)_

Q-Q plot(Quantile-Quantile Plot)



### Methods

#### Plot
```csharp
Microsoft.VisualBasic.Data.ChartPlots.QQPlot.Plot(System.Double[],System.Double[],System.Drawing.Size,System.Drawing.Size,System.String,System.String,System.String,System.Single,System.Int32,System.Double,System.Int32)
```
[2016-9-30 Currently this function is not working]
 
 The quantile-quantile (q-q) plot is a graphical technique for determining if two data sets 
 come from populations with a common distribution.
 
 A q-q plot Is a plot Of the quantiles Of the first data Set against the quantiles Of the 
 second data Set. By a quantile, we mean the fraction (Or percent) Of points below the given 
 value. That Is, the 0.3 (Or 30%) quantile Is the point at which 30% percent Of the data 
 fall below And 70% fall above that value.

 A 45-degree reference line Is also plotted. If the two sets come from a population with the 
 same distribution, the points should fall approximately along this reference line. The 
 greater the departure from this reference line, the greater the evidence for the conclusion 
 that the two data sets have come from populations with different distributions.

 The advantages Of the q-q plot are:

 + The sample sizes Do Not need To be equal.
 + Many distributional aspects can be simultaneously tested. For example, shifts In location, 
 shifts In scale, changes In symmetry, And the presence Of outliers can all be detected from 
 this plot. For example, If the two data sets come from populations whose distributions 
 differ only by a shift In location, the points should lie along a straight line that Is 
 displaced either up Or down from the 45-degree reference line.
 
 The q-q plot Is similar To a probability plot. For a probability plot, the quantiles For 
 one Of the data samples are replaced With the quantiles Of a theoretical distribution.

|Parameter Name|Remarks|
|--------------|-------|
|x|-|
|y|-|
|size|-|
|margin|-|
|bg$|-|
|ptSize!|-|



