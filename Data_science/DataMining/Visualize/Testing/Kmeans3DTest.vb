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
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.Visualize.DataMining
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language

Module Kmeans3DTest

    Sub Main()

        Dim matrix As List(Of DataSet) = DataSet.LoadDataSet("C:\Users\xieguigang\Desktop\testGL.csv")
        Dim cata As New Dictionary(Of NamedCollection(Of String))

        cata += New NamedCollection(Of String) With {
            .Name = "Degree 1", .Value = {"1.A4", "1.A5", "1.A6"}}
        cata += New NamedCollection(Of String) With {
            .Name = "Degree 2", .Value = {"2.C4", "2.C5", "2.C6"}}
        cata += New NamedCollection(Of String) With {
            .Name = "Degree 3", .Value = {"3.A4", "3.A5", "3.A6"}}

        Dim camera As New Camera With {
            .fov = 500000,
            .screen = New Size(1600, 1600),
            .ViewDistance = 3000,
            .angleX = 30,
            .angleY = 60,
            .angleZ = -56.25,
            .offset = New Point(-1500, 1600)
        }

        Call matrix.SaveTo("./matrix.csv")

        Call Kmeans.Scatter3D(matrix, cata, camera, clusterN:=7).AsGDIImage.CorpBlank(30, Color.White).SaveAs("G:\GCModeller\src\runtime\sciBASIC#\Data_science\kmeans3D.png")
    End Sub
End Module

