#Region "Microsoft.VisualBasic::cbde8d34e78df619f3e26774bab577d4, gr\physics\physics_Test\Form1.vb"

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

    '   Total Lines: 51
    '    Code Lines: 38 (74.51%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (25.49%)
    '     File Size: 1.95 KB


    ' Class Form1
    ' 
    '     Sub: Form1_Load, Updates
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Threading
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Language

Public Class Form1

    Dim world As New Microsoft.VisualBasic.Imaging.Physics.World(AddressOf Updates)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim rnd = World.LocationGenerator(Size)
        Dim a As New MassPoint With {.ID = NameOf(a), .Mass = 10, .Point = rnd()}
        Dim b As New MassPoint With {.ID = NameOf(b), .Mass = 50, .Point = rnd()}
        Dim c As New MassPoint With {.ID = NameOf(c), .Mass = 60, .Point = rnd()}

        world += a
        world += b
        world += c

        world.AddReaction(NameOf(a), NameOf(b),
                          Function(x, y)
                              Return Math.RepulsiveForce(1 / System.Math.Sqrt(((x.Point - y.Point) ^ 2).Sum), a.Point, b.Point)
                          End Function)
        world.AddReaction(NameOf(a), NameOf(c),
                          Function(x, y)
                              Return Math.AttractiveForce(System.Math.Sqrt(((x.Point - y.Point) ^ 2).Sum), a.Point, b.Point)
                          End Function)

        Call Me.Invoke(Sub()
                           Call Microsoft.VisualBasic.Parallel.RunTask(Sub() world.React(500))
                       End Sub)
    End Sub

    Sub Updates(objects As IEnumerable(Of MassPoint), forces As Dictionary(Of String, List(Of Force)))

        Using g As Graphics2D = Size.CreateGDIDevice
            For Each m In objects

                Call g.FillPie(Brushes.Black, New Rectangle(m.Point.Vector2D.ToPoint, New Size(10, 10)), 0, 360)
                Call Debugger.ShowForce(m, g, forces(m.ID))

            Next

            Me.BackgroundImage = g.ImageResource
        End Using

        Call Thread.Sleep(30)
    End Sub
End Class
