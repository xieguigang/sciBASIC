#Region "Microsoft.VisualBasic::87a4d0e58d678ca46a41c0205cf00704, Data_science\Visualization\test\ChartingBase.Test\Module2.vb"

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

    '   Total Lines: 36
    '    Code Lines: 28 (77.78%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (22.22%)
    '     File Size: 953 B


    ' Module Module2
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: HeatmapScatter, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Imaging

Module Module2

    Sub New()

    End Sub

    Sub Main()
        Call HeatmapScatter()

    End Sub

    Sub HeatmapScatter()
        Dim data As New List(Of DataSet)
        Dim rnd As New Random

        For i As Integer = 0 To 1000
            data += New DataSet With {
                .ID = App.NextTempName,
                .Properties = New Dictionary(Of String, Double) From {
                    {"X", rnd.NextInteger(1000)},
                    {"Y", rnd.NextInteger(1000)},
                    {"Z", rnd.NextInteger(1000)}
                }
            }
        Next

        Call Scatter _
            .PlotHeatmap(data, valueField:="Z", legendTitle:="Heatmap(Z)") _
            .Save("x:\scatter.plotHeatmap.png")
    End Sub
End Module
