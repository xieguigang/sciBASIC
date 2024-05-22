#Region "Microsoft.VisualBasic::c698f3ceca015d598a121270bd86cf11, gr\physics\World.vb"

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

    '   Total Lines: 132
    '    Code Lines: 88 (66.67%)
    ' Comment Lines: 21 (15.91%)
    '    - Xml Docs: 90.48%
    ' 
    '   Blank Lines: 23 (17.42%)
    '     File Size: 4.38 KB


    ' Class World
    ' 
    ' 
    '     Enum Type
    ' 
    ' 
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Delegate Sub
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: LocationGenerator
    ' 
    '         Sub: AddReaction, RaiseEvents, (+2 Overloads) React
    ' 
    '         Operators: +
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' Physics world reactor
''' </summary>
Public Class World

    Public Enum Type As Byte
        Plain2D = 2
        Spatial3D = 3
    End Enum

    Public Delegate Function Reaction(m1 As MassPoint, m2 As MassPoint) As Force

    ''' <summary>
    ''' 将状态被改变的对象输出到显示设备或者存储设备之上
    ''' </summary>
    ''' <param name="objs"></param>
    ''' <param name="physX">调试使用</param>
    Public Delegate Sub Output(objs As IEnumerable(Of MassPoint), physX As Dictionary(Of String, List(Of Force)))

    Protected objects As List(Of MassPoint)
    Protected reactions As New Dictionary(Of String, Dictionary(Of String, NamedValue(Of Reaction)))
    Protected forceSystem As New Dictionary(Of String, List(Of Force))
    Protected outputs As Output

    Protected Overridable Sub RaiseEvents()
        If Not outputs Is Nothing Then
            Call outputs(objects, forceSystem)
        End If
    End Sub

    Sub New(Optional updates As Output = Nothing)
        outputs = updates
    End Sub

    ''' <summary>
    ''' 添加两个对象之间的相互作用
    ''' </summary>
    ''' <param name="a$">对象a：<see cref="MassPoint.ID"/></param>
    ''' <param name="b$">对象b：<see cref="MassPoint.ID"/></param>
    ''' <param name="react">这两个对象之间如何产生力？</param>
    ''' <param name="trace$"></param>
    Public Sub AddReaction(a$, b$, react As Reaction, <CallerMemberName> Optional trace$ = Nothing)
        If Not reactions.ContainsKey(a) Then
            reactions.Add(a, New Dictionary(Of String, NamedValue(Of Reaction)))
        End If

        Dim list = reactions(a)

        list(b) = New NamedValue(Of Reaction) With {
            .Name = react.ToString,
            .Value = react,
            .Description = trace
        }
    End Sub

    ''' <summary>
    ''' 指令世界模型对象迭代计算指定的迭代计算次数
    ''' </summary>
    ''' <param name="time%">迭代的次数</param>
    Public Sub React(time As UInteger)
        For i As UInteger = 0 To time
            Call React()
            Call RaiseEvents()
        Next
    End Sub

    Private Sub React()
        For Each F In forceSystem.Values
            Call F.Clear()
        Next

        For Each a As MassPoint In objects.Where(Function(m) Me.reactions.ContainsKey(m.ID))
            Dim reactions = Me.reactions(a.ID)
            Dim forces As List(Of Force) = forceSystem(a.ID)

            For Each b As MassPoint In objects.Where(Function(m) reactions.ContainsKey(m.ID))
                Dim reaction As NamedValue(Of Reaction) = reactions(b.ID)

                With reaction.Value(a, b)
                    .source = reaction.Name

                    Call forces.Add(.ByRef)
                    Call forceSystem(b.ID).Add(- .ByRef)
                End With
            Next
        Next

        For Each m As MassPoint In objects
            ' 力影响了物体的加速度
            m += forceSystem(m.ID).Sum
            ' 加速度改变了物体现有的运动状态
            m.Displacement()
        Next
    End Sub

    Public Shared Operator +(world As World, m As MassPoint) As World
        world.objects += m
        world.forceSystem.Add(m.ID, New List(Of Force))

        If m.Acceleration Is Nothing Then
            m.Acceleration = New Vector(2)
        End If
        If m.Velocity Is Nothing Then
            m.Velocity = New Vector(2)
        End If

        Return world
    End Operator

    Public Shared Function LocationGenerator(size As Size) As Func(Of Vector)
        Dim rand As New Random
        Dim w% = size.Width
        Dim h% = size.Height

        Return Function()
                   Call Thread.Sleep(1)

                   SyncLock rand
                       With rand
                           Return New Vector(data:={ .Next(0, w), .Next(0, h)})
                       End With
                   End SyncLock
               End Function
    End Function
End Class
