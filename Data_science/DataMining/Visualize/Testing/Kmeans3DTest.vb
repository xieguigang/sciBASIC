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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language

Module Kmeans3DTest

    Sub Main()

        Dim matrix As New List(Of DataSet)
        Dim cata As New Dictionary(Of NamedCollection(Of String))

        cata += New NamedCollection(Of String) With {
            .Name = "T2 vs T1", .Value = {"1", "2", "3", "4"}}
        cata += New NamedCollection(Of String) With {
            .Name = "T3 vs T2", .Value = {"5", "6", "7", "8"}}
        cata += New NamedCollection(Of String) With {
            .Name = "T4 vs T3", .Value = {"9", "10", "11", "12"}}

        Dim rnd As New Random

        For i As Integer = 0 To 1000
            matrix += New DataSet With {
                .ID = i,
                .Properties = 12.SeqRandom.ToDictionary(Function(id) CStr(id + 1), Function() rnd.NextDouble * 1000000)
            }
        Next

        Dim camera As New Camera With {
            .fov = 500000,
            .screen = New Size(1200, 1000),
            .ViewDistance = 3400,
            .angleX = 30,
            .angleY = 60,
            .angleZ = -56.25
        }

        Call Kmeans.Scatter3D(matrix, cata, camera).AsGDIImage.CorpBlank(30, Color.White).SaveAs("./kmeans3D.png")
    End Sub
End Module

