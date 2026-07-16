#Region "Microsoft.VisualBasic::26543d29c52a9a568cdad91c9cef7cf3, gr\Landscape\ThreeMF\Extensions.vb"

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

    '   Total Lines: 70
    '    Code Lines: 40 (57.14%)
    ' Comment Lines: 23 (32.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (10.00%)
    '     File Size: 2.36 KB


    '     Module Extensions
    ' 
    '         Function: GetMaterials, GetTriangleCount, GetVertexCount, ValidMesh
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Landscape.ThreeMF.Xml
Imports Microsoft.VisualBasic.Linq

Namespace ThreeMF

    ''' <summary>
    ''' 3mf 格式相关的扩展工具方法
    ''' </summary>
    Public Module Extensions

        ''' <summary>
        ''' 验证 mesh 对象是否包含有效数据（顶点和三角面均非空）
        ''' </summary>
        ''' <param name="mesh"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ValidMesh(mesh As Mesh) As Boolean
            Return Not mesh Is Nothing AndAlso
                   Not mesh.vertices.IsNullOrEmpty AndAlso
                   Not mesh.triangles.IsNullOrEmpty
        End Function

        ''' <summary>
        ''' 获取 mesh 中的顶点总数
        ''' </summary>
        ''' <param name="mesh"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetVertexCount(mesh As Mesh) As Integer
            If mesh Is Nothing OrElse mesh.vertices Is Nothing Then
                Return 0
            End If
            Return mesh.vertices.Length
        End Function

        ''' <summary>
        ''' 获取 mesh 中的三角面总数
        ''' </summary>
        ''' <param name="mesh"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetTriangleCount(mesh As Mesh) As Integer
            If mesh Is Nothing OrElse mesh.triangles Is Nothing Then
                Return 0
            End If
            Return mesh.triangles.Length
        End Function

        ''' <summary>
        ''' 从 basematerials 中提取所有的 Brush 画刷对象
        ''' </summary>
        ''' <param name="mats"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetMaterials(mats As BaseMaterials) As Brush()
            If mats Is Nothing OrElse mats.items Is Nothing Then
                Return {}
            End If
            Return mats.items _
                .Select(Function(b)
                            Dim c As Color = b.displaycolor.TranslateColor
                            Return New SolidBrush(c)
                        End Function) _
                .ToArray
        End Function

    End Module
End Namespace
