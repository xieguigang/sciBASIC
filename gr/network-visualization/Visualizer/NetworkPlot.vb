Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports std = System.Math

''' <summary>
''' 网络图渲染类：承载网络模型与渲染配置，对外暴露 <see cref="Render"/> 产出图像。
''' 
''' 该类对应于 <see cref="NetworkVisualizer.DrawImage"/> 原来的绘图主流程，
''' 将坐标换算、边/节点/标签/凸包多边形的绘制逻辑收敛为职责单一的渲染对象，
''' 参考同模块内 <c>DrawKDTree</c> 的“配置对象 + 渲染类 + 薄外壳”范式，
''' 但不继承自 <c>Plot</c> 以避免引入 ChartPlots 主题耦合。
''' </summary>
Public Class NetworkPlot

    ReadOnly net As NetworkGraph
    ReadOnly config As NetworkRenderConfig

    ''' <summary>
    ''' 构造一个网络图渲染对象
    ''' </summary>
    ''' <param name="net">待绘制的网络图模型</param>
    ''' <param name="config">渲染配置（默认值与 <see cref="NetworkVisualizer.DrawImage"/> 的 Optional 参数一致）</param>
    Sub New(net As NetworkGraph, config As NetworkRenderConfig)
        Me.net = net
        Me.config = config
    End Sub

    ''' <summary>
    ''' 执行网络图的可视化绘制并返回图像数据
    ''' </summary>
    ''' <returns></returns>
    Public Function Render() As GraphicsData
        Call GetType(NetworkVisualizer).Assembly _
            .FromAssembly _
            .DoCall(Sub(assm)
                        Dim driverPrompt$ = "
 Current graphic driver is pixel based gdi+ engine, and you could change the graphics driver 
 to vector based graphic engine via config in commandline:

    tool /command [...arguments] /@set ""graphic_driver=svg/ps"""

                        If g.ActiveDriver <> Drivers.GDI AndAlso g.ActiveDriver <> Drivers.Default Then
                            driverPrompt = ""
                        End If

                        Call VBDebugger.WaitOutput()
                        Call CLITools.AppSummary(assm, "Welcome to use network graph visualizer api from sciBASIC.NET framework.", driverPrompt, App.StdOut)
                        Call VBDebugger.WriteLine("")
                    End Sub)

        ' 所绘制的图像输出的尺寸大小
        Dim frameSize As SizeF = PrinterDimension.SizeOf(config.CanvasSize)
        Dim margin As Padding = CSS.Padding.TryParse(
            config.Padding, [default]:=New Padding With {
                .Bottom = 100,
                .Left = 100,
                .Right = 100,
                .Top = 100
            })

        Call $"Canvas size expression '{config.CanvasSize}' = [{frameSize.Width}, {frameSize.Height}]".debug
        Call $"Canvas padding [{margin.Top}, {margin.Right}, {margin.Bottom}, {margin.Left}]".debug

        ' 1. 先将网络图形对象置于输出的图像的中心位置
        ' 2. 进行矢量图放大
        ' 3. 执行绘图操作

        ' 获取得到当前的这个网络对象相对于图像的中心点的位移值
        Dim scalePos As Dictionary(Of String, PointF) = CanvasScaler.CalculateNodePositions(net, frameSize, margin)

        Call "Initialize gdi objects...".info

        Dim renderEdge As New EdgeRendering(config, scalePos, net)
        Dim renderNode As New NodeRendering(net, config, scalePos)
        Dim renderLabel As New LabelRendering(config)
        Dim hull As New HullPolygonRendering(config, scalePos)

        ' if required hide disconnected nodes, then only the connected node in the network 
        ' Graph will be draw
        ' otherwise all of the nodes in target network graph will be draw onto the canvas.
        Dim connectedNodes = net.connectedNodes.AsDefault
        Dim drawPoints = net.vertex.ToArray Or connectedNodes.When(config.HideDisconnectedNode)
        Dim labels As New List(Of LayoutLabel)

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)

                If Not config.HullPolygonGroups.IsEmpty Then
                    Call "Render hull polygon layer...".debug
                    Call hull.RenderHull(g, drawPoints)
                End If

                Call "Render network edges...".info
                ' 首先在这里绘制出网络的框架：将所有的边绘制出来
                labels += renderEdge.drawEdges(g)

                Call "Render network elements...".info
                ' 然后将网络之中的节点绘制出来，同时记录下节点的位置作为label text的锚点
                ' 最后通过退火算法计算出合适的节点标签文本的位置之后，再使用一个循环绘制出
                ' 所有的节点的标签文本

                ' 在这里进行节点的绘制
                labels += renderNode.RenderingVertexNodes(drawPoints:=drawPoints, g:=g)

                If config.DisplayId AndAlso labels = 0 Then
                    Call "There is no node label data could be draw currently, please check your data....".Warning
                End If

                If config.DisplayId AndAlso labels > 0 Then
                    Call renderLabel.renderLabels(g, labels)
                End If

                Call "Network canvas rendering job done!".debug
            End Sub

        Call "Start Render...".info

        Return g.GraphicsPlots(frameSize.ToSize, margin, config.Background, plotInternal, driver:=config.Driver)
    End Function
End Class
