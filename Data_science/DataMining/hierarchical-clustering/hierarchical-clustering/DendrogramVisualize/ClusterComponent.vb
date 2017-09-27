#Region "Microsoft.VisualBasic::422d17dbe0761b9ee40eee04db13a6a5, ..\sciBASIC#\Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DendrogramVisualize\ClusterComponent.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Language
Imports sys = System.Math

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Namespace DendrogramVisualize

    ''' <summary>
    ''' 树
    ''' </summary>
    Public Class ClusterComponent : Inherits AbstractTree(Of ClusterComponent)
        Implements IPaintable

        Public Property NamePadding As Integer = 6
        Public Property LinkPoint As PointF
        Public Property InitPoint As PointF
        Public Property Cluster As Cluster
        Public Property PrintName As Boolean

#Region "Layout"

        Public ReadOnly Property RectMinX As Double
            Get

                ' TODO Better use closure / callback here
                '  Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = sys.Min(InitPoint.X, LinkPoint.X)
                For Each child As ClusterComponent In Childs
                    val = sys.Min(val, child.RectMinX)
                Next
                Return val
            End Get
        End Property

        Public ReadOnly Property RectMinY As Double
            Get

                ' TODO Better use closure here
                ' Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = sys.Min(InitPoint.Y, LinkPoint.Y)
                For Each child As ClusterComponent In Childs
                    val = sys.Min(val, child.RectMinY)
                Next
                Return val
            End Get
        End Property

        Public ReadOnly Property RectMaxX As Double
            Get

                ' TODO Better use closure here
                ' Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = Math.Max(InitPoint.X, LinkPoint.X)
                For Each child As ClusterComponent In Childs
                    val = Math.Max(val, child.RectMaxX)
                Next
                Return val
            End Get
        End Property

        Public ReadOnly Property RectMaxY As Double
            Get

                ' TODO Better use closure here
                '  Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = Math.Max(InitPoint.Y, LinkPoint.Y)
                For Each child As ClusterComponent In Childs
                    val = Math.Max(val, child.RectMaxY)
                Next
                Return val
            End Get
        End Property
#End Region

        Public Sub New(cluster As Cluster, printName As Boolean, initPoint As PointF)
            Me.PrintName = printName
            Me.Cluster = cluster
            Me.InitPoint = initPoint
            Me.LinkPoint = initPoint
        End Sub

        ''' <summary>
        ''' 绘制具体的聚类结果
        ''' </summary>
        ''' <param name="g"></param>
        Public Sub paint(g As Graphics2D, args As PainterArguments, ByRef labels As List(Of NamedValue(Of PointF))) Implements IPaintable.paint
            Dim x1, y1, x2, y2 As Integer
            Dim fontMetrics As FontMetrics = g.FontMetrics

            With args

                x1 = CInt(Fix(InitPoint.X * .xDisplayFactor + .xDisplayOffset))
                y1 = CInt(Fix(InitPoint.Y * .yDisplayFactor + .yDisplayOffset))
                x2 = CInt(Fix(LinkPoint.X * .xDisplayFactor + .xDisplayOffset))
                y2 = y1

                If .LinkDotRadius > 0 Then
                    Dim dotRadius = .LinkDotRadius
                    Dim d% = dotRadius * 2
                    g.FillEllipse(Brushes.Black, x1 - dotRadius, y1 - dotRadius, d, d)
                End If
                g.DrawLine(.stroke, x1, y1, x2, y2)

                If Cluster.IsLeaf Then
                    Dim nx! = x1 + NamePadding
                    Dim ny! = y1
                    Dim location As New PointF With {
                        .X = nx,
                        .Y = y1 - (fontMetrics.Height / 2) - 2
                    }

                    ' 绘制叶节点
                    If args.ShowLabelName Then
                        g.DrawString(Cluster.Label, fontMetrics, Brushes.Black, location)
                    End If
                    labels += New NamedValue(Of PointF) With {
                        .Name = Cluster.Label,
                        .Value = location
                    }

                    If Not .classTable Is Nothing Then
                        ' 如果还存在分类信息的话，会绘制分类的颜色条
                        Dim color As Brush = .classTable(Cluster.Label).GetBrush
                        Dim topleft As New PointF(nx + .classLegendPadding, y1 - .classLegendSize.Height / 2)
                        Dim rect As New RectangleF(topleft, .classLegendSize)

                        g.FillRectangle(color, rect)
                    End If
                End If
                If .decorated AndAlso
                    Cluster.Distance IsNot Nothing AndAlso
                    (Not Cluster.Distance.NaN) AndAlso
                    Cluster.Distance.Distance > 0 Then

                    Dim s As String = String.Format("{0:F2}", Cluster.Distance)
                    Dim rect As RectangleF = fontMetrics.GetStringBounds(s, g.Graphics)
                    Dim location As New PointF With {
                        .X = x1 - CInt(Fix(rect.Width)),
                        .Y = y1 - 2 - rect.Height
                    }

                    g.DrawString(s, fontMetrics, Brushes.Black, location)
                End If

                x1 = x2
                y1 = y2
                y2 = CInt(Fix(LinkPoint.Y * .yDisplayFactor + .yDisplayOffset))
                g.DrawLine(.stroke, x1, y1, x2, y2)

                For Each child As ClusterComponent In Childs
                    child.paint(g, args, labels)
                Next
            End With
        End Sub

        Public Function GetNameWidth(g As Graphics2D, includeNonLeafs As Boolean) As Integer
            Dim width As Integer = 0
            If includeNonLeafs OrElse Cluster.IsLeaf Then
                Dim rect As RectangleF = g.FontMetrics.GetStringBounds(Cluster.Label, g.Graphics)
                width = CInt(Fix(rect.Width))
            End If
            Return width
        End Function

        Public Function GetMaxNameWidth(g As Graphics2D, includeNonLeafs As Boolean) As Integer
            Dim width As Integer = GetNameWidth(g, includeNonLeafs)
            For Each comp As ClusterComponent In Childs
                Dim childWidth As Integer = comp.GetMaxNameWidth(g, includeNonLeafs)
                If childWidth > width Then width = childWidth
            Next comp
            Return width
        End Function
    End Class

End Namespace
