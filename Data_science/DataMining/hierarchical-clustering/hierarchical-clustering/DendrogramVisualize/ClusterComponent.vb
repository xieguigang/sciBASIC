#Region "Microsoft.VisualBasic::b84e2e514c3a2addc2bda973f1aaf54d, ..\sciBASIC#\Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DendrogramVisualize\ClusterComponent.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Text
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

    Public Class ClusterComponent
        Implements IPaintable

        Public Property Children As New List(Of ClusterComponent)
        Public Property NamePadding As Integer = 6
        ''' <summary>
        ''' 点的大小
        ''' </summary>
        ''' <returns></returns>
        Public Property DotRadius As Integer = 5
        Public Property LinkPoint As PointF
        Public Property InitPoint As PointF
        Public Property Cluster As Cluster
        Public Property PrintName As Boolean

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
        Public Sub paint(g As Graphics2D, args As PainterArguments) Implements IPaintable.paint
            Dim x1, y1, x2, y2 As Integer
            Dim fontMetrics As FontMetrics = g.FontMetrics

            With args

                x1 = CInt(Fix(InitPoint.X * .xDisplayFactor + .xDisplayOffset))
                y1 = CInt(Fix(InitPoint.Y * .yDisplayFactor + .yDisplayOffset))
                x2 = CInt(Fix(LinkPoint.X * .xDisplayFactor + .xDisplayOffset))
                y2 = y1
                g.FillEllipse(Brushes.Black, x1 - DotRadius, y1 - DotRadius, DotRadius * 2, DotRadius * 2)
                g.DrawLine(.stroke, x1, y1, x2, y2)

                If Cluster.Leaf Then
                    Dim nx! = x1 + NamePadding
                    Dim ny! = y1

                    ' 绘制叶节点
                    g.DrawString(Cluster.Name, fontMetrics, Brushes.Black, nx, y1 - (fontMetrics.Height / 2) - 2)

                    If Not .classTable Is Nothing Then
                        ' 如果还存在分类信息的话，会绘制分类的颜色条
                        Dim color As Brush = .classTable(Cluster.Name).GetBrush
                        Dim topleft As New PointF(nx + .classLegendPadding, y1 - .classLegendSize.Height / 2)
                        Dim rect As New RectangleF(topleft, .classLegendSize)

                        g.FillRectangle(color, rect)
                    End If
                End If
                If .decorated AndAlso Cluster.Distance IsNot Nothing AndAlso (Not Cluster.Distance.NaN) AndAlso Cluster.Distance.Distance > 0 Then
                    Dim s As String = String.Format("{0:F2}", Cluster.Distance)
                    Dim rect As RectangleF = fontMetrics.GetStringBounds(s, g.Graphics)
                    g.DrawString(s, fontMetrics, Brushes.Black, x1 - CInt(Fix(rect.Width)), y1 - 2 - rect.Height)
                End If

                x1 = x2
                y1 = y2
                y2 = CInt(Fix(LinkPoint.Y * .yDisplayFactor + .yDisplayOffset))
                g.DrawLine(.stroke, x1, y1, x2, y2)

                For Each child As ClusterComponent In Children
                    child.paint(g, args)
                Next
            End With
        End Sub

        Public ReadOnly Property RectMinX As Double
            Get

                ' TODO Better use closure / callback here
                '  Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = sys.Min(InitPoint.X, LinkPoint.X)
                For Each child As ClusterComponent In Children
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
                For Each child As ClusterComponent In Children
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
                For Each child As ClusterComponent In Children
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
                For Each child As ClusterComponent In Children
                    val = Math.Max(val, child.RectMaxY)
                Next
                Return val
            End Get
        End Property

        Public Function getNameWidth(g As Graphics2D, includeNonLeafs As Boolean) As Integer
            Dim width As Integer = 0
            If includeNonLeafs OrElse Cluster.Leaf Then
                Dim rect As RectangleF = g.FontMetrics.GetStringBounds(Cluster.Name, g.Graphics)
                width = CInt(Fix(rect.Width))
            End If
            Return width
        End Function

        Public Function GetMaxNameWidth(g As Graphics2D, includeNonLeafs As Boolean) As Integer
            Dim width As Integer = getNameWidth(g, includeNonLeafs)
            For Each comp As ClusterComponent In Children
                Dim childWidth As Integer = comp.GetMaxNameWidth(g, includeNonLeafs)
                If childWidth > width Then width = childWidth
            Next comp
            Return width
        End Function
    End Class

End Namespace
