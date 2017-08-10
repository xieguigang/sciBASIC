#Region "Microsoft.VisualBasic::1cafc104ae849505cc95046b1c6c635b, ..\sciBASIC#\Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DendrogramVisualize\NodeLayout.vb"

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

Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace DendrogramVisualize

    ''' <summary>
    ''' 计算出layout位置信息的结果
    ''' </summary>
    Public Structure NodeLayout

        Dim childs As NodeLayout()
        Dim name$
        Dim distance As Distance
        Dim layout As Coordinate
        
    End Structure
End Namespace
