#Region "Microsoft.VisualBasic::ecfee668298e727469b5a2ae9a76eb20, gr\physics\Particles2D\GameParticle.vb"

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

    '   Total Lines: 32
    '    Code Lines: 21 (65.62%)
    ' Comment Lines: 7 (21.88%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 4 (12.50%)
    '     File Size: 1.07 KB


    '     Class GameParticle
    ' 
    '         Sub: Reset
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 轻量粒子：仅位置/速度/寿命的简化模型，用于爆炸、火花、灰尘等特效。

Imports System.Drawing

Namespace Particles2D

    ''' <summary>
    ''' 单个粒子。数据极简（位置、速度、寿命、尺寸、颜色），
    ''' 不与其他物理系统耦合以支持海量粒子。
    ''' </summary>
    Public Class GameParticle
        Public Position As Vector2
        Public Velocity As Vector2
        Public Life As Double
        Public MaxLife As Double
        Public Size As Single
        Public Color As Color
        Public Active As Boolean = False

        ''' <summary>复用该粒子实例，重置其状态</summary>
        Public Sub Reset(pos As Vector2, vel As Vector2, life As Double, size As Single, color As Color)
            Position = pos
            Velocity = vel
            Life = life
            MaxLife = life
            Size = size
            Color = color
            Active = True
        End Sub
    End Class
End Namespace
