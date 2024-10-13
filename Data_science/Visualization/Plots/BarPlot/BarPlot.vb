#Region "Microsoft.VisualBasic::41576917a6d0f6428d5992dfb575f2d6, Data_science\Visualization\Plots\BarPlot\BarPlot.vb"

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

    '   Total Lines: 96
    '    Code Lines: 66 (68.75%)
    ' Comment Lines: 22 (22.92%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (8.33%)
    '     File Size: 4.14 KB


    '     Module BarPlotAPI
    ' 
    '         Function: FromData, Plot, Rectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
#End If

Namespace BarPlot

    ''' <summary>
    ''' 这个不像<see cref="Histogram"/>用于描述若干组连续的数据，这个是将数据按照标签分组来表述出来的
    ''' </summary>
    <HideModuleName>
    Public Module BarPlotAPI

        ''' <summary>
        ''' Bar data plot
        ''' </summary>
        ''' <param name="data">Data groups</param>
        ''' <param name="size">image output size</param>
        ''' <param name="padding">margin blank of the plots region</param>
        ''' <param name="bg$">Background color expression</param>
        ''' <param name="showGrid">Show axis grid?</param>
        ''' <param name="stacked">Bar plot is stacked of each sample?</param>
        ''' <param name="stackReordered">reorder bar data? Only works in stacked mode</param>
        ''' <param name="showLegend">Show data legend?</param>
        ''' <param name="legendPos">Position of the data legend on the image</param>
        ''' <param name="legendBorder">legend shape border style.</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(data As BarDataGroup,
                             Optional size As Size = Nothing,
                             Optional padding$ = "padding: 300 120 300 120;",
                             Optional bg$ = "white",
                             Optional showGrid As Boolean = True,
                             Optional stacked As Boolean = False,
                             Optional stackReordered? As Boolean = True,
                             Optional showLegend As Boolean = True,
                             Optional legendPos As Point = Nothing,
                             Optional legendBorder As Stroke = Nothing,
                             Optional legendFont As Font = Nothing,
                             Optional dpi As Integer = 300,
                             Optional driver As Drivers = Drivers.GDI) As GraphicsData

            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .drawGrid = showGrid,
                .drawLegend = showLegend
            }
            Dim app As New SimpleBarPlot(data, theme) With {
                .stacked = stacked,
                .stackReorder = stackReordered
            }

            If Not legendPos.IsEmpty Then
                theme.legendLayout = New Absolute(legendPos)
            End If

            Return app.Plot(size, dpi, driver)
        End Function

        Public Function Rectangle(top As Single, left As Single, right As Single, bottom As Single) As Rectangle
            Dim pt As New Point(left, top)
            Dim size As New Size(right - left, bottom - top)
            Return New Rectangle(pt, size)
        End Function

        ''' <summary>
        ''' Creates sample groups from a data vector
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function FromData(data As IEnumerable(Of Double)) As BarDataGroup
            Return New BarDataGroup With {
                .Serials = {
                    New NamedValue(Of Color) With {
                        .Name = "",
                        .Value = Color.Lime
                    }
                },
                .Samples = LinqAPI.Exec(Of BarDataSample) <=
                    From n
                    In data.SeqIterator
                    Select New BarDataSample With {
                        .data = {n.value},
                        .tag = n.i
                    }
            }
        End Function
    End Module
End Namespace
