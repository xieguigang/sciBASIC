Imports System.ComponentModel

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
End Namespace