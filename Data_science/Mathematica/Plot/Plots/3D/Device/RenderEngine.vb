
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D.Device

    ''' <summary>
    ''' 将生成的绘图元素在这个引擎模块之中进行排序操作，然后进行图表的绘制
    ''' </summary>
    Public Module RenderEngine

        <Extension>
        Public Sub RenderAs3DChart(elements As IEnumerable(Of Element3D), canvas As IGraphics, camera As Camera)

        End Sub
    End Module
End Namespace