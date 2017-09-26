#Region "Microsoft.VisualBasic::1bf017a9a3ac884aac918a4bf364fb12, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\Testing\Module2.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap

Module Module2

    Sub Main()

        Dim path = "G:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\images\dendrogram\heatmap.Test.csv"
        Dim data = DataSet.LoadDataSet(path)
        Dim experiments As New Dictionary(Of String, String) From {
            {"T1", "red"},
            {"T2", "red"},
            {"T3", "red"},
            {"T4", "red"},
            {"K1", "blue"},
            {"K2", "blue"},
            {"K3", "blue"},
            {"K4", "blue"},
            {"average", "green"}
        }

        Call Heatmap.Plot(data, size:="3200,6000", reverseClrSeq:=True, drawScaleMethod:=DrawElements.Cols, drawClass:=(Nothing, experiments)).Save(path.TrimSuffix & ".png")

        ' Call AxisScalling.CreateAxisTicks({-10.3301, 13.7566}, 20).GetJson(True).__DEBUG_ECHO

        Pause()

    End Sub


End Module

