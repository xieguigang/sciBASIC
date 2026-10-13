#Region "Microsoft.VisualBasic::fa9487de6a7351398040b04cf3d08362, physics\WorldPlayer\World\BodyVisual.vb"

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

    '   Total Lines: 28
    '    Code Lines: 14 (50.00%)
    ' Comment Lines: 11 (39.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (10.71%)
    '     File Size: 1.13 KB


    ' Class BodyVisual
    ' 
    '     Properties: Age, Body, Color, Id, IsStaticGeometry
    '                 Label, Mesh
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Engine.Physics

''' <summary>
''' 一个"可被渲染的刚体"：物理刚体 + 渲染网格 + 外观。
''' 宿主通过它把物理世界映射到画面上，脚本侧只看到字符串 id。
''' </summary>
Public Class BodyVisual

    ''' <summary>脚本可见的标识。</summary>
    Public Property Id As String
    ''' <summary>可读名称（脚本可通过 label 检索）。</summary>
    Public Property Label As String
    ''' <summary>物理刚体。</summary>
    Public Property Body As RigidBody3D
    ''' <summary>局部渲染网格（与碰撞体共用同一份几何）。</summary>
    Public Property Mesh As Object
    ''' <summary>基础颜色。</summary>
    Public Property Color As Color
    ''' <summary>是否静态关卡几何。</summary>
    Public Property IsStaticGeometry As Boolean
    ''' <summary>存活时长（秒），用于生成时的高亮渐变。</summary>
    Public Property Age As Double

    Public Overrides Function ToString() As String
        Return $"{Id} [{Label}]"
    End Function
End Class
