#Region "Microsoft.VisualBasic::ee1bb7c4241b5e7586b7d5f52b09e09f, gr\Landscape\Data\ModelFormat.vb"

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

    '   Total Lines: 35
    '    Code Lines: 15 (42.86%)
    ' Comment Lines: 11 (31.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (25.71%)
    '     File Size: 880 B


    '     Enum ModelFormat
    ' 
    '         _3DS, _3MF, DAE, GLB, GLTF
    '         OBJ, STL, Unknown
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
