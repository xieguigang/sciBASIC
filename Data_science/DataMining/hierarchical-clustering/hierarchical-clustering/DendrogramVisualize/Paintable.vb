#Region "Microsoft.VisualBasic::8c0daf71e37b0102735ab844a9663ac2, ..\sciBASIC#\Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DendrogramVisualize\Paintable.vb"

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

Imports Microsoft.VisualBasic.Imaging

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
    Public Interface Paintable

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="xDisplayOffset%"></param>
        ''' <param name="yDisplayOffset%"></param>
        ''' <param name="xDisplayFactor#"></param>
        ''' <param name="yDisplayFactor#"></param>
        ''' <param name="decorated"></param>
        ''' <param name="classTable">``<see cref="Cluster.Name"/> --> Color``</param>
        Sub paint(g As Graphics2D, xDisplayOffset%, yDisplayOffset%, xDisplayFactor#, yDisplayFactor#, decorated As Boolean, classHeight!, Optional classTable As Dictionary(Of String, String) = Nothing)
    End Interface

End Namespace
