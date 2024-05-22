#Region "Microsoft.VisualBasic::3af2f49e121c64755e9b05ef2ed8e5d1, Data_science\DataMining\BinaryTree\test\Program.vb"

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

    '   Total Lines: 17
    '    Code Lines: 5 (29.41%)
    ' Comment Lines: 9 (52.94%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (17.65%)
    '     File Size: 693 B


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

'Imports System
'Imports Microsoft.VisualBasic.Data.csv.IO
'Imports Microsoft.VisualBasic.DataMining.BinaryTree
'Imports Microsoft.VisualBasic.DataMining.KMeans

Module Program
    Sub Main(args As String())
        '        Dim ds As DataSet() = DataSet.LoadDataSet("G:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\f5dfc234eafd0716742e3b14b72a70c0_umap3.csv").ToArray
        '        Dim data As EntityClusterModel() = EntityClusterModel.FromModel(ds).ToArray
        '        Dim knn As New KNNGraph(data)
        '        Dim test = knn.BuildClusterGraph(k:=16)

        Call AffinityPropagationDemo.Main2()

        '        Pause()
    End Sub
End Module
