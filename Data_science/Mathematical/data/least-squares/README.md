```vbnet
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.LeastSquares
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Serialization.JSON

Dim inits As Dictionary(Of NamedValue(Of Double())) = "./test_linearfit.csv".LoadData.ToDictionary
Dim y2 As New NamedValue(Of Double())("y-linearfit", LinearFit(inits("X").x, inits("Y").x).FitedYlist)
Dim ypoly2 As New NamedValue(Of Double())("y-polyfit-2", PolyFit(inits("X").x, inits("Y").x, 2).FitedYlist)
Dim ypoly3 As New NamedValue(Of Double())("y-polyfit-3", PolyFit(inits("X").x, inits("Y").x, 3).FitedYlist)
Dim ypoly4 As New NamedValue(Of Double())("y-polyfit-4", PolyFit(inits("X").x, inits("Y").x, 4).FitedYlist)
Dim ypoly5 As New NamedValue(Of Double())("y-polyfit-5", PolyFit(inits("X").x, inits("Y").x, 5).FitedYlist)
Dim output As New List(Of NamedValue(Of Double()))

Call output.AddRange(inits.Values)
Call output.Add(y2)
Call output.Add(ypoly2)
Call output.Add(ypoly3)
Call output.Add(ypoly4)
Call output.Add(ypoly5)

Call output.SaveTo("./output.csv")
```

![](./demo.png)