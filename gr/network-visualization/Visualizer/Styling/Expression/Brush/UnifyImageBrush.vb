#Region "Microsoft.VisualBasic::3628ccb3e8bbcf28796cd26fe32a8211, gr\network-visualization\Visualizer\Styling\Expression\Brush\UnifyImageBrush.vb"

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

'   Total Lines: 31
'    Code Lines: 22 (70.97%)
' Comment Lines: 3 (9.68%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 6 (19.35%)
'     File Size: 1005 B


'     Class UnifyImageBrush
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: GetBrush
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

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
Imports TextureBrush = System.Drawing.TextureBrush
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
Imports TextureBrush = Microsoft.VisualBasic.Imaging.TextureBrush
#End If

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 全部都使用统一的图案
    ''' </summary>
    Public Class UnifyImageBrush : Implements IGetBrush

        ReadOnly brush As TextureBrush

        Sub New(expression As String)
            Dim image As Image = UrlEvaluator.EvaluateAsImage(expression)
            Dim brush As New TextureBrush(image)

            Me.brush = brush
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            For Each n As Node In nodes
                Yield New Map(Of Node, Brush) With {
                    .Key = n,
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
