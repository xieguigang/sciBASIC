#Region "Microsoft.VisualBasic::3e58c9ea9987da9a2d371f965a50f658, gr\physics\Particles2D\Emitter.vb"

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
    '    Code Lines: 28 (51.85%)
    ' Comment Lines: 14 (25.93%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 12 (22.22%)
    '     File Size: 1.93 KB


    '     Class Emitter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GenerateOne
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 粒子发射器：按方向/速度/寿命生成粒子。

Imports System
Imports System.Drawing
Imports std = System.Math

Namespace Particles2D

    ''' <summary>
    ''' 粒子发射器：在 <see cref="Position"/> 处沿 <see cref="BaseAngle"/> 方向、
    ''' 以 <see cref="Spread"/> 张角随机产生粒子。支持一次性爆发或持续生成。
    ''' </summary>
    Public Class Emitter
        ''' <summary>发射位置</summary>
        Public Position As Vector2

        ''' <summary>发射中心方向（弧度）</summary>
        Public BaseAngle As Double = -std.PI / 2.0

        ''' <summary>张角（弧度，绕中心方向两侧各 Spread/2）</summary>
        Public Spread As Double = std.PI

        ''' <summary>初速度大小</summary>
        Public Speed As Double = 120.0

        ''' <summary>寿命（秒）</summary>
        Public Life As Double = 1.5

        ''' <summary>粒子尺寸（像素）</summary>
        Public Size As Single = 3.0

        ''' <summary>粒子颜色</summary>
        Public Color As Color = Color.Orange

        Private Shared rnd As New Random

        Sub New(pos As Vector2, Optional speed As Double = 120.0, Optional life As Double = 1.5)
            Me.Position = pos
            Me.Speed = speed
            Me.Life = life
        End Sub

        ''' <summary>生成单个粒子（处于激活状态）</summary>
        Public Function GenerateOne() As GameParticle
            Dim angle = BaseAngle + (rnd.NextDouble() - 0.5) * Spread
            Dim speed = Me.Speed * (0.5 + rnd.NextDouble())
            Dim v = New Vector2(std.Cos(angle), std.Sin(angle)) * speed
            Dim p = New GameParticle
            p.Reset(Position, v, Life * (0.7 + 0.6 * rnd.NextDouble()), Size, Color)
            Return p
        End Function
    End Class
End Namespace
