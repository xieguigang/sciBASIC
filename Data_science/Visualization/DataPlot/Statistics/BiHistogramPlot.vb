#Region "Microsoft.VisualBasic::1a5d8f3c7e2b4906b4c1a9d7e3f5b2c8, Data_science\Visualization\DataPlot\Statistics\BiHistogramPlot.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Class BiHistogramPlot
    ' 
    '     Properties: SampleA, SampleB
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Plot
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq

' ============================================================================
'  BiHistogramPlot.vb - 双向（镜像）直方图
'
'  旧项目里的 Bihistogram.vb 是空桩（Plot 方法直接返回 Nothing）。
'  这里按标准双直方图实现：两个样本共用同一组分箱，一个向上、一个向下，
'  可以直接比较两个分布的中心位置与离散程度。
' ============================================================================

''' <summary>双向直方图（比较两个分布）</summary>
Public Class BiHistogramPlot
    Inherits HistogramPlot

    ''' <summary>向上绘制的样本</summary>
    Public Property SampleA As BiHistogramSample = Nothing
    ''' <summary>向下绘制的样本</summary>
    Public Property SampleB As BiHistogramSample = Nothing

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
        MyBase.Mirrored = True
    End Sub

    Public Sub PlotBihistogram()
        If SampleA Is Nothing OrElse SampleB Is Nothing Then
            Throw New InvalidOperationException("BiHistogramPlot requires two samples.")
        End If
        If SampleA.Data Is Nothing OrElse SampleA.Data.Length = 0 OrElse
           SampleB.Data Is Nothing OrElse SampleB.Data.Length = 0 Then
            Throw New InvalidOperationException("Both bi-histogram samples must contain data.")
        End If

        If SampleA.Color Is Nothing Then SampleA.Color = Theme.Palette(0)
        If SampleB.Color Is Nothing Then SampleB.Color = Theme.Palette(1)

        MyBase.Groups = New List(Of CategoryGroup) From {
            New CategoryGroup With {.Name = SampleA.Name, .Data = SampleA.Data, .Color = SampleA.Color},
            New CategoryGroup With {.Name = SampleB.Name, .Data = SampleB.Data, .Color = SampleB.Color}
        }
        MyBase.Mirrored = True
        MyBase.Bins = If(Me.Bins > 0, Me.Bins, 30)
        MyBase.Density = False

        Call MyBase.Plot()
    End Sub
End Class
