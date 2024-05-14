#Region "Microsoft.VisualBasic::49476d68c88847a89f0153ca4e85a6b5, Data_science\Visualization\Plots\Scatter\LinePlot.vb"

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

    '   Total Lines: 20
    '    Code Lines: 15
    ' Comment Lines: 1
    '   Blank Lines: 4
    '     File Size: 684 B


    ' Module LinePlot
    ' 
    '     Sub: drawErrorLine
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging

Public Module LinePlot

    <Extension>
    Friend Sub drawErrorLine(canvas As IGraphics, scaler As DataScaler, pt As PointF, value#, width!, color As SolidBrush)
        Dim p0 As New PointF With {
            .X = pt.X,
            .Y = scaler.TranslateY(value)
        }

        ' 下面分别绘制竖线误差线以及横线
        Call canvas.DrawLine(New Pen(color), pt, p0)
        Call canvas.DrawLine(New Pen(color), CSng(p0.X - width), p0.Y, CSng(p0.X + width), p0.Y)
    End Sub

End Module
