#Region "Microsoft.VisualBasic::7578d500f6246583fda1c117b1c71c2e, gr\network-visualization\Visualizer\Styling\Expression\Brush\PassthroughBrush.vb"

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

    '   Total Lines: 36
    '    Code Lines: 25 (69.44%)
    ' Comment Lines: 4 (11.11%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 7 (19.44%)
    '     File Size: 1.16 KB


    '     Class PassthroughBrush
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBrush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 只能够映射颜色
    ''' </summary>
    Public Class PassthroughBrush : Implements IGetBrush

        ReadOnly selector As Func(Of Node, Object)

        Sub New(expression As String)
            selector = expression.SelectNodeValue
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            Dim color As Color
            Dim brush As Brush

            ' 使用单词进行直接映射
            For Each n As Node In nodes
                color = CStrSafe(selector(n)).TranslateColor
                brush = New SolidBrush(color)

                Yield New Map(Of Node, Brush) With {
                    .Key = n,
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
