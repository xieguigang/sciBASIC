#Region "Microsoft.VisualBasic::40e047c363d763a4e27819f95aa545c4, Data_science\Visualization\test\PCAPlotTest.vb"

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

    '   Total Lines: 32
    '    Code Lines: 24 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (25.00%)
    '     File Size: 1.43 KB


    ' Module PCAPlotTest
    ' 
    '     Function: projectDataset
    ' 
    '     Sub: decathlon2Test, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.PCA
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Prcomp
Imports csvFile = Microsoft.VisualBasic.Data.csv.IO.File

Module PCAPlotTest

    Sub Main()
        Call decathlon2Test()


        Dim data As GeneralMatrix = csvFile.Load("D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\flower.csv").AsMatrix

        Call PCAPlot.PC2(data, 8).AsGDIImage.SaveAs("D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\flower.PCA2.png")
    End Sub

    Private Function projectDataset(sample As DataSet) As DataSet
        Return sample.SubSet({"X100m", "Long.jump", "Shot.put", "High.jump", "X400m", "X110m.hurdle", "Discus", "Pole.vault", "Javeline", "X1500m"})
    End Function

    Sub decathlon2Test()
        Dim data As DataSet() = DataSet.LoadDataSet("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\PCA\decathlon2\decathlon2.csv").Select(AddressOf projectDataset).ToArray
        Dim index = data.PropertyNames
        Dim matrix = data.Select(Function(d) index.Select(Function(name) d(name)).AsVector).ToArray
        Dim res_pca As New PCA(matrix, True, True)

        Pause()
    End Sub
End Module
