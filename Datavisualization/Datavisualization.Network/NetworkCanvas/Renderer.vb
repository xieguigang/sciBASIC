Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts.Interfaces

Public Class Renderer
    Inherits AbstractRenderer

    ''' <summary>
    ''' Gets the graphics source
    ''' </summary>
    Protected __graphicsProvider As Func(Of Graphics)
    ''' <summary>
    ''' gets the graphics region for the projections: <see cref="GraphToScreen"/> and <see cref="ScreenToGraph"/>
    ''' </summary>
    Protected __regionProvider As Func(Of Rectangle)

    Public ReadOnly Property ClientRegion As Rectangle
        Get
            Return __regionProvider()
        End Get
    End Property

    ''' <summary>
    ''' 这个构造函数会生成一些静态数据的缓存
    ''' </summary>
    ''' <param name="canvas"></param>
    ''' <param name="regionProvider"></param>
    ''' <param name="iForceDirected"></param>
    Public Sub New(canvas As Func(Of Graphics), regionProvider As Func(Of Rectangle), iForceDirected As IForceDirected)
        MyBase.New(iForceDirected)
        __graphicsProvider = canvas
        __regionProvider = regionProvider

        ' using cache
        Dim ws As New Dictionary(Of Edge, Single)
        Dim nr As New Dictionary(Of Node, Single)

        For Each edge As Edge In iForceDirected.graph.edges
            Dim w As Single = CSng(5.0! * edge.Data.weight)
            w = If(w < 1.5!, 1.5!, w)
            Call ws.Add(edge, w)
        Next
        For Each n As Node In iForceDirected.graph.nodes
            Dim r As Single = n.Data.radius
            If r = 0! Then
                r = If(n.Data.Neighborhoods < 30,
                    n.Data.Neighborhoods * 9,
                    n.Data.Neighborhoods * 7)
                r = If(r = 0, 20, r)
            End If
            Call nr.Add(n, r)
        Next

        widthHash = ws
        radiushash = nr
    End Sub

    Public Overrides Sub Clear()

    End Sub

    ''' <summary>
    ''' Projects the data model to our screen for display.
    ''' </summary>
    ''' <param name="iPos"></param>
    ''' <returns></returns>
    Public Shared Function GraphToScreen(iPos As FDGVector2, rect As Rectangle) As Point
        Dim x As Integer = CInt(Math.Truncate(iPos.x + (CSng(rect.Right - rect.Left) / 2.0F)))
        Dim y As Integer = CInt(Math.Truncate(iPos.y + (CSng(rect.Bottom - rect.Top) / 2.0F)))
        Return New Point(x, y)
    End Function

    Public Shared Function GraphToScreen(iPos As Point, rect As Rectangle) As Point
        Dim x As Integer = CInt(Math.Truncate(iPos.X + (CSng(rect.Right - rect.Left) / 2.0F)))
        Dim y As Integer = CInt(Math.Truncate(iPos.Y + (CSng(rect.Bottom - rect.Top) / 2.0F)))
        Return New Point(x, y)
    End Function

    ''' <summary>
    ''' Projects the client graphics data to the data model. 
    ''' </summary>
    ''' <param name="iScreenPos"></param>
    ''' <returns></returns>
    Public Function ScreenToGraph(iScreenPos As Point) As FDGVector2
        Dim retVec As New FDGVector2()
        Dim rect = __regionProvider()
        retVec.x = CSng(iScreenPos.X) - (CSng(rect.Right - rect.Left) / 2.0F)
        retVec.y = CSng(iScreenPos.Y) - (CSng(rect.Bottom - rect.Top) / 2.0F)
        Return retVec
    End Function

    ''' <summary>
    ''' The edge drawing width cache
    ''' </summary>
    Protected widthHash As IReadOnlyDictionary(Of Edge, Single)
    ''' <summary>
    ''' The node drawing radius cache
    ''' </summary>
    Protected radiushash As IReadOnlyDictionary(Of Node, Single)

    Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Dim rect As Rectangle = __regionProvider()
        Dim pos1 As Point = GraphToScreen(TryCast(iPosition1, FDGVector2), rect)
        Dim pos2 As Point = GraphToScreen(TryCast(iPosition2, FDGVector2), rect)
        Dim canvas As Graphics = __graphicsProvider()

        SyncLock canvas
            Dim w As Single = widthHash(iEdge)
            Dim LineColor As New Pen(Color.Gray, w)

            Call canvas.DrawLine(
                LineColor,
                pos1.X,
                pos1.Y,
                pos2.X,
                pos2.Y)
        End SyncLock
    End Sub

    Protected Overrides Sub drawNode(n As Node, iPosition As AbstractVector)
        Dim pos As Point = GraphToScreen(TryCast(iPosition, FDGVector2), __regionProvider())
        Dim canvas As Graphics = __graphicsProvider()

        SyncLock canvas
            Dim r As Single = radiushash(n)
            Dim pt As New Point(CInt(pos.X - r / 2), CInt(pos.Y - r / 2))
            Dim rect As New Rectangle(pt, New Size(CInt(r), CInt(r)))

            Call canvas.FillPie(n.Data.Color, rect, 0, 360)
        End SyncLock
    End Sub
End Class
