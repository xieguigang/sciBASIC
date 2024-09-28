#Region "Microsoft.VisualBasic::090e8cdb4e143bb186dc293db70de869, Data_science\Visualization\Plots\g\Legends\LegendObject.vb"

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

    '   Total Lines: 58
    '    Code Lines: 23 (39.66%)
    ' Comment Lines: 29 (50.00%)
    '    - Xml Docs: 96.55%
    ' 
    '   Blank Lines: 6 (10.34%)
    '     File Size: 1.87 KB


    '     Class LegendObject
    ' 
    '         Properties: color, fontstyle, style, title
    ' 
    '         Function: GetFont, MeasureTitle, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

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

Namespace Graphic.Legend

    ''' <summary>
    ''' 图例
    ''' </summary>
    Public Class LegendObject

        ''' <summary>
        ''' The shape of this legend
        ''' </summary>
        ''' <returns></returns>
        Public Property style As LegendStyles
        ''' <summary>
        ''' The legend label
        ''' </summary>
        ''' <returns></returns>
        Public Property title As String
        ''' <summary>
        ''' The color of the legend <see cref="style"/> shape.
        ''' </summary>
        ''' <returns></returns>
        Public Property color As String
        ''' <summary>
        ''' CSS expression, which can be parsing by <see cref="CSSFont"/>, drawing font of <see cref="title"/> 
        ''' </summary>
        ''' <returns></returns>
        Public Property fontstyle As String

        ''' <summary>
        ''' <see cref="fontstyle"/> to <see cref="Font"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFont(css As CSSEnvirnment) As Font
            Return css.GetFont(CSSFont.TryParse(fontstyle))
        End Function

        ''' <summary>
        ''' 计算标题文本的绘制大小
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function MeasureTitle(g As IGraphics) As SizeF
            Return g.MeasureString(title, GetFont(g.LoadEnvironment))
        End Function

        Public Overrides Function ToString() As String
            Return title
        End Function
    End Class
End Namespace
