#Region "Microsoft.VisualBasic::7b5b95a2ca989c266b0cd52e08449e5d, Data_science\Graph\Model\GridGraph\Grid.vb"

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

    '   Total Lines: 83
    '    Code Lines: 45 (54.22%)
    ' Comment Lines: 22 (26.51%)
    '    - Xml Docs: 72.73%
    ' 
    '   Blank Lines: 16 (19.28%)
    '     File Size: 2.51 KB


    '     Class Grid
    ' 
    '         Properties: layout, steps
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: (+2 Overloads) Index, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging

Namespace GridGraph

    ''' <summary>
    ''' 网格也可以看作为一种网络
    ''' </summary>
    Public Class Grid

        ReadOnly X, Y As OrderSelector(Of Double)

        Public ReadOnly Property layout As RectangleF
        Public ReadOnly Property steps As SizeF

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="size">实际的物理大小，而非网格之中的单元格数量</param>
        ''' <param name="steps"></param>
        Sub New(size As Size, steps As SizeF)
            Call Me.New(New Rectangle(New Point, size), steps)
        End Sub

        Sub New(layout As Rectangle, steps As SizeF)
            Call Me.New(layout.ToFloat, steps)
        End Sub

        Sub New(layout As RectangleF, steps As SizeF)
            X = New OrderSelector(Of Double)(Math.seq(layout.X, layout.Right, steps.Width))
            Y = New OrderSelector(Of Double)(Math.seq(layout.Y, layout.Bottom, steps.Height))

            Me.steps = steps
            Me.layout = layout
        End Sub

        ''' <summary>
        ''' 返回数据点在网格之中的``X,Y``方格的顶点编号
        ''' </summary>
        ''' <returns></returns>
        Public Function Index(x#, y#) As Point
            Dim xi%
            Dim yi%

            If x > Me.X.Max Then
                xi = Me.X.Count
            Else
                xi = Me.X.FirstGreaterThan(x)
            End If

            If y > Me.Y.Max Then
                yi = Me.Y.Count
            Else
                yi = Me.Y.FirstGreaterThan(y)
            End If

            ' x = 8
            ' xi-1  xi   xi + 1
            ' 5     10   15
            '    x
            ' index of x is xi -1

            Return New Point(xi - 1, yi - 1)
        End Function

        ''' <summary>
        ''' 返回数据点在网格之中的``X,Y``方格的顶点编号
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        Public Function Index(p As PointF) As Point
            Dim xi = X.FirstGreaterThan(p.X)
            Dim yi = Y.FirstGreaterThan(p.Y)

            Return New Point(xi, yi)
        End Function

        Public Overrides Function ToString() As String
            Return $"{layout} @ {steps}"
        End Function
    End Class
End Namespace
