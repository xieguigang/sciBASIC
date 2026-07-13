#Region "Microsoft.VisualBasic::c62d482cdccddd8af53a7e42e791c69d, gr\Landscape\ThreeMF\Project.vb"

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


' Code Statistics:

'   Total Lines: 54
'    Code Lines: 33 (61.11%)
' Comment Lines: 13 (24.07%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 8 (14.81%)
'     File Size: 1.80 KB


'     Class Project
' 
'         Properties: model, Thumbnail
' 
'         Function: FromZipDirectory, GetMatrix, GetSurfaces
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Landscape.Data
Imports Microsoft.VisualBasic.Imaging.Landscape.ThreeMF.Xml
Imports Microsoft.VisualBasic.Linq

Namespace ThreeMF

    Public Class Project

        ''' <summary>
        ''' ``*.3mf/Metadata/thumbnail.png``
        ''' </summary>
        ''' <returns></returns>
        Public Property Thumbnail As Image
        ''' <summary>
        ''' ``*.3mf/3D/3dmodel.model``
        ''' </summary>
        ''' <returns></returns>
        Public Property model As XmlModel3D

        Public Shared Function FromZipDirectory(dir$) As Project
            Return New Project With {
                .Thumbnail = $"{dir}/Metadata/thumbnail.png".LoadImage,
                .model = ModelIO.Load3DModel(dir & "/3D/3dmodel.model")
            }
        End Function

        ''' <summary>
        ''' Get all of the 3D surface model data in this 3mf project.
        ''' </summary>
        ''' <param name="centraOffset"></param>
        ''' <returns></returns>
        Public Function GetSurfaces(Optional centraOffset As Boolean = False) As Surface()
            If model Is Nothing Then
                Return {}
            Else
                Dim out As Surface() = model.GetSurfaces.ToArray

                If centraOffset Then
                    With out.Centra
                        out = .Offsets(out).ToArray
                    End With
                End If

                Return out
            End If
        End Function

        ''' <summary>
        ''' 将 3MF 项目中的所有 3D 表面模型数据转换为统一的 <see cref="Data.SceneModel"/> 对象，
        ''' 以便与 CFD 体素化等下游处理流程对接。
        ''' </summary>
        ''' <returns>转换后的 SceneModel，若项目无有效模型则返回空 SceneModel</returns>
        Public Function ToSceneModel() As Data.SceneModel
            Dim drawingSurfaces As Surface() = GetSurfaces()
            If drawingSurfaces Is Nothing OrElse drawingSurfaces.Length = 0 Then
                Return New Data.SceneModel With {.Surfaces = {}}
            End If

            Dim surfaces As New List(Of Data.Surface)(drawingSurfaces.Length)

            For Each s As Surface In drawingSurfaces
                If s.vertices Is Nothing OrElse s.vertices.Length < 3 Then Continue For

                ' 从 Brush 中提取颜色字符串
                Dim paint As String = "#C0C0C0"
                If s.brush IsNot Nothing AndAlso TypeOf s.brush Is SolidBrush Then
                    Dim c As Color = DirectCast(s.brush, SolidBrush).Color
                    paint = $"#{c.R:X2}{c.G:X2}{c.B:X2}"
                End If

                surfaces.Add(New Data.Surface With {
                    .vertices = {
                        New Data.Vertex(s.vertices(0)),
                        New Data.Vertex(s.vertices(1)),
                        New Data.Vertex(s.vertices(2))
                    },
                    .paint = paint
                })
            Next

            Return New Data.SceneModel With {
                .Surfaces = surfaces.ToArray
            }
        End Function

        Public Function GetMatrix(Optional centraOffset As Boolean = False) As Matrix
            Return New Matrix(GetSurfaces(centraOffset))
        End Function
    End Class
End Namespace
