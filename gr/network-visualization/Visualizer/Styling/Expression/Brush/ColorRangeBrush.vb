#Region "Microsoft.VisualBasic::25d5f0d06f4c7575dc449f276f129929, gr\network-visualization\Visualizer\Styling\Expression\Brush\ColorRangeBrush.vb"

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

'   Total Lines: 50
'    Code Lines: 39 (78.00%)
' Comment Lines: 4 (8.00%)
'    - Xml Docs: 75.00%
' 
'   Blank Lines: 7 (14.00%)
'     File Size: 1.81 KB


'     Class ColorRangeBrush
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: GetBrush
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

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

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 区间映射，也可能是category映射
    ''' </summary>
    Public Class ColorRangeBrush : Implements IGetBrush

        Dim patternName$
        Dim n%
        Dim colors As Brush()
        Dim range As DoubleRange = New Double() {0, n - 1}
        Dim selector As Func(Of Node, Object)
        Dim getValue As Func(Of Node, Double)

        Sub New(map As MapExpression)
            patternName = map.values(0)
            n = map.values(1)
            colors = Designer _
                .GetColors(patternName, n) _
                .Select(Function(c) DirectCast(New SolidBrush(c), Brush)) _
                .ToArray
            selector = map.propertyName.SelectNodeValue
            getValue = Function(node As Node) Val(selector(node))
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            ' {value, index}
            Dim nodesArray = nodes.ToArray
            Dim nodeValues = nodesArray.RangeTransform(getValue, range)
            Dim index%
            Dim brush As Brush

            For i As Integer = 0 To nodesArray.Length - 1
                index = CInt(nodeValues(i).Maps)
                brush = colors(index)

                Yield New Map(Of Node, Brush) With {
                    .Key = nodesArray(i),
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
