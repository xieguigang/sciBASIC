#Region "Microsoft.VisualBasic::4ac8a91c6d15a7784fcbff353e1beb24, gr\network-visualization\Visualizer\Styling\Expression\Brush\UnifyColorBrush.vb"

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

    '   Total Lines: 28
    '    Code Lines: 20
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 906 B


    '     Class UnifyColorBrush
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

Namespace Styling.FillBrushes

    ''' <summary>
    ''' 全部都使用统一的颜色进行填充
    ''' </summary>
    Public Class UnifyColorBrush : Implements IGetBrush

        ReadOnly brush As SolidBrush

        Sub New(expression As String)
            brush = New SolidBrush(expression.TranslateColor)
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
