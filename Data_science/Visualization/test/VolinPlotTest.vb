#Region "Microsoft.VisualBasic::7c6f76be586555303be099a2a84e3d02, Data_science\Visualization\test\VolinPlotTest.vb"

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

    ' Module VolinPlotTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver

Module VolinPlotTest

    Sub Main()
        Dim data As IEnumerable(Of DataSet) = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups.csv").ToArray



        Call ViolinPlot.Plot(dataset:=data, size:="3300,3600", removesOutliers:=False, yTickFormat:="G2").Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups.VolinPlot.png")

        g.SetDriver(Drivers.SVG)

        Call ViolinPlot.Plot(dataset:=data, size:="3300,3600", removesOutliers:=False, yTickFormat:="G2", labelAngle:=0.0).Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups.VolinPlot.svg")

        Call Pause()
    End Sub
End Module
