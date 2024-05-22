#Region "Microsoft.VisualBasic::2dd36df890fd43916c15ad3e1b811e5b, Data_science\DataMining\DensityQuery\GridBox.vb"

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

    '   Total Lines: 32
    '    Code Lines: 24 (75.00%)
    ' Comment Lines: 1 (3.12%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (21.88%)
    '     File Size: 1010 B


    ' Class GridBox
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Gridding
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph

Public Class GridBox(Of T)

    ReadOnly grid As Grid(Of T)
    ReadOnly gridSize As Size

    Sub New(grid As Grid(Of T), boxWidth As Integer, boxHeight As Integer)
        Me.grid = grid
        Me.gridSize = New Size(boxWidth, boxHeight)
    End Sub

    Public Iterator Function Gridding() As IEnumerable(Of T())
        Dim rect As Rectangle = grid.rectangle
        Dim center As Point
        Dim dw As Integer = gridSize.Width / 2
        Dim dh As Integer = gridSize.Height / 2
        Dim block As T()

        ' [x,y] is top left
        For x As Integer = rect.Left To rect.Right Step gridSize.Width
            For y As Integer = rect.Top To rect.Bottom Step gridSize.Height
                center = New Point(x + dw, y + dh)
                block = grid.Query(center.X, center.Y, gridSize).ToArray

                Yield block
            Next
        Next
    End Function

End Class
