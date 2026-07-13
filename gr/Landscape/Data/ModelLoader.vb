#Region "Microsoft.VisualBasic::f948289575ecd0752cdca3fbcb20fa4b, gr\Landscape\Data\ModelLoader.vb"

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

    '   Total Lines: 156
    '    Code Lines: 76 (48.72%)
    ' Comment Lines: 62 (39.74%)
    '    - Xml Docs: 90.32%
    ' 
    '   Blank Lines: 18 (11.54%)
    '     File Size: 6.36 KB


    '     Enum ModelFormat
    ' 
    '         _3DS, _3MF, DAE, GLB, GLTF
    '         OBJ, STL, Unknown
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module ModelLoader
    ' 
    '         Function: DetectFormat, LoadModel, (+2 Overloads) VoxelizeForCFD
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Landscape.Collada
Imports Microsoft.VisualBasic.Imaging.Landscape.Gltf
Imports Microsoft.VisualBasic.Imaging.Landscape.Max3DS
Imports Microsoft.VisualBasic.Imaging.Landscape.Stl
Imports Microsoft.VisualBasic.Imaging.Landscape.ThreeMF
Imports Microsoft.VisualBasic.Imaging.Landscape.Voxelization
Imports Microsoft.VisualBasic.Imaging.Landscape.Wavefront

Namespace Data

    ''' <summary>
    ''' 支持的 3D 模型文件格式枚举
    ''' </summary>
    Public Enum ModelFormat
        ''' <summary>未知或不支持的格式</summary>
        Unknown

        ''' <summary>Stereolithography (STL) — ASCII 或 Binary</summary>
        STL

        ''' <summary>glTF 2.0 文本格式 (.gltf)</summary>
        GLTF

        ''' <summary>glTF 2.0 二进制格式 (.glb)</summary>
        GLB

        ''' <summary>Wavefront OBJ (.obj)</summary>
        OBJ

        ''' <summary>COLLADA Digital Asset Exchange (.dae)</summary>
        DAE

        ''' <summary>3D-Studio Max (.3ds)</summary>
        <Description("3DS")>
        _3DS

        ''' <summary>3D Manufacturing Format (.3mf)</summary>
        <Description("3MF")>
        _3MF
    End Enum

    ''' <summary>
    ''' 三维模型统一加载器。
    ''' 
    ''' 根据文件扩展名自动检测格式，将任意支持的 3D 模型文件
    ''' 转换为统一的 <see cref="SceneModel"/> 对象，并可直接对接
    ''' <see cref="Voxelization.Voxelizer.Voxelize"/> 进行 CFD 体素化。
    ''' 
    ''' ## 支持的格式
    ''' 
    ''' - STL (.stl) — ASCII 和 Binary 格式自动检测
    ''' - glTF (.gltf) — JSON 文本格式 + 外部 .bin 缓冲区
    ''' - GLB (.glb) — glTF 二进制容器格式
    ''' - COLLADA (.dae) — XML 格式的几何数据
    ''' - Wavefront OBJ (.obj) — 文本格式，支持三角扇分解的多边形面
    ''' - 3DS (.3ds) — 3D-Studio 旧版交换格式
    ''' - 3MF (.3mf) — 3D 制造格式（ZIP 容器 + XML 模型）
    ''' 
    ''' ## 使用示例
    ''' 
    ''' ```vbnet
    ''' ' 从文件加载任意格式的 3D 模型
    ''' Dim model As SceneModel = ModelLoader.LoadModel("model.stl")
    ''' 
    ''' ' 直接进行 CFD 体素化
    ''' Dim voxels As VoxelModel = ModelLoader.VoxelizeForCFD("model.obj", resolution:=128)
    ''' ```
    ''' </summary>
    Public Module ModelLoader

        ''' <summary>
        ''' 从文件路径加载 3D 模型，自动根据扩展名检测格式
        ''' </summary>
        ''' <param name="filePath$">3D 模型文件路径</param>
        ''' <returns>统一的 SceneModel 对象</returns>
        ''' <exception cref="FileNotFoundException">文件不存在时抛出</exception>
        ''' <exception cref="NotSupportedException">格式不支持时抛出</exception>
        <Extension>
        Public Function LoadModel(filePath$) As SceneModel
            If Not File.Exists(filePath) Then
                Throw New FileNotFoundException($"找不到模型文件: {filePath}")
            End If

            Dim format As ModelFormat = DetectFormat(filePath)

            Select Case format
                Case ModelFormat.STL
                    Return filePath.ParseSTL
                Case ModelFormat.GLTF
                    Return GltfReader.ReadFile(filePath)
                Case ModelFormat.GLB
                    Return GlbReader.ReadFile(filePath)
                Case ModelFormat.OBJ
                    Using reader As New StreamReader(filePath)
                        Return ObjTextParser.ParseFile(reader).ToSceneModel
                    End Using
                Case ModelFormat.DAE
                    Return ColladaParser.ReadFile(filePath)
                Case ModelFormat._3DS
                    Return filePath.Parse3DS
                Case ModelFormat._3MF
                    Return ModelIO.Open(filePath).ToSceneModel
                Case Else
                    Throw New NotSupportedException($"不支持的 3D 模型文件格式: {filePath.ExtensionSuffix}")
            End Select
        End Function

        ''' <summary>
        ''' 从文件路径加载 3D 模型并直接进行 CFD 体素化
        ''' </summary>
        ''' <param name="filePath$">3D 模型文件路径</param>
        ''' <param name="resolution">最长边上的体素分辨率，默认 128</param>
        ''' <returns>CFD 实心体素模型</returns>
        <Extension>
        Public Function VoxelizeForCFD(filePath$, Optional resolution As Integer = 128) As VoxelModel
            Dim model As SceneModel = filePath.LoadModel
            Return model.Voxelize(resolution)
        End Function

        ''' <summary>
        ''' 从文件路径加载 3D 模型并以指定体素物理尺寸进行 CFD 体素化
        ''' </summary>
        ''' <param name="filePath$">3D 模型文件路径</param>
        ''' <param name="voxelSize">每个体素的物理尺寸（世界坐标单位）</param>
        ''' <returns>CFD 实心体素模型</returns>
        <Extension>
        Public Function VoxelizeForCFD(filePath$, voxelSize As Double) As VoxelModel
            Dim model As SceneModel = filePath.LoadModel
            Return model.Voxelize(voxelSize)
        End Function

        ''' <summary>
        ''' 根据文件扩展名检测 3D 模型格式
        ''' </summary>
        ''' <param name="filePath$">文件路径</param>
        ''' <returns>检测到的格式枚举值</returns>
        Public Function DetectFormat(filePath$) As ModelFormat
            Dim ext As String = filePath.ExtensionSuffix.ToLowerInvariant

            Select Case ext
                Case "stl" : Return ModelFormat.STL
                Case "gltf" : Return ModelFormat.GLTF
                Case "glb" : Return ModelFormat.GLB
                Case "obj" : Return ModelFormat.OBJ
                Case "dae" : Return ModelFormat.DAE
                Case "3ds" : Return ModelFormat._3DS
                Case "3mf" : Return ModelFormat._3MF
                Case Else : Return ModelFormat.Unknown
            End Select
        End Function

    End Module
End Namespace
