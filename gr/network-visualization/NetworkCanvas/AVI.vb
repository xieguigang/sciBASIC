Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Interfaces
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.AVIMedia
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module AVI

    ReadOnly defaultArguments As [Default](Of ForceDirectedArgs) = ForceDirectedArgs.DefaultNew

    <Extension>
    Public Function DoRenderVideo(network As NetworkGraph,
                                  frameSize As [Variant](Of Size, Integer(), String),
                                  Optional physics As ForceDirectedArgs = Nothing,
                                  Optional ShowLabels As Boolean = True,
                                  Optional render3D As Boolean = False,
                                  Optional fps As Integer = 24,
                                  Optional drawFrames As Integer = 2048) As Encoder
        Dim canvasSize As Size
        Dim renderer As Renderer

        If frameSize Like GetType(String) Then
            canvasSize = frameSize.TryCast(Of String).SizeParser
        ElseIf frameSize Like GetType(Size) Then
            canvasSize = frameSize
        Else
            canvasSize = New Size With {
                .Width = frameSize.TryCast(Of Integer()).GetValue(0),
                .Height = frameSize.TryCast(Of Integer()).GetValue(1)
            }
        End If

        Dim g As Graphics
        Dim region As New Rectangle(New Point, canvasSize)
        Dim engine As IForceDirected

        physics = physics Or defaultArguments

        If render3D Then
            engine = New ForceDirected3D(network, physics.Stiffness, physics.Repulsion, physics.Damping)
            renderer = New Renderer3D(Function() g, Function() region, engine)
        Else
            engine = New ForceDirected2D(network, physics.Stiffness, physics.Repulsion, physics.Damping)
            renderer = New Renderer(Function() g, Function() region, engine)
        End If

        renderer.ShowLabels = ShowLabels

        Dim avi As New AVIStream(fps, canvasSize.Width, canvasSize.Height)
        Dim r As Double

        For i As Integer = 0 To drawFrames
            Using canvas As Graphics2D = canvasSize.CreateGDIDevice
                g = canvas.Graphics
                g.CompositingQuality = CompositingQuality.HighQuality
                g.SmoothingMode = SmoothingMode.HighQuality

                If render3D Then
                    r += 0.4
                    DirectCast(renderer, Renderer3D).rotate = r
                End If

                Call engine.Calculate(0.05F)
                Call renderer.Draw(0.05F, physicsUpdate:=False)

                Call avi.addFrame(canvas.ImageResource)
            End Using
        Next

        Dim video As New Encoder(New Settings With {.width = canvasSize.Width, .height = canvasSize.Height})
        Call video.streams.Add(avi)
        Return video
    End Function
End Module
