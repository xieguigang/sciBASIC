#Region "Microsoft.VisualBasic::64f323af4af4104fc143d30e42288576, Data_science\Visualization\test\DensityPlotTest.vb"

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

    '   Total Lines: 22
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 784 B


    ' Module DensityPlotTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports stdNum = System.Math

Module DensityPlotTest

    Sub Main()
        Dim data = DataSet.LoadDataSet("D:\OneDrive\2017-8-31\3. DEPs\Time_series\T4vsT3.csv")
        Dim points = data.Select(Function(x)
                                     Return New PointF(x!log2FC, -stdNum.Log10(x("p.value")))
                                 End Function).ToArray

        Call DensityPlot.Plot(
            points,
            ptSize:=15,
            levels:=65,
            schema:=ColorMap.PatternJet).Save("./test.png")
    End Sub
End Module
