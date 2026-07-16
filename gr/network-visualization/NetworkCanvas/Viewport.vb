' /********************************************************************************/
'
' 2D 视口变换与渲染视图状态
'
' 该文件定义了网络图在 2D 视图中的缩放/平移变换（Viewport）以及
' Canvas 与 Renderer 之间共享的视图状态（CanvasViewState），用于控制
' 悬停/选中高亮、LOD（细节层次）与背景网格。
'
' /********************************************************************************/

Imports System.Collections.Generic
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

''' <summary>
''' 2D 视口变换：缩放(zoom)与平移(pan)，在绘图坐标与屏幕坐标之间互转。
''' </summary>
Public Class Viewport

    Public Const DefaultZoom As Single = 1.0!

    ''' <summary>
    ''' 缩放因子，1 表示原始比例。
    ''' </summary>
    Public zoom As Single = DefaultZoom
    ''' <summary>
    ''' 屏幕像素平移量（向右/向下为正）。
    ''' </summary>
    Public panX As Single = 0
    Public panY As Single = 0

    Const MinZoom As Single = 0.02!
    Const MaxZoom As Single = 50.0!

    ''' <summary>
    ''' 将绘图坐标转换为屏幕坐标（含缩放与平移）。
    ''' </summary>
    Public Function ToScreen(gx As Single, gy As Single, rect As Rectangle) As PointF
        Dim sx As Single = rect.Width / 2.0F + gx * zoom + panX
        Dim sy As Single = rect.Height / 2.0F + gy * zoom + panY
        Return New PointF(sx, sy)
    End Function

    ''' <summary>
    ''' 将屏幕坐标转换为绘图坐标（ToScreen 的逆变换）。
    ''' </summary>
    Public Function ToGraph(sx As Single, sy As Single, rect As Rectangle) As FDGVector2
        Dim gx As Single = (sx - panX - rect.Width / 2.0F) / zoom
        Dim gy As Single = (sy - panY - rect.Height / 2.0F) / zoom
        Return New FDGVector2(gx, gy)
    End Function

    ''' <summary>
    ''' 以屏幕锚点(anchor)为中心进行缩放，保证锚点下的图形点保持不动，避免视图“漂移”。
    ''' </summary>
    Public Sub ZoomAt(anchor As Point, factor As Single, rect As Rectangle)
        Dim gx As Single = (anchor.X - panX - rect.Width / 2.0F) / zoom
        Dim gy As Single = (anchor.Y - panY - rect.Height / 2.0F) / zoom

        zoom = System.Math.Min(MaxZoom, System.Math.Max(MinZoom, zoom * factor))

        panX = anchor.X - rect.Width / 2.0F - gx * zoom
        panY = anchor.Y - rect.Height / 2.0F - gy * zoom
    End Sub

    ''' <summary>
    ''' 平移视图。
    ''' </summary>
    Public Sub Pan(dx As Single, dy As Single)
        panX += dx
        panY += dy
    End Sub

    ''' <summary>
    ''' 复位到默认（无缩放、无平移）。
    ''' </summary>
    Public Sub Reset()
        zoom = DefaultZoom
        panX = 0
        panY = 0
    End Sub
End Class

''' <summary>
''' 渲染视图状态：在 Canvas 与 Renderer 之间共享，控制视口、悬停/选中高亮、LOD 与网格。
''' </summary>
Public Class CanvasViewState

    Public ReadOnly Viewport As New Viewport

    Public Property Hovered As Node
    Public Property Selected As New HashSet(Of Node)
    Public Property ShowGrid As Boolean = True
    Public Property ShowLabels As Boolean = True

    ' LOD 阈值
    Public Property LabelZoomThreshold As Single = 0.5!
    Public Property EdgeZoomThreshold As Single = 0.25!
    Public Property MaxLabelNodes As Integer = 4000

    ''' <summary>
    ''' 是否应当绘制节点标签（依据缩放级别与节点总数）。
    ''' </summary>
    Public Function ShouldDrawLabels(nodeCount As Integer, zoom As Single) As Boolean
        If Not ShowLabels Then
            Return False
        End If
        If zoom < LabelZoomThreshold Then
            Return False
        End If
        If nodeCount > MaxLabelNodes Then
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' 是否应当绘制边（缩放过小时可隐藏次要边）。
    ''' </summary>
    Public Function ShouldDrawEdges(zoom As Single) As Boolean
        Return zoom >= EdgeZoomThreshold
    End Function

    ''' <summary>
    ''' 该节点是否处于高亮状态（悬停或选中）。
    ''' </summary>
    Public Function IsHighlighted(n As Node) As Boolean
        If n Is Nothing Then
            Return False
        End If
        Return (Hovered Is n) OrElse Selected.Contains(n)
    End Function
End Class
