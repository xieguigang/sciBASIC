Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts
Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts.Interfaces

Public Class Renderer
    Inherits AbstractRenderer

    Dim __graphicsProvider As Func(Of Graphics)
    Dim __regionProvider As Func(Of Rectangle)

    Public Sub New(canvas As Func(Of Graphics), regionProvider As Func(Of Rectangle), iForceDirected As IForceDirected)
        MyBase.New(iForceDirected)
        __graphicsProvider = canvas
        __regionProvider = regionProvider
    End Sub

    Public Overrides Sub Clear()

    End Sub

    Public Function GraphToScreen(iPos As FDGVector2) As Pair(Of Integer, Integer)
        Dim retPair As New Pair(Of Integer, Integer)()
        Dim rect = __regionProvider()
        retPair.first = CInt(Math.Truncate(iPos.x + (CSng(rect.Right - rect.Left) / 2.0F)))
        retPair.second = CInt(Math.Truncate(iPos.y + (CSng(rect.Bottom - rect.Top) / 2.0F)))
        Return retPair
    End Function

    Public Function ScreenToGraph(iScreenPos As Pair(Of Integer, Integer)) As FDGVector2
        Dim retVec As New FDGVector2()
        Dim rect = __regionProvider()
        retVec.x = CSng(iScreenPos.first) - (CSng(rect.Right - rect.Left) / 2.0F)
        retVec.y = CSng(iScreenPos.second) - (CSng(rect.Bottom - rect.Top) / 2.0F)
        Return retVec
    End Function

    Protected Overrides Sub drawEdge(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Call DrawLine(iEdge, iPosition1, iPosition2)
    End Sub

    Protected Overrides Sub drawNode(iNode As Node, iPosition As AbstractVector)
        Call DrawBox(iNode, iPosition)
    End Sub

    Public Sub DrawLine(iEdge As Edge, iPosition1 As AbstractVector, iPosition2 As AbstractVector)
        Dim pos1 As Pair(Of Integer, Integer) = GraphToScreen(TryCast(iPosition1, FDGVector2))
        Dim pos2 As Pair(Of Integer, Integer) = GraphToScreen(TryCast(iPosition2, FDGVector2))
        Dim canvas As Graphics = __graphicsProvider()
        Dim w As Integer = 5 * iEdge.Data.weight
        w = If(w < 1.5, 1.5, w)
        Dim LineColor As New Pen(Color.Gray, w)

        Call canvas.DrawLine(
            LineColor,
            pos1.first,
            pos1.second,
            pos2.first,
            pos2.second)
    End Sub

    Public Sub DrawBox(n As Node, iPosition As AbstractVector)
        Dim pos As Pair(Of Integer, Integer) = GraphToScreen(TryCast(iPosition, FDGVector2))
        Dim canvas As Graphics = __graphicsProvider()
        Dim r As Single = n.Data.radius

        If r = 0! Then
            r = If(n.Data.Neighborhoods < 30, n.Data.Neighborhoods * 9, n.Data.Neighborhoods * 7)
            r = If(r = 0, 20, r)
        End If

        Dim br As New SolidBrush(If(n.Data.Color.IsEmpty, Color.Black, n.Data.Color))
        Dim pt As New Point(pos.first - r / 2, pos.second - r / 2)
        Dim rect As New Rectangle(pt, New Size(r, r))

        Call canvas.FillPie(br, rect, 0, 360)
    End Sub
End Class
