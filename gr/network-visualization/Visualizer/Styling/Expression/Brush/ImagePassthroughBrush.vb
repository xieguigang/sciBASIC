#Region "Microsoft.VisualBasic::14b2f43162bdcbc63fe9426b0d465115, gr\network-visualization\Visualizer\Styling\Expression\Brush\ImagePassthroughBrush.vb"

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

    '   Total Lines: 38
    '    Code Lines: 28
    ' Comment Lines: 3
    '   Blank Lines: 7
    '     File Size: 1.39 KB


    '     Class ImagePassthroughBrush
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
    ''' 属性名作为文件名，从指定的文件夹之中读取图片文件的passthrough映射
    ''' </summary>
    Public Class ImagePassthroughBrush : Implements IGetBrush

        ReadOnly directory$
        ReadOnly extensionName$
        ReadOnly selector As Func(Of Node, Object)

        Sub New(map As MapExpression)
            directory = map.values(Scan0).GetStackValue("(", ")").Trim(" "c, "'"c)
            extensionName = map.values(1).Trim(" "c, "."c, "*"c)
            selector = map.propertyName.SelectNodeValue
        End Sub

        Public Iterator Function GetBrush(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Brush)) Implements IGetBrush.GetBrush
            Dim filePath As String
            Dim brush As Brush

            For Each n As Node In nodes
                filePath = $"{directory}/{selector(n)}.{extensionName}"
                brush = New TextureBrush(filePath.LoadImage)

                Yield New Map(Of Node, Brush) With {
                    .Key = n,
                    .Maps = brush
                }
            Next
        End Function
    End Class
End Namespace
