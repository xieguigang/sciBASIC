#Region "Microsoft.VisualBasic::6acd89e865e8c5c097141b42ae7f0647, ..\visualbasic_App\Data_science\Mathematical\MathApp\Testing\Module1.vb"

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
    End Sub

    Sub Main()

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

