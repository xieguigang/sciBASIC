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

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Mathematical

Module Module1

    Public Sub FittingTest()

        Dim result As Dictionary(Of NamedValue(Of Double())) = "G:\eeeeeeeeeeee.ConvertedToVBNet\test_linearfit.csv".LoadData.ToDictionary

        Dim out As FittedResult = Fit.linearFit(result("X").x.ToList, result("Y").x.ToList)

        Dim y2 As New NamedValue(Of Double())("y-linearfit", out.fitedYs.ToArray)
        Dim ypoly2 As New NamedValue(Of Double())("y-polyfit-2", Fit.polyfit(result("X").x.ToList, result("Y").x.ToList, 2).fitedYs.ToArray)
        Dim ypoly3 As New NamedValue(Of Double())("y-polyfit-3", Fit.polyfit(result("X").x.ToList, result("Y").x.ToList, 3).fitedYs.ToArray)
        Dim ypoly4 As New NamedValue(Of Double())("y-polyfit-4", Fit.polyfit(result("X").x.ToList, result("Y").x.ToList, 4).fitedYs.ToArray)
        Dim ypoly5 As New NamedValue(Of Double())("y-polyfit-5", Fit.polyfit(result("X").x.ToList, result("Y").x.ToList, 5).fitedYs.ToArray)

        Dim outpiu As New List(Of NamedValue(Of Double()))

        outpiu.AddRange(result.Values)
        outpiu.Add(y2)
        outpiu.Add(ypoly2)
        outpiu.Add(ypoly3)
        outpiu.Add(ypoly4)
        outpiu.Add(ypoly5)


        Call outpiu.SaveTo("./output.csv")
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

