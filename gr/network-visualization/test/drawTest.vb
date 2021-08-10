#Region "Microsoft.VisualBasic::ee92784f50716526655e60a4fee5e812, gr\network-visualization\test\drawTest.vb"

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

' Module drawTest
' 
'     Function: DrawTest
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Cytoscape
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module drawTest

    Sub Main()
        Call drawKDTreeTest()
        'Call DrawTest()
        'Call Pause()

        'Dim graph = CytoscapeTableLoader.CytoscapeExportAsGraph("C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\net_test\xcb-main-Edges.csv", "C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\net_test\xcb-main-Nodes.csv")
        'Call graph.doForceLayout(iterations:=100, showProgress:=True)
        'Call graph.Tabular.Save("./")
        'Call graph.DrawImage("2000,2000").Save("./test.png")
        'Call vbnet.Save(graph.Tabular, "./network.vbnet")
    End Sub

    Private Function DrawTest()
        Dim net = vbnet.Load("C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\ModelTest\ModelTest\bin\Debug\network.vbnet")
        Dim graph = net.CreateGraph
        Call graph.DrawImage("2000,2000").Save("./test.png")
    End Function

    Sub drawKDTreeTest()
        Dim size As New Size(3600, 2700)
        Dim points2 As Point2D() = 1000.SeqRandom.Select(Function(i) New Point2D(randf.NextInteger(size.Width), randf.NextInteger(size.Height))).ToArray
        Dim points As FDGVector3() = {
             New FDGVector3(0, 0, 0),
             New FDGVector3(10, 10, 10),
             New FDGVector3(0, 9, 8),
             New FDGVector3(1, 9, 12),
             New FDGVector3(100, 100, 100),
             New FDGVector3(200, 200, 200)
        }


        Dim tree As New KdTree(Of FDGVector3)(points, New PointAccess3)

        Dim results = tree.nearest(New FDGVector3(-128, 0, 10), 3).ToArray

        Dim tree2 As New KdTree(Of Point2D)(points2, New PointAccess)
        Dim query = {
            New NamedValue(Of PointF)("1", points2.Random, "#009EFB"),
            New NamedValue(Of PointF)("1", points2.Random, "#55CE63"),
            New NamedValue(Of PointF)("1", points2.Random, "#F62D51"),
            New NamedValue(Of PointF)("1", points2.Random, "#FFBC37"),
            New NamedValue(Of PointF)("1", points2.Random, "#7460EE"),
            New NamedValue(Of PointF)("1", points2.Random, "#52E5DD"),
            New NamedValue(Of PointF)("1", points2.Random, "#984ea3"),
            New NamedValue(Of PointF)("1", points2.Random, "#ffff00")
        }

        Call DrawKDTree.Plot(tree2, query, k:=30, size:="1600,900", padding:="padding: 20px 20px 20px 20px;").Save("./test.png")

        ' Pause()
    End Sub
End Module

Public Class PointAccess3 : Inherits KdNodeAccessor(Of FDGVector3)

    Public Overrides Sub setByDimensin(x As FDGVector3, dimName As String, value As Double)
        Select Case dimName.ToLower
            Case "x" : x.x = value
            Case "y" : x.y = value
            Case "z" : x.z = value
        End Select
    End Sub

    Public Overrides Function GetDimensions() As String()
        Return {"x", "y", "z"}
    End Function

    Public Overrides Function metric(a As FDGVector3, b As FDGVector3) As Double
        Return {a.x, a.y, a.z}.EuclideanDistance({b.x, b.y, b.z})
    End Function

    Public Overrides Function getByDimension(x As FDGVector3, dimName As String) As Double
        Select Case dimName.ToLower
            Case "x" : Return x.x
            Case "y" : Return x.y
            Case "z" : Return x.z
        End Select
    End Function

    Public Overrides Function nodeIs(a As FDGVector3, b As FDGVector3) As Boolean
        Return a Is b
    End Function

    Public Overrides Function activate() As FDGVector3
        Return New FDGVector3
    End Function
End Class

Public Class PointAccess : Inherits KdNodeAccessor(Of Point2D)

    Public Overrides Sub setByDimensin(x As Point2D, dimName As String, value As Double)
        If dimName.TextEquals("x") Then
            x.X = value
        Else
            x.Y = value
        End If
    End Sub

    Public Overrides Function GetDimensions() As String()
        Return {"x", "y"}
    End Function

    Public Overrides Function metric(a As Point2D, b As Point2D) As Double
        Return a.DistanceTo(b)
    End Function

    Public Overrides Function getByDimension(x As Point2D, dimName As String) As Double
        If dimName.TextEquals("x") Then
            Return x.X
        Else
            Return x.Y
        End If
    End Function

    Public Overrides Function nodeIs(a As Point2D, b As Point2D) As Boolean
        Return a Is b
    End Function

    Public Overrides Function activate() As Point2D
        Return New Point2D
    End Function
End Class