#Region "Microsoft.VisualBasic::f9c15fb13d735dc3fbf1d7cd2ca53bd0, ..\sciBASIC#\gr\Datavisualization.Network\NetworkCanvas\Styling\StyleMapper.vb"

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
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Quantile
Imports Microsoft.VisualBasic.Scripting

Namespace Styling

    ''' <summary>
    ''' 
    ''' </summary>
    Public Structure StyleMapper

        Dim nodeSize As Func(Of Node, Double)
        Dim nodePaints As Func(Of Node, Color)
        Dim nodeLabelSize As Func(Of Node, Double)
        Dim nodeLabelPaints As Func(Of Node, Color)
        Dim nodeLabels As Func(Of Node, String)

    End Structure
End Namespace
