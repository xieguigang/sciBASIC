#Region "Microsoft.VisualBasic::cb65ad357f236df1729bf0a50bd5c042, Data_science\Visualization\Plots-statistics\Heatmap\Heatmap.vb"

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

    '   Total Lines: 191
    '    Code Lines: 122 (63.87%)
    ' Comment Lines: 51 (26.70%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 18 (9.42%)
    '     File Size: 10.56 KB


    '     Module Heatmap
    ' 
    '         Function: Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Heatmap

    ''' <summary>
    ''' A heat map (or heatmap) is a graphical representation of data where the individual values 
    ''' contained in a matrix are represented as colors. The term 'heat map' was originally coined 
    ''' and trademarked by software designer Cormac Kinney in 1991, to describe a 2D display 
    ''' depicting financial market information,[1] though similar plots such as shading matrices 
    ''' have existed for over a century.
    ''' </summary>
    Public Module Heatmap

        ' dendrogramLayout$ = A,B
        '                                         |
        '    A                                    | B
        ' ------+---------------------------------+
        '       |
        '       |
        '       |
        '       |

        ''' <summary>
        ''' 可以用来表示任意变量之间的相关度
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="customColors">
        ''' 可以使用这一组颜色来手动自定义heatmap的颜色，也可以使用<paramref name="mapName"/>来获取内置的颜色谱
        ''' </param>
        ''' <param name="mapLevels"></param>
        ''' <param name="mapName">
        ''' The color map name, using for the <see cref="Designer"/>
        ''' 
        ''' There are many different color schemes that can be used to illustrate the heatmap, with perceptual advantages 
        ''' and disadvantages for each. Rainbow colormaps are often used, as humans can perceive more shades of color than 
        ''' they can of gray, and this would purportedly increase the amount of detail perceivable in the image. However, 
        ''' this is discouraged by many in the scientific community, for the following reasons:
        '''
        ''' + The colors lack the natural perceptual ordering found In grayscale Or blackbody spectrum colormaps.
        ''' + Common colormaps(Like the "jet" colormap used As the Default In many visualization software packages) have 
        '''   uncontrolled changes In luminance that prevent meaningful conversion To grayscale For display Or printing. 
        '''   This also distracts from the actual data, arbitrarily making yellow And cyan regions appear more prominent 
        '''   than the regions Of the data that are actually most important.[citation needed]
        ''' + The changes between colors also lead To perception Of gradients that aren't actually present, making actual 
        '''   gradients less prominent, meaning that rainbow colormaps can actually obscure detail in many cases rather than 
        '''   enhancing it.
        ''' </param>
        ''' <param name="size"></param>
        ''' <param name="bg$"></param>
        ''' <param name="logTransform">0或者小于零的数表示不会进行log变换</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(data As IEnumerable(Of DataSet),
                             Optional customColors As Color() = Nothing,
                             Optional reverseClrSeq As Boolean = False,
                             Optional mapLevels% = 100,
                             Optional mapName$ = ColorBrewer.DivergingSchemes.RdYlBu11,
                             Optional size$ = "3000,2700",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional logTransform# = 0,
                             Optional drawScaleMethod As DrawElements = DrawElements.Cols,
                             Optional drawLabels As DrawElements = DrawElements.Both,
                             Optional drawDendrograms As DrawElements = DrawElements.Rows,
                             Optional drawClass As (rowClass As Dictionary(Of String, String), colClass As Dictionary(Of String, String)) = Nothing,
                             Optional dendrogramLayout$ = "200,200",
                             Optional rowLabelfontStyle$ = CSSFont.Win7Normal,
                             Optional colLabelFontStyle$ = CSSFont.Win7LargerBold,
                             Optional legendTitle$ = "Heatmap Color Legend",
                             Optional legendFontStyle$ = CSSFont.PlotSubTitle,
                             Optional min# = -1,
                             Optional max# = 1,
                             Optional mainTitle$ = "heatmap",
                             Optional titleFontCSS$ = CSSFont.Win7VeryLarge,
                             Optional drawGrid As Boolean = False,
                             Optional drawValueLabel As Boolean = False,
                             Optional valuelabelFontCSS$ = CSSFont.PlotLabelNormal,
                             Optional legendWidth! = -1,
                             Optional legendHasUnmapped As Boolean = True,
                             Optional legendSize$ = "600,100",
                             Optional tick# = -1,
                             Optional legendLayout As Layouts = Layouts.Horizon,
                             Optional ppi As Integer = 100) As GraphicsData

            Dim array As DataSet() = data.ToArray
            Dim dlayout As (A%, B%)
            Dim dataTable As Dictionary(Of DataSet) = array.ToDictionary
            Dim margin As Padding = padding
            Dim env As CSSEnvirnment = CSSEnvirnment.Empty(ppi)

            With dendrogramLayout.SizeParser
                dlayout = (.Width, .Height)
            End With

            Dim plotInternal =
                Sub(g As IGraphics, region As GraphicsRegion, args As PlotArguments)
                    Dim css As CSSEnvirnment = g.LoadEnvironment
                    Dim dw! = args.dStep.Width, dh! = args.dStep.Height
                    Dim blockSize As New SizeF(dw, dh)
                    Dim colors As SolidBrush() = args.colors
                    Dim valuelabelFont As Font = css.GetFont(valuelabelFontCSS)
                    Dim titleFont As Font = css.GetFont(titleFontCSS)
                    Dim legendFont As Font = css.GetFont(legendFontStyle)
                    Dim rowLabelFont As Font = css.GetFont(rowLabelfontStyle)

                    ' 按行绘制heatmap之中的矩阵
                    For Each x As DataSet In args.RowOrders.Select(Function(key) dataTable(key))     ' 在这里绘制具体的矩阵
                        Dim levelRow As DataSet = args.levels(x.ID)

                        For Each key As String In args.ColOrders
                            Dim c# = x(key)
                            Dim level% = levelRow(key)  '  得到等级
                            Dim b = colors(
                                If(level% > colors.Length - 1,
                                    colors.Length - 1,
                                    level))
                            Dim rect As New RectangleF With {
                                .Location = New PointF(args.left, args.top),
                                .Size = blockSize
                            }
