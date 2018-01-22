```vbnet
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.LeastSquares
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Serialization.JSON

Dim inits As Dictionary(Of NamedValue(Of Double())) = "./test_linearfit.csv" _
    .LoadData _
    .ToDictionary
Dim output As New List(Of NamedValue(Of Double()))(inits.Values)
Dim y1 = LinearFit(inits("X").x, inits("Y").x)
Dim ypoly2 = PolyFit(inits("X").x, inits("Y").x, 2)
Dim ypoly3 = PolyFit(inits("X").x, inits("Y").x, 3)
Dim ypoly4 = PolyFit(inits("X").x, inits("Y").x, 4)
Dim ypoly5 = PolyFit(inits("X").x, inits("Y").x, 5)

output += {
    New NamedValue(Of Double()) With {
        .Name = "y-linearfit",
        .x = y1.FitedYlist
    },
    New NamedValue(Of Double()) With {
        .Name = "y-polyfit-2",
        .x = ypoly2.FitedYlist
    },
    New NamedValue(Of Double()) With {
        .Name = "y-polyfit-3",
        .x = ypoly3.FitedYlist
    },
    New NamedValue(Of Double()) With {
        .Name = "y-polyfit-4",
        .x = ypoly4.FitedYlist
    },
    New NamedValue(Of Double()) With {
        .Name = "y-polyfit-5",
        .x = ypoly5.FitedYlist
    }
}

Call y1.GetJson.SaveTo("./y1.json")
Call ypoly2.GetJson.SaveTo("./ypoly2.json")
Call ypoly3.GetJson.SaveTo("./ypoly3.json")
Call ypoly4.GetJson.SaveTo("./ypoly4.json")
Call ypoly5.GetJson.SaveTo("./ypoly5.json")

Call output.SaveTo("./output.csv")
```

![](./demo.png)