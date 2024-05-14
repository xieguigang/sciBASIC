#Region "Microsoft.VisualBasic::fc39a545922a1503b3a6caac253c4d2e, gr\network-visualization\NetworkCanvas\Renderer.vb"

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

    '   Total Lines: 187
    '    Code Lines: 125
    ' Comment Lines: 29
    '   Blank Lines: 33
    '     File Size: 6.87 KB


    ' Class Renderer
    ' 
    '     Properties: ClientRegion, Font, ShowLabels, ZeroFilter
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+2 Overloads) GraphToScreen, ScreenToGraph
    ' 
    '     Sub: Clear, DirectDraw, doEdgeDrawing, drawEdge, drawNode
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce.Interfaces
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports stdNum = System.Math

Public Class Renderer : Inherits AbstractRenderer
    Implements IGraphicsEngine

    ''' <summary>
    ''' Gets the graphics source
    ''' </summary>
    Protected graphicsProvider As Func(Of Graphics)
    ''' <summary>
    ''' gets the graphics region for the projections: <see cref="GraphToScreen"/> and <see cref="ScreenToGraph"/>
    ''' </summary>
    Protected regionProvider As Func(Of Rectangle)

    Public ReadOnly Property ClientRegion As Rectangle
        Get
            Return regionProvider()
        End Get
    End Property

    Public Property ShowLabels As Boolean Implements IGraphicsEngine.ShowLabels

    ''' <summary>
    ''' 这个构造函数会生成一些静态数据的缓存
    ''' </summary>
    ''' <param name="canvas"></param>
    ''' <param name="regionProvider"></param>
    ''' <param name="iForceDirected"></param>
    Public Sub New(canvas As Func(Of Graphics), regionProvider As Func(Of Rectangle), iForceDirected As IForceDirected)
        MyBase.New(iForceDirected)

        Me.graphicsProvider = canvas
        Me.regionProvider = regionProvider

        ' using cache
        Dim ws As New Dictionary(Of Edge, Pen)
        Dim nr As New Dictionary(Of Node, Single)

        For Each edge As Edge In iForceDirected.graph.graphEdges
            If edge.data.style Is Nothing Then
                Dim w As Single = CSng(5.0! * edge.weight)
                w = If(w < 3.0!, 3.0!, w)
                Call ws.Add(edge, New Pen(Color.LightGray, w))
            Else
                Call ws.Add(edge, edge.data.style)
            End If
        Next
        For Each n As Node In iForceDirected.graph.vertex
            Dim r As Single = If(n.data.size.IsNullOrEmpty, 0, n.data.size(0))

            If r = 0! Then
                r = If(n.data.neighborhoods < 30,
                    n.data.neighborhoods * 9,
                    n.data.neighborhoods * 7)
                r = If(r = 0, 20, r)

                n.data.size = {r}
            End If

            Call nr.Add(n, r)
        Next

        edgeStyles = ws
        radiushash = nr
    End Sub

    Public Property ZeroFilter As Boolean = True

    Public Overrides Sub DirectDraw()
        forceDirected.EachEdge(AddressOf doEdgeDrawing)
        forceDirected.EachNode(Sub(node As Node, point As LayoutPoint) drawNode(node, point.position))
    End Sub

    Protected Sub doEdgeDrawing(edge As Edge, spring As Spring)
        If ZeroFilter Then
            If (edge.U.data.size(0) < 0.6 OrElse edge.V.data.size(0) < 0.6) Then
                Return
            ElseIf edge.U.data.size(0) > 500 OrElse edge.V.data.size(0) > 500 Then
                Return
            End If
        End If

        Call drawEdge(edge, spring.A.position, spring.B.position)
    End Sub

    Public Overrides Sub Clear()

    End Sub

    ''' <summary>
    ''' Projects the data model to our screen for display.
    ''' </summary>
    ''' <param name="iPos"></param>
    ''' <returns></returns>
    Public Shared Function GraphToScreen(iPos As FDGVector2, rect As Rectangle) As Point
        Dim x As Integer = CInt(stdNum.Truncate(iPos.x + (CSng(rect.Right - rect.Left) / 2.0F)))
        Dim y As Integer = CInt(stdNum.Truncate(iPos.y + (CSng(rect.Bottom - rect.Top) / 2.0F)))
        Return New Point(x, y)
    End Function

    Public Shared Function GraphToScreen(iPos As Point, rect As Rectangle) As Point
        Dim x As Integer = CInt(stdNum.Truncate(iPos.X + (CSng(rect.Right - rect.Left) / 2.0F)))
        Dim y As Integer = CInt(stdNum.Truncate(iPos.Y + (CSng(rect.Bottom - rect.Top) / 2.0F)))
        Return New Point(x, y)
    End Function

    ''' <summary>
    ''' Projects the client graphics data to the data model. 
    ''' </summary>
    ''' <param name="iScreenPos"></param>
    ''' <returns></returns>
    Public Function ScreenToGraph(iScreenPos As Point) As FDGVector2
        Dim retVec As New FDGVector2()
        Dim rect = regionProvider()
        retVec.x = CSng(iScreenPos.X) - (CSng(rect.Right - rect.Left) / 2.0F)
        retVec.y = CSng(iScreenPos.Y) - (CSng(rect.Bottom - rect.Top) / 2.0F)
        Return retVec
    End Function

    ''' <summary>
    ''' The edge drawing width cache
    ''' </summary>
    Protected edgeStyles As IReadOnlyDictionary(Of Edge, Pen)
    ''' <summary>
    ''' The node drawing radius cache
    ''' </summary>
    Protected radiushash As IReadOnlyDictionary(Of Node, Single)

    Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Dim rect As Rectangle = regionProvider()
        Dim pos1 As Point = GraphToScreen(TryCast(iPosition1, FDGVector2), rect)
        Dim pos2 As Point = GraphToScreen(TryCast(iPosition2, FDGVector2), rect)
        Dim canvas As Graphics = graphicsProvider()

        SyncLock canvas
            Try
                Call canvas.DrawLine(
                    edgeStyles(iEdge),
                    pos1.X,
                    pos1.Y,
                    pos2.X,
                    pos2.Y
                )
            Catch ex As Exception

            End Try
        End SyncLock
    End Sub

    Public Property Font As Font = New Font(FontFace.SegoeUI, 6, FontStyle.Regular)

    Protected Overrides Sub drawNode(n As Node, iPosition As AbstractVector)
        Dim pos As Point = GraphToScreen(TryCast(iPosition, FDGVector2), regionProvider())
        Dim canvas As Graphics = graphicsProvider()

        SyncLock canvas
            Dim r As Single = radiushash(n)
            Dim pt As New Point(CInt(pos.X - r / 2), CInt(pos.Y - r / 2))
            Dim rect As New Rectangle(pt, New Size(CInt(r), CInt(r)))

            Try
                Call canvas.FillPie(n.data.color, rect, 0, 360)
            Catch ex As Exception

            End Try

            If ShowLabels Then
                Dim center As Point = rect.Centre
                Dim labelText As String = n.data.label
                Dim sz As SizeF = canvas.MeasureString(labelText, Font)

                center = New Point(
                    CInt(center.X - sz.Width / 2),
                    CInt(center.Y - sz.Height / 2)
                )

                Call canvas.DrawString(labelText, Font, Brushes.Black, center)
            End If
        End SyncLock
    End Sub
End Class
