Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Module lapplyTest

    Dim list As NamedValue(Of Double())()

    Sub New()
        list = 5000.SeqRandom.Select(Function(i) {
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {1, 1, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {2, 11, 711}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {3, 11, 611}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {4, 111, 151}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {5, 1111, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {6, 11, 114}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {7, 11, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {8, 12, 811}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {9, 1444, 311}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {1, 1, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {2, 11, 711}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {3, 11, 611}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {4, 111, 151}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {5, 1111, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {6, 11, 114}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {7, 11, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {8, 12, 811}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {9, 1444, 311}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {1, 1, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {2, 11, 711}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {3, 11, 611}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {4, 111, 151}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {5, 1111, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {6, 11, 114}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {7, 11, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {8, 12, 811}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {9, 1444, 311}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {1, 1, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {2, 11, 711}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {3, 11, 611}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {4, 111, 151}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {5, 1111, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {6, 11, 114}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {7, 11, 11}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {8, 12, 811}},
            New NamedValue(Of Double()) With {.Name = App.NextTempName, .Value = {9, 1444, 311}}
        }).IteratesALL.ToArray
    End Sub

    Sub Main()

        Call testApply()
        Call testSelectLinq()
        Call testApply()
        Call testSelectLinq()
        Call testApply()
        Call testSelectLinq()
        Call testApply()
        Call testSelectLinq()
        Call testApply()
        Call testSelectLinq()
        Call testApply()
        Call testSelectLinq()

        Pause()
    End Sub

    Dim result As Dictionary(Of String, Double)

    Sub testApply()
        Call BENCHMARK(Sub()
                           result = list.lapply(Of Double)(Function(a As NamedValue(Of Double()), x As Double, y As Double)
                                                               Return a.Value(Scan0) + x + y * a.Value.Average
                                                           End Function, 5, 6)
                       End Sub)
    End Sub

    Sub testSelectLinq()
         Call BENCHMARK(Sub()
                           Dim x = 5
                            Dim y = 6

                            result = list.Select(Function(a As NamedValue(Of Double()))
                                                        Return   (key:= a.Name , val:= a.Value(Scan0) + x + y * a.Value.Average)
                                                    End Function) _
                                            .ToDictionary(Function(xx) xx.key ,  
                                                          Function (xx)
                                                              Return xx.val 
                                                          End Function)
                       End Sub)
    End Sub

End Module
