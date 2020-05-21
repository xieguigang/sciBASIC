#Region "Microsoft.VisualBasic::cc8d78761ce9b59a11f7ce6d4894c20c, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DendrogramVisualize\ClusterComponent.vb"

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

    '     Class ClusterComponent
    ' 
    '         Properties: Children, Cluster, InitPoint, LinkPoint, NamePadding
    '                     PrintName, RectMaxX, RectMaxY, RectMinX, RectMinY
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetMaxNameWidth, getNameWidth
    ' 
    '         Sub: Paint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

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
        Public Property LinkPoint As PointF
        Public Property InitPoint As PointF
        Public Property Cluster As Cluster
        Public Property PrintName As Boolean

#Region "layout property"

        Public ReadOnly Property RectMinX As Double
            Get

                ' TODO Better use closure / callback here
                '  Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = stdNum.Min(InitPoint.X, LinkPoint.X)
                For Each child As ClusterComponent In Children
                    val = stdNum.Min(val, child.RectMinX)
                Next
                Return val
            End Get
        End Property

        Public ReadOnly Property RectMinY As Double
            Get

                ' TODO Better use closure here
                ' Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = stdNum.Min(InitPoint.Y, LinkPoint.Y)
                For Each child As ClusterComponent In Children
                    val = stdNum.Min(val, child.RectMinY)
                Next
                Return val
            End Get
        End Property

        Public ReadOnly Property RectMaxX As Double
            Get

                ' TODO Better use closure here
                ' Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = stdNum.Max(InitPoint.X, LinkPoint.X)
                For Each child As ClusterComponent In Children
                    val = stdNum.Max(val, child.RectMaxX)
                Next
                Return val
            End Get
        End Property

        Public ReadOnly Property RectMaxY As Double
            Get

                ' TODO Better use closure here
                '  Debug.Assert(InitPoint IsNot Nothing AndAlso LinkPoint IsNot Nothing)
                Dim val As Double = stdNum.Max(InitPoint.Y, LinkPoint.Y)
                For Each child As ClusterComponent In Children
                    val = stdNum.Max(val, child.RectMaxY)
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
        ''' <remarks>
        ''' 在进行绘制的时候，默认的布局样式是竖直样式的
        ''' 对于绘制水平方向的层次聚类树，则只需要将竖直布局样式的的点的x, y交换一下即可
        ''' 对于弧形布局的层次聚类树的绘制，则是将竖直样式的点的y映射为圆弧的度，x映射为圆弧的半径即可
        ''' </remarks>
        Public Sub Paint(g As Graphics2D, args As PainterArguments, ByRef labels As List(Of NamedValue(Of PointF))) Implements IPaintable.Paint
            Dim x1, y1, x2, y2 As Integer
            Dim fontMetrics As FontMetrics = g.FontMetrics

            With args

                If .layout = Layouts.Vertical Then
                    x1 = CInt(Fix(InitPoint.X * .xDisplayFactor + .xDisplayOffset))
                    y1 = CInt(Fix(InitPoint.Y * .yDisplayFactor + .yDisplayOffset))
                    x2 = CInt(Fix(LinkPoint.X * .xDisplayFactor + .xDisplayOffset))
                    y2 = y1  ' 只变化X，Y不变，表示树枝在竖直布局下的水平延伸
                Else
                    y1 = CInt(Fix(InitPoint.X * .xDisplayFactor + .yDisplayOffset))
                    x1 = CInt(Fix(InitPoint.Y * .yDisplayFactor + .xDisplayOffset))
                    y2 = CInt(Fix(LinkPoint.X * .xDisplayFactor + .yDisplayOffset))
                    x2 = x1  ' 只变化Y，X不变，表示树枝在水平布局下的竖直延伸
                End If

                If .LinkDotRadius > 0 Then
                    Dim dotRadius = .LinkDotRadius
                    Dim d% = dotRadius * 2

                    g.FillEllipse(Brushes.Black, x1 - dotRadius, y1 - dotRadius, d, d)
                End If

                g.DrawLine(.stroke, x1, y1, x2, y2)

                If Cluster.Leaf Then

                    ' 如果目标是叶节点才会进行标签字符串的绘制操作
                    Dim nx!
                    Dim ny!

                    If .layout = Layouts.Vertical Then
                        nx = x1 + NamePadding
                        ny = y1 - (fontMetrics.Height / 2) - 2
                    Else
                        nx = x1 - g.MeasureString(Cluster.Name, g.Font).Width / 2
                        ny = y1 + 5
                    End If

                    Dim location As New PointF With {
                        .X = nx,
                        .Y = ny
                    }

                    ' 绘制叶节点
                    If args.ShowLabelName Then
                        g.DrawString(Cluster.Name, fontMetrics, Brushes.Black, location)
                    End If

                    labels += New NamedValue(Of PointF) With {
                        .Name = Cluster.Name,
                        .Value = location
                    }

                    If Not .classTable Is Nothing Then

                        ' 如果还存在分类信息的话，会绘制分类的颜色条
                        Dim color As Brush = .classTable(Cluster.Name).GetBrush
                        Dim topleft As PointF

                        If .layout = Layouts.Vertical Then
                            topleft = New PointF(nx + .classLegendPadding, y1 - .classLegendSize.Height / 2)
                        Else
                            topleft = New PointF(x1 - .classLegendSize.Width / 2, y1 + .classLegendPadding)
                        End If

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

                ' 进行递归绘制
                x1 = x2
                y1 = y2

                If .layout = Layouts.Vertical Then
                    ' 变y，表示树枝在递归绘图的时候在竖直方向上延伸
                    y2 = CInt(Fix(LinkPoint.Y * .yDisplayFactor + .yDisplayOffset))
                Else
                    ' 变X，表示树枝在递归绘图的时候在水平方向上延伸
                    x2 = CInt(Fix(LinkPoint.Y * .xDisplayFactor + .yDisplayOffset))
                End If

                g.DrawLine(.stroke, x1, y1, x2, y2)

                ' 进行递归绘制
                For Each child As ClusterComponent In Children
                    child.Paint(g, args, labels)
                Next
            End With
        End Sub

        Private Function getNameWidth(g As Graphics2D, includeNonLeafs As Boolean) As Integer
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

                If childWidth > width Then
                    width = childWidth
                End If
            Next

            Return width
        End Function
    End Class
End Namespace
