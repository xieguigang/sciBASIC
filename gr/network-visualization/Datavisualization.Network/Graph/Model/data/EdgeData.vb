#Region "Microsoft.VisualBasic::2a89d0958f4ba036d69e790e9716be35, gr\network-visualization\Datavisualization.Network\Graph\Model\data\EdgeData.vb"

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

    '   Total Lines: 94
    '    Code Lines: 70 (74.47%)
    ' Comment Lines: 12 (12.77%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (12.77%)
    '     File Size: 3.31 KB


    '     Class EdgeData
    ' 
    '         Properties: bends, length, style
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Clone, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.EdgeBundling
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

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
Imports LineCap = System.Drawing.Drawing2D.LineCap
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
Imports LineCap = Microsoft.VisualBasic.Imaging.LineCap
Imports TextureBrush = Microsoft.VisualBasic.Imaging.TextureBrush
#End If

Namespace Graph

    Public Class EdgeData : Inherits GraphData

        ''' <summary>
        ''' 这个属性值一般是由两个节点之间的坐标位置所计算出来的欧几里得距离
        ''' </summary>
        ''' <returns></returns>
        Public Property length As Double
        Public Property bends As WayPointVector()

        ''' <summary>
        ''' [color, width, dash]
        ''' </summary>
        ''' <returns></returns>
        Public Property style As Pen

        Public Sub New()
            MyBase.New()

            length = 1.0F
        End Sub

        ''' <summary>
        ''' Value copy
        ''' </summary>
        ''' <param name="copy"></param>
        Sub New(copy As EdgeData)
            Me.label = copy.label
            Me.length = copy.length
            Me.Properties = New Dictionary(Of String, String)(copy.Properties)
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function Clone() As EdgeData
            Dim bendList As WayPointVector() = bends _
                .SafeQuery _
                .Select(Function(a)
                            Return New WayPointVector(a)
                        End Function) _
                .ToArray
            Dim styleCopy As Pen = Nothing

            If Not style Is Nothing Then
                styleCopy = New Pen(style.Color, style.Width) With {
                    .DashStyle = style.DashStyle
                }
            End If

            Return New EdgeData With {
                .label = label,
                .bends = bendList,
                .style = styleCopy,
                .length = length,
                .Properties = New Dictionary(Of String, String)(Properties)
            }
        End Function
    End Class
End Namespace
