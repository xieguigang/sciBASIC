#Region "Microsoft.VisualBasic::e935134d1fffee82b61c5d0882d3c8ba, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\Layouts\forceNetwork.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Terminal

Namespace Layouts

    Public Module forceNetwork

        ''' <summary>
        ''' Applies the force directed layout.
        ''' (如果有些时候这个函数不起作用的话，考虑一下在调用这个函数之前，先使用<see cref="doRandomLayout"/>初始化随机位置)
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="iterations"></param>
        <ExportAPI("Layout.ForceDirected")>
        <Extension>
        Public Sub doForceLayout(ByRef net As NetworkGraph,
                                 Optional Stiffness# = 80,
                                 Optional Repulsion# = 4000,
                                 Optional Damping# = 0.83,
                                 Optional iterations% = 1000,
                                 Optional showProgress As Boolean = False)

            Dim physicsEngine As New ForceDirected2D(net, Stiffness, Repulsion, Damping)
            Dim tick As Action
            Dim progress As ProgressBar = Nothing

            If showProgress Then
                Dim ticking As New ProgressProvider(iterations)
                Dim ETA$

                progress = New ProgressBar("Do Force Directed Layout...", cls:=showProgress)
                tick = Sub()
                           ETA = "ETA=" & ticking _
                               .ETA(progress.ElapsedMilliseconds) _
                               .FormatTime
                           progress.SetProgress(ticking.StepProgress, ETA)
                       End Sub
            Else
                tick = Sub()
                       End Sub
            End If

            For i As Integer = 0 To iterations
                Call physicsEngine.Calculate(0.05F)
            Next

            Call physicsEngine.EachNode(
                Sub(node, point)
                    node.Data.initialPostion = point.position
                End Sub)

            If Not progress Is Nothing Then
                Call progress.Dispose()
            End If
        End Sub

        <Extension>
        Public Sub doRandomLayout(ByRef net As NetworkGraph)
            Dim rnd As New Random

            For Each x As Node In net.nodes
                x.Data.initialPostion = New FDGVector2 With {
                    .x = rnd.NextDouble * 1000,
                    .y = rnd.NextDouble * 1000
                }
            Next
        End Sub
    End Module
End Namespace
