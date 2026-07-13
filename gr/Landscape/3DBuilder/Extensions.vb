#Region "Microsoft.VisualBasic::5f24913964d45e5acfe669d853532b77, gr\Landscape\3DBuilder\Extensions.vb"

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

    '   Total Lines: 88
    '    Code Lines: 62 (70.45%)
    ' Comment Lines: 18 (20.45%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 8 (9.09%)
    '     File Size: 3.20 KB


    '     Module Extensions
    ' 
    '         Function: GetMaterials, GetTriangleCount, GetVertexCount, TranslateColor, ValidMesh
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Landscape.Vendor_3mf.XML
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Vendor_3mf

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
        Public Function ValidMesh(mesh As mesh) As Boolean
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
        Public Function GetVertexCount(mesh As mesh) As Integer
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
        Public Function GetTriangleCount(mesh As mesh) As Integer
            If mesh Is Nothing OrElse mesh.triangles Is Nothing Then
                Return 0
            End If
            Return mesh.triangles.Length
        End Function

        ''' <summary>
        ''' 将 OLE 颜色字符串转换为 System.Drawing.Color
        ''' </summary>
        ''' <param name="displaycolor">OLE颜色值字符串</param>
        ''' <returns></returns>
        <Extension>
        Public Function TranslateColor(displaycolor As String) As Color
            If displaycolor.StringEmpty Then
                Return Color.Gray
            End If
            Try
                Return ColorTranslator.FromOle(CInt(displaycolor))
            Catch
                Return Color.Gray
            End Try
        End Function

        ''' <summary>
        ''' 从 basematerials 中提取所有的 Brush 画刷对象
        ''' </summary>
        ''' <param name="mats"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetMaterials(mats As basematerials) As Brush()
            If mats Is Nothing OrElse mats.basematerials Is Nothing Then
                Return {}
            End If
            Return mats.basematerials _
                .Select(Function(b)
                            Dim c As Color = b.displaycolor.TranslateColor
                            Return New SolidBrush(c)
                        End Function) _
                .ToArray
        End Function

    End Module
End Namespace