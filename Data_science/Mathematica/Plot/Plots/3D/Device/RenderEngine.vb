#Region "Microsoft.VisualBasic::b7490c6302cfff54b79a3c0d8b15e92b, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\3D\Device\RenderEngine.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Math2D

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

            ' 进行投影之后只需要直接取出XY即可得到二维的坐标
            ' 然后生成多边形，进行画布的居中处理
            Dim polygon As Point() = model _
                .Select(Function(element) element.GetPosition(canvas)) _
                .ToArray
            Dim centra As PointF = polygon.CentralOffset(canvas.Size)
            Dim orders = PainterAlgorithm _
                .OrderProvider(model, Function(e) e.Location.Z) _
                .ToArray

            For i As Integer = 0 To model.Length - 1
                Dim index = orders(i)
                Call model(index).Draw(canvas, centra)
            Next
        End Sub
    End Module
End Namespace
