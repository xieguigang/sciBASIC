#Region "Microsoft.VisualBasic::d4cb0ab86b09c9e0874909778666f40d, gr\network-visualization\network_layout\ForceDirected\Planner.vb"

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

    '   Total Lines: 240
    '    Code Lines: 170 (70.83%)
    ' Comment Lines: 31 (12.92%)
    '    - Xml Docs: 70.97%
    ' 
    '   Blank Lines: 39 (16.25%)
    '     File Size: 9.51 KB


    '     Class Planner
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Collide, RejectRegions, reset, runAttraction, runRepulsive
    '              setPosition
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Namespace ForceDirected

    ''' <summary>
    ''' 力导向原始算法代码
    ''' </summary>
    Public Class Planner : Implements IPlanner

        Protected ReadOnly CANVAS_WIDTH As Integer = 1000
        Protected ReadOnly CANVAS_HEIGHT As Integer = 1000

        Protected ReadOnly g As NetworkGraph

        Protected ReadOnly mDxMap As New Dictionary(Of String, Double)
        Protected ReadOnly mDyMap As New Dictionary(Of String, Double)

        Protected ReadOnly k As Double
        Protected ReadOnly ejectFactor As Integer = 6
        Protected ReadOnly condenseFactor As Integer = 3
        Protected ReadOnly maxtx As Integer = 4
        Protected ReadOnly maxty As Integer = 3
        Protected ReadOnly dist_thresh As DoubleRange
        ''' <summary>
        ''' 会尽量避免在这个区域内存在网络的节点，这个区域一般为legend的绘制区域
        ''' </summary>
        Protected ReadOnly avoidRegions As (rect As Rectangle2D, center As PointF)()

        ''' <summary>
        ''' 温度冷却调度：每轮迭代后位移上限按 <see cref="coolingRate"/> 递减，
        ''' 使布局从“大步铺开”逐渐收敛到“微调”，避免恒被常数 ±maxtx/±maxty 截断而失效。
        ''' </summary>
        Protected ReadOnly initTemperature As Double
        Protected temperature As Double
        Protected Const coolingRate As Double = 0.99

        Sub New(g As NetworkGraph,
                Optional ejectFactor As Integer = 6,
                Optional condenseFactor As Integer = 3,
                Optional maxtx As Integer = 4,
                Optional maxty As Integer = 3,
                Optional dist_threshold$ = "30,250",
                Optional size$ = "1000,1000",
                Optional avoidRegions As RectangleF() = Nothing)

            Me.g = g

            With size.SizeParser
                CANVAS_WIDTH = .Width
                CANVAS_HEIGHT = .Height
            End With

            Me.dist_thresh = dist_threshold.NumericRangeParser
            Me.maxtx = maxtx
            Me.maxty = maxty
            Me.condenseFactor = condenseFactor
            Me.ejectFactor = ejectFactor
            Me.k = std.Sqrt(CANVAS_WIDTH * CANVAS_HEIGHT / g.vertex.Count)
            Me.avoidRegions = avoidRegions _
                .SafeQuery _
                .Select(Function(rect) (New Rectangle2D(rect), rect.Centre)) _
                .ToArray

            ' 兼容旧参数：若调用方显式传入较大的 maxtx/maxty 则以其为主；
            ' 否则旧的默认 4/3 过小，改用与画布尺度相关的合理初温。
            initTemperature = std.Max(maxtx, maxty)

            If initTemperature <= 4 Then
                initTemperature = std.Min(CANVAS_WIDTH, CANVAS_HEIGHT) * 0.1
            End If

            temperature = initTemperature
        End Sub

        ''' <summary>
        ''' 使用 <see cref="ForceDirectedParameters"/> 参数对象构造布局器，便于通过 PropertyGrid 编辑参数
        ''' </summary>
        Sub New(g As NetworkGraph, params As ForceDirectedParameters)
            Me.New(g,
                   ejectFactor:=params.EjectFactor,
                   condenseFactor:=params.CondenseFactor,
                   maxtx:=params.MaxTx,
                   maxty:=params.MaxTy,
                   dist_threshold:=params.DistThreshold,
                   size:=String.Format("{0},{1}", params.CanvasWidth, params.CanvasHeight))
        End Sub

        ''' <summary>
        ''' run a step of the current layout algorithm 
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Collide(Optional timeStep As Double = Double.NaN) Implements IPlanner.Collide
            Call reset()
            Call runRepulsive()
            Call runAttraction()
            Call RejectRegions()
            Call setPosition()
            ' 温度冷却：随迭代递减位移上限，逐步收敛
            temperature = temperature * coolingRate
        End Sub

        Protected Sub reset()
            For Each v As Node In g.vertex
                mDxMap(v.label) = 0.0
                mDyMap(v.label) = 0.0
            Next
        End Sub

        Protected Sub RejectRegions()
            Dim dist, distX, distY As Double
            Dim id As String
            Dim dx, dy As Double

            For Each rect In avoidRegions
                For Each v As Node In g.vertex
                    distX = rect.center.X - v.data.initialPostion.x
                    distY = rect.center.Y - v.data.initialPostion.y
                    dist = std.Sqrt(distX * distX + distY * distY)
                    id = v.label

                    If dist > 0 Then
                        dx = (distX / dist) * (k * k / dist) * ejectFactor * 5
                        dy = (distY / dist) * (k * k / dist) * ejectFactor * 5

                        mDxMap(id) = mDxMap(id) + dx
                        mDyMap(id) = mDyMap(id) + dy
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 计算出任意两个节点之间的排斥力
        ''' </summary>
        Protected Overridable Sub runRepulsive()
            Dim distX, distY, dist As Double
            Dim id As String
            Dim ejectFactor = Me.ejectFactor
            Dim dx, dy As Double

            For Each v As Node In g.vertex
                id = v.label

                mDxMap(id) = 0.0
                mDyMap(id) = 0.0

                For Each u As Node In g.vertex.Where(Function(ui) Not ui Is v)
                    distX = v.data.initialPostion.x - u.data.initialPostion.x
                    distY = v.data.initialPostion.y - u.data.initialPostion.y
                    dist = std.Sqrt(distX * distX + distY * distY)

                    'If (dist < dist_thresh.Min) Then
                    '    ejectFactor = 5
                    'End If

                    If dist > 0 AndAlso dist < dist_thresh.Max Then
                        dx = (distX / dist) * (k * k / dist) * ejectFactor
                        dy = (distY / dist) * (k * k / dist) * ejectFactor

                        mDxMap(id) = mDxMap(id) + dx
                        mDyMap(id) = mDyMap(id) + dy
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 边连接两端的节点之间的吸引力
        ''' </summary>
        Protected Overridable Sub runAttraction()
            Dim u, v As Node
            Dim distX, distY, dist As Double
            Dim dx, dy As Double

            For Each edge As Edge In g.graphEdges
                u = edge.U
                v = edge.V
                distX = u.data.initialPostion.x - v.data.initialPostion.x
                distY = u.data.initialPostion.y - v.data.initialPostion.y
                dist = std.Sqrt(distX * distX + distY * distY)
                dx = distX * dist / k * condenseFactor
                dy = distY * dist / k * condenseFactor

                mDxMap(u.label) = mDxMap(u.label) - dx
                mDyMap(u.label) = mDyMap(u.label) - dy
                mDxMap(v.label) = mDxMap(v.label) + dx
                mDyMap(v.label) = mDyMap(v.label) + dy
            Next
        End Sub

        Private Sub setPosition()
            Dim dx, dy As Double
            Dim x, y As Double
            Dim len As Double

            For Each node As Node In g.vertex.Where(Function(v) Not v.pinned)
                dx = mDxMap(node.label)
                dy = mDyMap(node.label)

                ' 按合力方向归一化后以当前温度限幅，保留排斥/吸引的相对强弱
                ' （原先逐轴硬截断到 ±maxtx/±maxty，使所有节点位移被等同压到极小常数，
                '   排斥与吸引的相对强弱全部失效、收敛极慢）。
                len = std.Sqrt(dx * dx + dy * dy)

                If len > temperature AndAlso len > 0 Then
                    dx = dx / len * temperature
                    dy = dy / len * temperature
                End If

                x = node.data.initialPostion.x
                y = node.data.initialPostion.y
                x = x + dx ' If((x + dx) >= CANVAS_WIDTH OrElse (x + dx) <= 0, x - dx, x + dx)
                y = y + dy ' If((y + dy) >= CANVAS_HEIGHT OrElse (y + dy <= 0), y - dy, y + dy)

                If x >= CANVAS_WIDTH Then
                    x = CANVAS_WIDTH
                ElseIf x < 0 Then
                    x = 0
                End If

                If y >= CANVAS_HEIGHT Then
                    y = CANVAS_HEIGHT
                ElseIf y < 0 Then
                    y = 0
                End If

                node.data.initialPostion.x = x
                node.data.initialPostion.y = y
            Next
        End Sub
    End Class
End Namespace
