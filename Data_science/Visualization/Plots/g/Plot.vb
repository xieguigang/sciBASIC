#Region "Microsoft.VisualBasic::5bcebbdcd94c7e7cc5653f028fe49ba3, Data_science\Visualization\Plots\g\Plot.vb"

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

    '   Total Lines: 256
    '    Code Lines: 185 (72.27%)
    ' Comment Lines: 41 (16.02%)
    '    - Xml Docs: 97.56%
    ' 
    '   Blank Lines: 30 (11.72%)
    '     File Size: 10.98 KB


    '     Class Plot
    ' 
    '         Properties: legendTitle, main, xlabel, ylabel, zlabel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: EvaluateLayout, (+2 Overloads) Plot
    ' 
    '         Sub: DrawLegends, DrawMainTitle, DrawMultipleLineTitle, (+2 Overloads) Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
#End If

Namespace Graphic

    ''' <summary>
    ''' the chartting plot framework in sciBASIC 
    ''' </summary>
    Public MustInherit Class Plot

        Protected ReadOnly theme As Theme

        Public Property xlabel As String = "X"
        Public Property ylabel As String = "Y"
        Public Property zlabel As String = "Z"
        Public Property legendTitle As String = "Legend"

        ''' <summary>
        ''' the main title string
        ''' </summary>
        ''' <returns></returns>
        Public Property main As String = Nothing

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(theme As Theme)
            Me.theme = theme
        End Sub

        ''' <summary>
        ''' function for make plot
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="dpi"></param>
        ''' <param name="driver"></param>
        ''' <returns>this function returns a graphics plot wrapper object that supports save png/svg/pdf file</returns>
        Public Overloads Function Plot(size As SizeF,
                                       Optional dpi As Integer = 300,
                                       Optional driver As Drivers = Drivers.Default) As GraphicsData
            Return g.GraphicsPlots(
                size:=size.ToSize,
                padding:=theme.padding,
                bg:=theme.background,
                plotAPI:=AddressOf PlotInternal,
                driver:=driver,
                dpi:=dpi
            )
        End Function

        ''' <summary>
        ''' function for make plot
        ''' </summary>
        ''' <param name="size$"></param>
        ''' <param name="ppi"></param>
        ''' <param name="driver"></param>
        ''' <returns>this function returns a graphics plot wrapper object that supports save png/svg/pdf file</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Overloads Function Plot(Optional size$ = Resolution2K.Size,
                                                   Optional ppi As Integer = 300,
                                                   Optional driver As Drivers = Drivers.Default) As GraphicsData
            Return g.GraphicsPlots(
                size:=size.SizeParser,
                padding:=theme.padding,
                bg:=theme.background,
                plotAPI:=AddressOf PlotInternal,
                driver:=driver,
                dpi:=ppi
            )
        End Function

        ''' <summary>
        ''' make plot with a specific given layout information
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="layout">
        ''' 一般而言，这个属性是<see cref="GraphicsRegion.PlotRegion"/>的属性值
        ''' </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Sub Plot(ByRef g As IGraphics, layout As Rectangle)
            Call PlotInternal(g, EvaluateLayout(g, layout))
        End Sub

        ''' <summary>
        ''' make plot with a specific given layout information
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="canvas"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Sub Plot(ByRef g As IGraphics, canvas As GraphicsRegion)
            Call PlotInternal(g, canvas)
        End Sub

        Protected Shared Function EvaluateLayout(g As IGraphics, layout As Rectangle) As GraphicsRegion
            Dim padding As New Padding With {
                .Left = layout.Left,
                .Top = layout.Top,
                .Bottom = g.Height - layout.Bottom,
                .Right = g.Width - layout.Right
            }
            Dim canvas As New GraphicsRegion(New Size(g.Width, g.Height), padding)

            Return canvas
        End Function

        Protected MustOverride Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)

        ''' <summary>
        ''' custom layout via <see cref="theme.legendLayout"/>
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="legends"></param>
        ''' <param name="showBorder"></param>
        ''' <param name="canvas"></param>
        Protected Sub DrawLegends(g As IGraphics, legends As LegendObject(), showBorder As Boolean, canvas As GraphicsRegion)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim legendLabelFont As Font = css.GetFont(CSSFont.TryParse(theme.legendLabelCSS))
            Dim lsize As SizeF = g.MeasureString("A", legendLabelFont)
            Dim legendParts As LegendObject()() = Nothing
            Dim maxWidth!
            Dim legendPos As PointF
            Dim legendSize$
            Dim region As Rectangle = canvas.PlotRegion(css)

            Const ratio As Double = 0.65

            lsize = New SizeF(lsize.Height * ratio, lsize.Height * ratio)
            legendSize = $"{lsize.Width},{lsize.Height}"

            If theme.legendLayout Is Nothing Then
                Dim maxLen = legends.Select(Function(l) l.title).MaxLengthString
                Dim lFont As Font = css.GetFont(CSSFont.TryParse(legends.First.fontstyle))

                maxWidth! = g.MeasureString(maxLen, lFont).Width
                legendPos = New PointF With {
                    .X = region.Right + lsize.Width,
                    .Y = region.Top + lFont.Height
                }

                If theme.legendSplitSize > 0 AndAlso legends.Length > theme.legendSplitSize Then
                    legendParts = legends.Split(theme.legendSplitSize)
                End If
            Else
                legendPos = theme.legendLayout.GetLocation(canvas, Nothing)
            End If

            If legendParts.IsNullOrEmpty Then
                Call g.DrawLegends(
                    legendPos, legends, legendSize,
                    shapeBorder:=theme.legendBoxStroke,
                    regionBorder:=If(showBorder, theme.legendBoxStroke, Nothing),
                    fillBg:=theme.legendBoxBackground
                )
            Else
                For Each part As LegendObject() In legendParts
                    Call g.DrawLegends(
                        legendPos, part, legendSize,
                        shapeBorder:=theme.legendBoxStroke,
                        regionBorder:=If(showBorder, theme.legendBoxStroke, Nothing),
                        fillBg:=theme.legendBoxBackground
                    )

                    legendPos = New Point With {
                        .X = legendPos.X + maxWidth + lsize.Width + 5,
                        .Y = legendPos.Y
                    }
                Next
            End If
        End Sub

        Protected Sub DrawMainTitle(g As IGraphics, plotRegion As Rectangle, Optional offsetFactor As Double = 1.125)
            If Not main.StringEmpty Then
                Dim css As CSSEnvirnment = g.LoadEnvironment
                Dim fontOfTitle As Font = css.GetFont(CSSFont.TryParse(theme.mainCSS))
                Dim titleSize As SizeF = g.MeasureString(main, fontOfTitle)
                Dim position As New PointF With {
                    .X = plotRegion.X + (plotRegion.Width - titleSize.Width) / 2,
                    .Y = plotRegion.Y - titleSize.Height * offsetFactor
                }
                Dim color As Brush = Brushes.Black

                If position.Y < 0 Then
                    position = New PointF(position.X, 10)
                End If
                If Not theme.mainTextColor.StringEmpty Then
                    color = theme.mainTextColor.GetBrush
                End If

                If theme.mainTextWrap AndAlso titleSize.Width > plotRegion.Width Then
                    Dim charWidth As Double = titleSize.Width / main.Length
                    Dim maxChars As Integer = (plotRegion.Width / charWidth - 1) * 0.85

                    Call DrawMultipleLineTitle(g, plotRegion, fontOfTitle, color, maxChars, offsetFactor)
                Else
                    Call g.DrawString(main, fontOfTitle, color, position)
                End If
            End If
        End Sub

        Private Sub DrawMultipleLineTitle(g As IGraphics,
                                          plotRegion As Rectangle,
                                          fontOfTitle As Font,
                                          color As Brush,
                                          maxChars As Integer,
                                          offsetFactor As Double)

            Dim lines As String() = main.SplitParagraph(len:=maxChars).ToArray
            Dim titleSize As SizeF = g.MeasureString("A", fontOfTitle)
            Dim y As Single = plotRegion.Y - titleSize.Height * offsetFactor * lines.Length
            Dim position As PointF

            For Each line As String In lines
                titleSize = g.MeasureString(line, fontOfTitle)
                position = New PointF With {
                    .X = plotRegion.X + (plotRegion.Width - titleSize.Width) / 2,
                    .Y = y
                }

                y += titleSize.Height + 5
                g.DrawString(line, fontOfTitle, color, position)
            Next
        End Sub
    End Class
End Namespace
