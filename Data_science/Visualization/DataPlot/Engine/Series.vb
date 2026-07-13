#Region "Microsoft.VisualBasic::5a643ab7c3309d790881b89a644455af, Data_science\Visualization\DataPlot\Engine\Series.vb"

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

    '   Total Lines: 13
    '    Code Lines: 11 (84.62%)
    ' Comment Lines: 1 (7.69%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (7.69%)
    '     File Size: 484 B


    ' Class Series
    ' 
    '     Properties: Color, LineStyle, MarkerShape, Name, Visible
    '                 X, Y
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>图表数据系列</summary>
Public Class Series
    Public Property Name As String = ""
    Public Property Color As Color? = Nothing
    Public Property X As Double() = {}
    Public Property Y As Double() = {}
    Public Property MarkerShape As MarkerShape = MarkerShape.Circle
    Public Property LineStyle As DashStyle = DashStyle.Solid
    Public Property Visible As Boolean = True
End Class
