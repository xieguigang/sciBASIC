#Region "Microsoft.VisualBasic::58077c6b20a05471b6aa4adc11dc1904, Data_science\Visualization\Plots\g\Theme\Theme.vb"

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

    '     Class Theme
    ' 
    '         Properties: axisLabelCSS, background, drawGrid, drawLegend, legendBoxStroke
    '                     legendLabelCSS, legendLayout, legendTitleCSS, mainCSS, padding
    '                     subtitleCSS, tagCSS, xAxisLayout, yAxisLayout
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis

Namespace Graphic.Canvas

    Public Class Theme

        Public Property background As String
        Public Property padding As String
        Public Property mainCSS As String
        Public Property subtitleCSS As String
        Public Property legendTitleCSS As String
        Public Property legendLabelCSS As String
        Public Property legendLayout As Layout
        Public Property legendBoxStroke As String
        Public Property tagCSS As String
        Public Property xAxisLayout As XAxisLayoutStyles
        Public Property yAxisLayout As YAxisLayoutStyles
        Public Property axisLabelCSS As String
        Public Property drawLegend As Boolean
        Public Property drawGrid As Boolean

    End Class
End Namespace
