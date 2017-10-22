#Region "Microsoft.VisualBasic::04ea86674dc0b8206d5f59338f69b709, ..\sciBASIC#\Data_science\DataMining\hierarchical-clustering\Test\KMeans.vb"

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

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans

Module KMeans

    Sub Main()

        Dim data = DataSet.LoadDataSet("C:\Users\xieguigang\Desktop\8.27\8.27\2. 17-92 vs ctrl\3. DEPs\data.csv").ToKMeansModels
        data = data.Kmeans(15)
        data.SaveTo("C:\Users\xieguigang\Desktop\8.27\8.27\2. 17-92 vs ctrl\3. DEPs\heatmap.kmeans.csv")
    End Sub
End Module
