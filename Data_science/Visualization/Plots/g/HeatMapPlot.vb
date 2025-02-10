#Region "Microsoft.VisualBasic::96ca66a28078728aec8284d72c5d0174, Data_science\Visualization\Plots\g\HeatMapPlot.vb"

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

    '   Total Lines: 86
    '    Code Lines: 69 (80.23%)
    ' Comment Lines: 8 (9.30%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (10.47%)
    '     File Size: 3.34 KB


    '     Class HeatMapPlot
    ' 
    '         Properties: colors, mapLevels, reverseColors
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBrushes, GetColors, Z
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Distributions

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
Imports FontStyle = System.Drawing.FontStyle
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
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

Namespace Graphic

    Public MustInherit Class HeatMapPlot : Inherits Plot

        Public Property mapLevels As Integer
        Public Property colors As Color()
        Public Property reverseColors As Boolean = False

        Protected Sub New(theme As Theme)
            MyBase.New(theme)
            colors = Designer.GetColors(theme.colorSet)
        End Sub

        ''' <summary>
        ''' get brush from <see cref="colors"/>
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Function GetBrushes() As SolidBrush()
            Dim colors = If(reverseColors, Me.colors.Reverse, Me.colors)
            Return Designer.CubicSpline(colors, n:=mapLevels) _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray
        End Function

        ''' <summary>
        ''' get interpolated <see cref="colors"/>
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Function GetColors() As Color()
            Return Designer.CubicSpline(If(reverseColors, Me.colors.Reverse, Me.colors), n:=mapLevels)
        End Function

        Public Shared Iterator Function Z(scatter As IEnumerable(Of PixelData)) As IEnumerable(Of PixelData)
            Dim allSpots As PixelData() = scatter.ToArray
            Dim v As Double() = allSpots _
                .Select(Function(p) p.Scale) _
                .AsVector _
                .Z _
                .ToArray

            For i As Integer = 0 To v.Length - 1
                Yield New PixelData With {
                    .Scale = v(i),
                    .X = allSpots(i).X,
                    .Y = allSpots(i).Y
                }
            Next
        End Function
    End Class
End Namespace
