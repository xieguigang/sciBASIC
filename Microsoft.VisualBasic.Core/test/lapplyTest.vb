Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Module lapplyTest

    Dim list As NamedValue(Of Double())()

    Sub New()
        list = {
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {1, 1, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {2, 11, 711}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {3, 11, 611}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {4, 111, 151}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {5, 1111, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {6, 11, 114}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {7, 11, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {8, 12, 811}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {9, 1444, 311}}
        }
    End Sub

    Sub Main()
        Dim result = list.lapply(Of Double)(Function(a As NamedValue(Of Double()), x As Double, y As Double)
                                                Return a.Value(Scan0) + x + y * a.Value.Average
                                            End Function, 5, 6)

        Pause()
    End Sub

End Module
