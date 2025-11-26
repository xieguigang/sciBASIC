#Region "Microsoft.VisualBasic::df98c3da0435e5e2186a7ac52cef9131, Data_science\Visualization\Plots-statistics\HeatMap\Models.vb"

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

    '   Total Lines: 60
    '    Code Lines: 30 (50.00%)
    ' Comment Lines: 21 (35.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (15.00%)
    '     File Size: 1.71 KB


    '     Class PlotArguments
    ' 
    '         Properties: dStep
    ' 
    '     Enum DrawElements
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Delegate Sub
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Heatmap

    Public Class PlotArguments

        Public left!

        ''' <summary>
        ''' 绘制矩阵之中的方格在xy上面的步进值
        ''' </summary>
        Public ReadOnly Property dStep As SizeF
            Get
                Return New SizeF With {
                    .Width = matrixPlotRegion.Width / ColOrders.Length,
                    .Height = matrixPlotRegion.Height / RowOrders.Length
                }
            End Get
        End Property

        ''' <summary>
        ''' 矩阵区域的大小和位置
        ''' </summary>
        Public matrixPlotRegion As Rectangle
        Public levels As Dictionary(Of String, DataSet)
        Public top!
        Public colors As SolidBrush()
        Public RowOrders$()
        Public ColOrders$()

    End Class

    ''' <summary>
    ''' Draw a specific heatmap element
    ''' </summary>
    Public Enum DrawElements As Byte
        ''' <summary>
        ''' Draw nothing
        ''' </summary>
        None = 0
        ''' <summary>
        ''' Only draw the heatmap element on matrix row
        ''' </summary>
        Rows = 2
        ''' <summary>
        ''' Only draw the heatmap element on the column
        ''' </summary>
        Cols = 4
        ''' <summary>
        ''' Draw both row and column heatmap elements
        ''' </summary>
        Both = 8
    End Enum

    Friend Delegate Sub HowtoDoPlot(g As IGraphics, region As GraphicsRegion, args As PlotArguments)

End Namespace
