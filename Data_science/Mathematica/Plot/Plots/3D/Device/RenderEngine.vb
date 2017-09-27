
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D.Device

    ''' <summary>
    ''' 将生成的绘图元素在这个引擎模块之中进行排序操作，然后进行图表的绘制
    ''' </summary>
    Public Module RenderEngine

        <Extension>
        Public Sub RenderAs3DChart(elements As IEnumerable(Of Element3D), canvas As IGraphics, camera As Camera, region As GraphicsRegion)
            ' 首先对模型执行rotate和project，然后再进行Z排序
            Dim model As Element3D() = elements.ToArray

            For Each element As Element3D In model
                Call element.Transform(camera)
            Next

            Dim orders = PainterAlgorithm _
                .OrderProvider(model, Function(e) e.Location.Z) _
                .ToArray

            For i As Integer = 0 To model.Length - 1
                Dim index = orders(i)
                Call model(index).Draw(canvas)
            Next
        End Sub
    End Module
End Namespace