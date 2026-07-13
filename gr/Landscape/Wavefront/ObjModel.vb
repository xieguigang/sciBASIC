#Region "Microsoft.VisualBasic::69fdd7bbbe1ef5f29ff64be9f9291f88, gr\Landscape\Wavefront\ObjModel.vb"

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

    '   Total Lines: 94
    '    Code Lines: 65 (69.15%)
    ' Comment Lines: 11 (11.70%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 18 (19.15%)
    '     File Size: 3.57 KB


    '     Class ObjModel
    ' 
    '         Properties: comment, mtllib, parts
    ' 
    '         Function: ReadFile, ToSceneModel
    ' 
    '     Class ObjGroup
    ' 
    '         Properties: f, g, IsEmpty, usemtl, vertex
    '                     vn
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language

Namespace Wavefront

    Public Class ObjModel

        ''' <summary>
        ''' lib file name of mtl data
        ''' </summary>
        ''' <returns></returns>
        Public Property mtllib As String
        Public Property parts As ObjGroup()
        Public Property comment As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function ReadFile(file As StreamReader) As ObjModel
            Return ObjTextParser.ParseFile(file)
        End Function

        ''' <summary>
        ''' 将 OBJ 模型转换为统一的 <see cref="Data.SceneModel"/> 对象，
        ''' 以便与 CFD 体素化等下游处理流程对接。
        ''' </summary>
        ''' <returns>转换后的 SceneModel，若输入无效则返回 Nothing</returns>
        Public Function ToSceneModel() As Data.SceneModel
            If parts Is Nothing OrElse parts.Length = 0 Then
                Return New Data.SceneModel With {.Surfaces = {}}
            End If

            Dim surfaces As New List(Of Data.Surface)

            For Each group As ObjGroup In parts
                If group.f Is Nothing OrElse group.vertex Is Nothing Then Continue For
                If group.f.Length = 0 OrElse group.vertex.Length = 0 Then Continue For

                ' 每个 Triangle.v3 包含 3 个顶点索引（从 OBJ 的 1-based 转换为 0-based）
                For Each tri As Triangle In group.f
                    If tri.v3 Is Nothing OrElse tri.v3.Length < 3 Then Continue For

                    Dim i0 = tri.v3(0) - 1
                    Dim i1 = tri.v3(1) - 1
                    Dim i2 = tri.v3(2) - 1

                    ' 验证索引有效性
                    If i0 < 0 OrElse i0 >= group.vertex.Length Then Continue For
                    If i1 < 0 OrElse i1 >= group.vertex.Length Then Continue For
                    If i2 < 0 OrElse i2 >= group.vertex.Length Then Continue For

                    surfaces.Add(New Data.Surface With {
                        .vertices = {
                            New Data.Vertex(group.vertex(i0)),
                            New Data.Vertex(group.vertex(i1)),
                            New Data.Vertex(group.vertex(i2))
                        },
                        .paint = if(String.IsNullOrEmpty(group.usemtl), "#C0C0C0", group.usemtl)
                    })
                Next
            Next

            Return New Data.SceneModel With {
                .Surfaces = surfaces.ToArray
            }
        End Function

    End Class

    Public Class ObjGroup

        Public Property g As String
        Public Property vertex As Point3D()
        Public Property vn As Point3D()
        Public Property usemtl As String
        Public Property f As Triangle()

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return g.StringEmpty AndAlso
                    vertex.IsNullOrEmpty AndAlso
                    vn.IsNullOrEmpty AndAlso
                    usemtl.StringEmpty AndAlso
                    f.IsNullOrEmpty
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{g Or "no_label".AsDefault}: {vertex.Length} vertexs and {f.Length} triangles"
        End Function

    End Class
End Namespace