#If DEBUG Then
                            ' Call $"{level} -> {b.Color.ToString}".__DEBUG_ECHO
#End If
                            Call g.FillRectangle(b, rect)

                            If drawGrid Then
                                Call g.DrawRectangles(Pens.WhiteSmoke, {rect})
                            End If
                            If drawValueLabel Then

                                With c.ToString("F2")
                                    Dim ksz As SizeF = g.MeasureString(.ByRef, valuelabelFont)
                                    Dim kpos As New PointF With {
                                        .X = rect.Left + (rect.Width - ksz.Width) / 2,
                                        .Y = rect.Top + (rect.Height - ksz.Height) / 2
                                    }
                                    Call g.DrawString(.ByRef, valuelabelFont, Brushes.White, kpos)
                                End With
                            End If

                            args.left += dw!
                        Next

                        args.left = args.matrixPlotRegion.Left
                        args.top += dh!

                        ' debug
                        ' Call g.DrawLine(Pens.Blue, New Point(args.left, args.top), New Point(args.matrixPlotRegion.Right, args.top))

                        If drawLabels = DrawElements.Both OrElse drawLabels = DrawElements.Rows Then
                            Dim sz As SizeF = g.MeasureString(x.ID, rowLabelFont)
                            Dim y As Single = args.top - dh - (sz.Height - dh) / 2
                            Dim lx As Single = args.matrixPlotRegion.Right + 10

                            ' 绘制行标签
                            Call g.DrawString(x.ID, rowLabelFont, Brushes.Black, New PointF(lx, y))
                        End If
                    Next

                    ' debug
                    ' Call g.DrawRectangle(Pens.LawnGreen, args.matrixPlotRegion)
                End Sub

            Return doPlot(
                plotInternal, array,
                env.GetFont(rowLabelfontStyle), env.GetFont(colLabelFontStyle), logTransform, drawScaleMethod, drawLabels, drawDendrograms, drawClass, dlayout,
                reverseClrSeq, customColors.GetBrushes, mapLevels, mapName,
                size.SizeParser, margin, bg,
                legendTitle, env.GetFont(legendFontStyle), Nothing,
                min, max,
                mainTitle, env.GetFont(titleFontCSS),
                legendWidth, legendHasUnmapped, legendSize.SizeParser,
                tick:=tick,
                legendLayout:=legendLayout
            )
        End Function
    End Module
End Namespace
