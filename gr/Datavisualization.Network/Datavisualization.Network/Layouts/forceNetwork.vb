Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Terminal

Namespace Layouts

    Public Module forceNetwork

        ''' <summary>
        ''' Applies the force directed layout.
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="iterations"></param>
        <ExportAPI("Layout.ForceDirected")>
        <Extension>
        Public Sub doForceLayout(ByRef net As NetworkGraph,
                                 Optional Stiffness# = 180,
                                 Optional Repulsion# = 4000,
                                 Optional Damping# = 0.83,
                                 Optional iterations% = 1000,
                                 Optional showProgress As Boolean = False)

            Dim physicsEngine As New ForceDirected2D(net, Stiffness, Repulsion, Damping)

            Using progress As New ProgressBar("Do Force Directed Layout...", cls:=showProgress)
                Dim tick As New ProgressProvider(iterations)
                Dim ETA$

                For i As Integer = 0 To iterations
                    Call physicsEngine.Calculate(0.05F)

                    If showProgress Then
                        ETA = "ETA=" & tick _
                            .ETA(progress.ElapsedMilliseconds) _
                            .FormatTime
                        progress.SetProgress(tick.StepProgress, ETA)
                    End If
                Next
            End Using

            Call physicsEngine.EachNode(
                Sub(node, point)
                    node.Data.initialPostion = point.position
                End Sub)
        End Sub
    End Module
End Namespace