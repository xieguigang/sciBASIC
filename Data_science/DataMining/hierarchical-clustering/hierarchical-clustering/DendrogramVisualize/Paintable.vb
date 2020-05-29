#Region "Microsoft.VisualBasic::4286ac68bfeb8cf0c0220ed59acad432, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DendrogramVisualize\Paintable.vb"

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

    '     Interface IPaintable
    ' 
    '         Sub: Paint
    ' 
    '     Structure PainterArguments
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Namespace DendrogramVisualize

    ''' <summary>
    ''' Implemented by visual components of the dendrogram.
    ''' @author lars
    ''' 
    ''' </summary>
    Public Interface IPaintable
        Sub Paint(g As IGraphics, args As PainterArguments, ByRef labels As List(Of NamedValue(Of PointF)))
    End Interface

    ''' <summary>
    ''' 对于绘制水平和竖直布局的层次聚类树而言，只需要在计算布局的时候将xy交换一下即可
    ''' </summary>
    Public Structure PainterArguments

        Dim xDisplayOffset%, yDisplayOffset%, xDisplayFactor#, yDisplayFactor#
        Dim decorated As Boolean
        Dim classHeight!
        Dim layout As Layouts
        ''' <summary>
        ''' ``<see cref="Cluster.Name"/> --> Color``
        ''' </summary>
        Dim classTable As Dictionary(Of String, String)
        Dim stroke As Pen
        Dim classLegendSize As Size
        Dim classLegendPadding%
        Dim ShowLabelName As Boolean
        ''' <summary>
        ''' 点的大小
        ''' </summary>
        Dim LinkDotRadius As Integer
        Dim labelFont As Font
        Dim g2Stroke As Pen

    End Structure
End Namespace
