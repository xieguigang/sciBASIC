#Region "Microsoft.VisualBasic::4eac5906847f21ac201442cc6405b015, Data_science\Visualization\test\VolinPlotTest.vb"

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

Module VolinPlotTest

    Sub Main()
        Dim data As IEnumerable(Of DataSet) = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups.csv").ToArray


        Call VolinPlot.Plot(dataset:=data, size:="2400,2700", removesOutliers:=False, yTickFormat:="G2").Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Visualization\data\sample_groups.VolinPlot.png")
        Call Pause()
    End Sub
End Module

