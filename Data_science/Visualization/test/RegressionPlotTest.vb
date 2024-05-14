#Region "Microsoft.VisualBasic::46846a79968d968d90bebaad8fd0979a, Data_science\Visualization\test\RegressionPlotTest.vb"

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
    '     File Size: 596 B


    ' Module RegressionPlotTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Imaging

Module RegressionPlotTest
    Sub Main()
        Dim weight = {115.0, 117, 120, 123, 126, 129, 132, 135, 139, 142, 146, 150, 154, 159, 164}
        Dim height = {58.0, 59.0, 60.0, 61.25, 62.0, 63.5, 64.0, 65.0, 66.1, 67.1, 68.0, 69.0, 70.0, 71.8, 72.0}
        Dim linear = LeastSquares.PolyFit(height, weight, 6)

        Call RegressionPlot.Plot(linear).AsGDIImage.SaveAs("./RegressionPlot.png")

        Pause()
    End Sub
End Module
