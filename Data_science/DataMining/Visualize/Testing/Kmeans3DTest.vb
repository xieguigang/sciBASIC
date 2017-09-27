#Region "Microsoft.VisualBasic::f1ddeffb0041f52f92a8da3fbc0d871d, ..\sciBASIC#\Data_science\DataMining\Visualize\Testing\Module1.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.Visualize.DataMining
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Module Kmeans3DTest

    Sub Main()

        Dim matrix As DataSet() = DataSet.LoadDataSet("D:\projects\杨先友-山核桃蛋白组\3. DEPs\Time_series.heatmap\Time_series.heatmap.csv").ToArray
        Dim cata As New Dictionary(Of NamedCollection(Of String))

        cata += New NamedCollection(Of String) With {
            .Name = "T2 vs T1", .Value = {"B1.A1", "B1.A2", "B2.A1", "B2.A2"}}
        cata += New NamedCollection(Of String) With {
            .Name = "T3 vs T2", .Value = {"C1.B1", "C1.B2", "C2.B1", "C2.B2"}}
        cata += New NamedCollection(Of String) With {
            .Name = "T4 vs T3", .Value = {"D1.C1", "D1.C2", "D2.C1", "D2.C2"}}

        Dim camera As New Camera With {
            .fov = 1000,
            .screen = New Size(2000, 1600),
            .ViewDistance = 50
        }

        Call Kmeans.Scatter3D(matrix, cata, camera).Save("./kmeans3D.png")
    End Sub
End Module

