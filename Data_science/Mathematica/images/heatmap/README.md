Plot a heatmap just super easy!

```vbnet
Dim data = LoadData("./Sample.csv", True)
Dim spcc = data.CorrelationMatrix(AddressOf Spearman)

Call HeatmapTable.Plot(spcc,) _
    .SaveAs("./Sample.SPCC.png")
Call Heatmap.Plot(spcc, mapLevels:=25) _
    .SaveAs("./Sample.heatmap.png")
```

![](./Sample.SPCC.png)
![](./Sample.heatmap.png)
