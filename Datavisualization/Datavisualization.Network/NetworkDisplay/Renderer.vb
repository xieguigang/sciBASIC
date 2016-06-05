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

        Call canvas.DrawLine(
            New Pen(brush:=Brushes.Gray),
            pos1.first,
            pos1.second,
            pos2.first,
            pos2.second)
    End Sub

    Public Sub DrawBox(iNode As Node, iPosition As AbstractVector)
        Dim pos As Pair(Of Integer, Integer) = GraphToScreen(TryCast(iPosition, FDGVector2))
        Dim canvas As Graphics = __graphicsProvider()

        Call canvas.DrawPie(New Pen(Brushes.Green), New Rectangle(New Point(pos.first, pos.second), New Size(20, 20)), 0, 360)
    End Sub
End Class
