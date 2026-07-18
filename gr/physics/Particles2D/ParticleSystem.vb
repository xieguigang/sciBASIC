#Region "Microsoft.VisualBasic::89d7ece578bd1c735b6716f3f64f3847, gr\physics\Particles2D\ParticleSystem.vb"

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

    '   Total Lines: 71
    '    Code Lines: 48 (67.61%)
    ' Comment Lines: 13 (18.31%)
    '    - Xml Docs: 84.62%
    ' 
    '   Blank Lines: 10 (14.08%)
    '     File Size: 2.51 KB


    '     Class ParticleSystem
    ' 
    '         Properties: Count, Particles
    ' 
    '         Function: GetFree
    ' 
    '         Sub: [Step], Emit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' 粒子系统：对象池管理粒子，仅做重力 + 线性阻力积分，避免 GC 抖动。

Imports System.Collections.Generic
Imports System.Linq

Namespace Particles2D

    ''' <summary>
    ''' 轻量粒子系统。使用对象池复用 <see cref="GameParticle"/>，避免频繁分配；
    ''' 每个粒子仅受重力与线性阻力影响，可承载海量粒子（爆炸、火花、灰尘、魔法等）。
    ''' 不与刚体耦合，以保证性能。
    ''' </summary>
    Public Class ParticleSystem
        Private pool As New List(Of GameParticle)

        ''' <summary>重力加速度（像素/秒²）</summary>
        Public Gravity As Vector2 = New Vector2(0, 30.0)

        ''' <summary>线性阻力（0 = 无）</summary>
        Public Drag As Double = 0.0

        ''' <summary>当前活跃粒子数</summary>
        Public ReadOnly Property Count As Integer
            Get
                Dim c = 0
                For Each p In pool
                    If p.Active Then c += 1
                Next
                Return c
            End Get
        End Property

        ''' <summary>活跃粒子集合（用于渲染）</summary>
        Public ReadOnly Property Particles As IEnumerable(Of GameParticle)
            Get
                Return pool.Where(Function(p) p.Active)
            End Get
        End Property

        ''' <summary>由发射器爆发 n 个粒子</summary>
        Public Sub Emit(emitter As Emitter, n As Integer)
            For i = 1 To n
                Dim free = GetFree()
                Dim g = emitter.GenerateOne()
                free.Reset(g.Position, g.Velocity, g.Life, g.Size, g.Color)
            Next
        End Sub

        ''' <summary>每步推进粒子</summary>
        Public Sub [Step](dt As Double)
            For Each p In pool
                If Not p.Active Then Continue For
                p.Velocity += Gravity * dt
                p.Velocity *= 1.0 / (1.0 + Drag * dt)
                p.Position += p.Velocity * dt
                p.Life -= dt
                If p.Life <= 0 Then p.Active = False
            Next
        End Sub

        Private Function GetFree() As GameParticle
            For Each p In pool
                If Not p.Active Then Return p
            Next
            Dim np = New GameParticle
            pool.Add(np)
            Return np
        End Function
    End Class
End Namespace
