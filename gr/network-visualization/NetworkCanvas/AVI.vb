#Region "Microsoft.VisualBasic::f26fc72e80d008a56bc766d42a017335, gr\network-visualization\NetworkCanvas\AVI.vb"

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

'   Total Lines: 79
'    Code Lines: 65 (82.28%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 14 (17.72%)
'     File Size: 3.29 KB


' Module AVI
' 
'     Function: DoRenderVideo
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce.Interfaces
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.AVIMedia
Imports Microsoft.VisualBasic.Imaging.Driver
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

        Dim g As IGraphics = Nothing
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
            Using canvas As IGraphics = DriverLoad.CreateGraphicsDevice(canvasSize, driver:=Drivers.GDI)
                If render3D Then
                    r += 0.4
                    DirectCast(renderer, Renderer3D).rotate = r
                End If

                Call engine.Collide(0.05F)
                Call renderer.Draw(0.05F, physicsUpdate:=False)

                Call avi.addFrame(DirectCast(canvas, GdiRasterGraphics).ImageResource)
            End Using
        Next

        Dim video As New Encoder(New Settings With {.width = canvasSize.Width, .height = canvasSize.Height})
        Call video.streams.Add(avi)
        Return video
    End Function
End Module
