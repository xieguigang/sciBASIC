#Region "Microsoft.VisualBasic::80fc21a29455d209471dd84d3d0d73e6, ..\sciBASIC#\Data_science\DataMining\Visualize\Kmeans.vb"

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
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans

Public Module Kmeans

    Public Function Scatter2D()

    End Function

    ''' <summary>
    ''' 这个函数主要是生成<see cref="Serial3D"/>数据模型组
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="catagory">
    ''' How to read the data and construct the <see cref="Serial3D"/> model group
    ''' </param>
    ''' <param name="size$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="padding$"></param>
    ''' <param name="clusterN">Expected kmeans cluster resulted number, default is 6 cluster</param>
    ''' <returns></returns>
    <Extension>
    Public Function Scatter3D(data As IEnumerable(Of DataSet),
                              catagory As Dictionary(Of NamedCollection(Of String)),
                              Optional size$ = "1600,1200",
                              Optional bg$ = "white",
                              Optional padding$ = g.DefaultPadding,
                              Optional clusterN% = 6) As GraphicsData

        Dim clusters As EntityLDM() = data.ToKMeansModels.Kmeans(expected:=clusterN)
    End Function
End Module

