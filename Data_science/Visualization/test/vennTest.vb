#Region "Microsoft.VisualBasic::274e1f1d04f72de31859ade394bd3753, Data_science\Visualization\test\vennTest.vb"

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

    '   Total Lines: 15
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 801 B


    ' Module vennTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Imaging

Module vennTest

    Sub Main()
        Dim a As New VennSet With {.color = Color.AliceBlue, .intersections = New Dictionary(Of String, Integer) From {{"b", 100}}, .Name = "a", .Size = 500}
        Dim b As New VennSet With {.color = Color.DarkGreen, .intersections = New Dictionary(Of String, Integer) From {{"a", 100}}, .Name = "b", .Size = 110}
        Dim c As New VennSet With {.color = Color.DeepSkyBlue, .intersections = New Dictionary(Of String, Integer) From {{"a", 300}, {"b", 30}}, .Name = "c", .Size = 500}

        Call VennPlot.Venn2(a, b).AsGDIImage.SaveAs("./venn2.png")
        Call VennPlot.Venn3(a, b, c).AsGDIImage.SaveAs("./venn3.png")
    End Sub
End Module
