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