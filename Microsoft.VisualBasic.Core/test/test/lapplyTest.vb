#Region "Microsoft.VisualBasic::95cadc0d9c1ac8ab5566911ac44427ab, Microsoft.VisualBasic.Core\test\test\lapplyTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 93
    '    Code Lines: 82
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 5.47 KB


    ' Module lapplyTest
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Main1, testApply, testSelectLinq
    ' 
    ' /********************************************************************************/

#End Region

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

    Sub Main1()

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
