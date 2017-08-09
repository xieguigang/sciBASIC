#Region "Microsoft.VisualBasic::8d44253e477180976932ecc12e030747, ..\sciBASIC#\Data_science\Mathematica\Math\MathApp\Testing\Module1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.LeastSquares
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
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
                .Value = y1.FitedYlist
            },
            New NamedValue(Of Double()) With {
                .Name = "y-polyfit-2",
                .Value = ypoly2.FitedYlist
            },
            New NamedValue(Of Double()) With {
                .Name = "y-polyfit-3",
                .Value = ypoly3.FitedYlist
            },
            New NamedValue(Of Double()) With {
                .Name = "y-polyfit-4",
                .Value = ypoly4.FitedYlist
            },
            New NamedValue(Of Double()) With {
                .Name = "y-polyfit-5",
                .Value = ypoly5.FitedYlist
            }
        }

        Call y1.GetJson.SaveTo("./y1.json")
        Call ypoly2.GetJson.SaveTo("./ypoly2.json")
        Call ypoly3.GetJson.SaveTo("./ypoly3.json")
        Call ypoly4.GetJson.SaveTo("./ypoly4.json")
        Call ypoly5.GetJson.SaveTo("./ypoly5.json")

        Call output.SaveTo("./output.csv")
    End Sub

    Sub Main()
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
