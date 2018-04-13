#Region "Microsoft.VisualBasic::5c34ffe4e8393a7f496a523d5a27f121, Data_science\Mathematica\Plot\test\PieChartTest.vb"

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

    ' Module PieChartTest
    ' 
    '     Sub: Main, radarTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Fractions
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Public Module PieChartTest

    Sub Main()

        Call radarTest()

        Dim rnd As New Random
        Dim data As New List(Of NamedValue(Of Integer))

        For i As Integer = 0 To 6
            data.Add(
                New NamedValue(Of Integer) With {
                     .Name = "block#" & i,
                     .Value = rnd.Next(300)
                })
        Next

        Call data _
            .Fractions(ColorBrewer.QualitativeSchemes.Accent8) _
            .Plot(legendAlt:=True) _
            .Save("./test_pie.png")
    End Sub

    Sub radarTest()
        Dim rnd As New Random
        Dim data As New List(Of NamedValue(Of Integer))

        For i As Integer = 0 To 6
            data.Add(
                New NamedValue(Of Integer) With {
                     .Name = "block#" & i,
                     .Value = rnd.Next(300)
                })
        Next

        Dim s = data _
            .Fractions(ColorBrewer.QualitativeSchemes.Accent8)

        Call RardarChart.Plot({New NamedValue(Of FractionData())("test1", s)}, spline:=200).Save("./test_radar.png")

        Pause()
    End Sub
End Module
