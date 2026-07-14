' Copyright (c) 2018 GPL3 Licensed
' 轻量粒子：仅位置/速度/寿命的简化模型，用于爆炸、火花、灰尘等特效。

Imports System.Drawing

Namespace Microsoft.VisualBasic.Imaging.Physics.Particles2D

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
