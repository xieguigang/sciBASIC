#Region "Microsoft.VisualBasic::8290d45f4f72ba1b4e23d32127955002, Data_science\Visualization\Plots\g\Plot.vb"

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

'     Class Plot
' 
'         Properties: main, xlabel, ylabel
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Plot
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Graphic

    Public MustInherit Class Plot

        Protected ReadOnly theme As Theme

        Public Property xlabel As String = "X"
        Public Property ylabel As String = "Y"

        ''' <summary>
        ''' the main title string
        ''' </summary>
        ''' <returns></returns>
        Public Property main As String

        Sub New(theme As Theme)
            Me.theme = theme
        End Sub

        Public Function Plot(Optional size$ = Resolution2K.Size, Optional ppi As Integer = 300, Optional driver As Drivers = Drivers.Default) As GraphicsData
            Return g.GraphicsPlots(
                size:=size.SizeParser,
                padding:=theme.padding,
                bg:=theme.background,
                plotAPI:=AddressOf PlotInternal,
                driver:=driver,
                dpi:=$"{ppi},{ppi}"
            )
        End Function

        Public Sub Plot(ByRef g As IGraphics, layout As Rectangle)
            Call PlotInternal(g, EvaluateLayout(g, layout))
        End Sub

        Protected Shared Function EvaluateLayout(g As IGraphics, layout As Rectangle) As GraphicsRegion
            Dim padding As New Padding With {
                .Left = layout.Left,
                .Top = layout.Top,
                .Bottom = g.Size.Height - layout.Bottom,
                .Right = g.Size.Width - layout.Right
            }
            Dim canvas As New GraphicsRegion(g.Size, padding)

            Return canvas
        End Function

        Protected MustOverride Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)

    End Class
End Namespace
