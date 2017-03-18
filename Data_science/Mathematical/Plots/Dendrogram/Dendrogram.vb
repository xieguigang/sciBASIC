#Region "Microsoft.VisualBasic::c55f07d7b217780d84c04359700c9cc0, ..\sciBASIC#\Data_science\Mathematical\Plots\Dendrogram\Dendrogram.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Dendrogram

    Public Class Tree : Inherits TreeNodeBase(Of Tree)

        Public Sub New(name As String)
            MyBase.New(name)
        End Sub

        Public Overrides ReadOnly Property MySelf As Tree
            Get
                Return Me
            End Get
        End Property
    End Class

    ''' <summary>
    ''' 系统树图；谱系图；树状谱
    ''' </summary>
    Public Module Dendrogram

        <Extension> Public Function Plot(tree As Tree, Optional size As Size = Nothing, Optional padding$ = g.DefaultPadding, Optional bg$ = "white") As Image
            Dim plotInternal =
                Sub(ByRef g As Graphics, region As GraphicsRegion)
                    With region.Padding
                        Call g.__drawVisits(tree, New PointF(.Left * 2, .Top))
                    End With
                End Sub
            Return g.GraphicsPlots(size, padding, bg, plotInternal)
        End Function

        ''' <summary>
        ''' 遍历树，进行绘图操作
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="tree"></param>
        <Extension>
        Private Sub __drawVisits(g As Graphics, tree As Tree, location As PointF)
            Dim y! = location.Y + 100
            Dim x! = location.X
            Dim pen As Pen = Pens.Black

            For Each child As Tree In tree.ChildNodes
                Call g.__drawVisits(child, New PointF(x, y))

                'a
                '|
                '|   
                'b---c
                '    |
                '    d
                Dim my = location.Y + (y - location.Y) / 2

                ' d -> c
                Call g.DrawLine(pen, New Point(x, y), New Point(x, my))
                ' c -> b
                Call g.DrawLine(pen, New Point(x, my), New Point(location.X, my))
                ' b -> a
                Call g.DrawLine(pen, New Point(location.X, my), location)

                x += 100
            Next

            Call tree.__drawNode(g, location)
        End Sub

        <Extension>
        Private Sub __drawNode(node As Tree, g As Graphics, location As PointF)
            Call g.DrawCircle(location, 5, Brushes.Red)
        End Sub
    End Module
End Namespace
