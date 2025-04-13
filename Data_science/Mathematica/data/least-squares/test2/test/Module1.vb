#Region "Microsoft.VisualBasic::5b99be8ad1c9f75e72c72e060dddfa6a, sciBASIC#\Data_science\Mathematica\data\least-squares\test2\test\Module1.vb"

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

    '   Total Lines: 98
    '    Code Lines: 59
    ' Comment Lines: 14
    '   Blank Lines: 25
    '     File Size: 3.34 KB


    ' Module Module1
    ' 
    '     Sub: FittingTest, Main, Rtest2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.LeastSquares
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Public Sub FittingTest()
        Dim inits As Dictionary(Of NamedValue(Of Double())) = "./test_linearfit.csv" _
            .LoadData _
            .ToDictionary
        Dim output As New List(Of NamedValue(Of Double()))(inits.Values)
        Dim y1 = LinearFit(inits("X").Value, inits("Y").Value)
        Dim ypoly2 = PolyFit(inits("X").Value, inits("Y").Value, 2)
        Dim ypoly3 = PolyFit(inits("X").Value, inits("Y").Value, 3)
        Dim ypoly4 = PolyFit(inits("X").Value, inits("Y").Value, 4)
        Dim ypoly5 = PolyFit(inits("X").Value, inits("Y").Value, 5)

        output += {
            New NamedValue(Of Double()) With {
                .Name = "y-linearfit",
                .Value = y1.ErrorTest.Select(Function(p) p.Yfit).ToArray
            },
            New NamedValue(Of Double()) With {
                .Name = "y-polyfit-2",
                .Value = ypoly2.ErrorTest.Select(Function(p) p.Yfit).ToArray
            },
            New NamedValue(Of Double()) With {
                .Name = "y-polyfit-3",
                .Value = ypoly3.ErrorTest.Select(Function(p) p.Yfit).ToArray
            },
            New NamedValue(Of Double()) With {
                .Name = "y-polyfit-4",
                .Value = ypoly4.ErrorTest.Select(Function(p) p.Yfit).ToArray
            },
            New NamedValue(Of Double()) With {
                .Name = "y-polyfit-5",
                .Value = ypoly5.ErrorTest.Select(Function(p) p.Yfit).ToArray
            }
        }

        Call y1.GetJson.SaveTo("./y1.json")
        Call ypoly2.GetJson.SaveTo("./ypoly2.json")
        Call ypoly3.GetJson.SaveTo("./ypoly3.json")
        Call ypoly4.GetJson.SaveTo("./ypoly4.json")
        Call ypoly5.GetJson.SaveTo("./ypoly5.json")

        Call output.SaveTo("./output.csv")
    End Sub

    Sub Rtest2()

        Dim weight = {115.0, 117, 120, 123, 126, 129, 132, 135, 139, 142, 146, 150, 154, 159, 164}
        Dim height = {58.0, 59.0, 60.0, 61.25, 62.0, 63.5, 64.0, 65.0, 66.1, 67.1, 68.0, 69.0, 70.0, 71.8, 72.0}


        Dim linear = LeastSquares.LinearFit(height, weight)
        Dim poly = LeastSquares.PolyFit(height, weight, 2)

        Pause()
    End Sub

    Sub Main()


        Call Rtest2()

        Call FittingTest()
        'Dim rnd As New Randomizer
        'Dim list As New List(Of Double)

        'For i As Integer = 0 To 100
        '    list.Add(rnd.NextDouble)
        'Next

        'Call list.GetJson.__DEBUG_ECHO

        'Dim bytes As Byte() = New Byte(500) {}

        'Call rnd.NextBytes(bytes)

        'Pause()


        'Dim b = 10.Sequence.ToArray(Function(x) Distributions.Beta.beta(x, 10, 100))

        'b = Distributions.Beta.beta(Mathematical.Extensions.seq(0, 1, 0.01), 0.5, 0.5).ToArray

        'Call b.FlushAllLines("x:\dddd.csv")

        'Call b.GetJson.__DEBUG_ECHO

        'Pause()
    End Sub
End Module
