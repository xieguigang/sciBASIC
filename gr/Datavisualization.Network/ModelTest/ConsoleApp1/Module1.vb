Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Public Module Module1

    <Extension>
    Public Function Vector2D(v As Vector) As PointF
        Return New PointF(v(0), v(1))
    End Function

    ''' <summary>
    ''' 每一个节点之间都存在斥力，只有有相互连线边的节点才会存在引力
    ''' </summary>
    Sub Main()

        Call Randomize()

        Dim V As New List(Of node)
        Dim E As New List(Of edge)

        V.Add(New node With {.ID = 1})
        V.Add(New node With {.ID = 2})
        V.Add(New node With {.ID = 3})
        V.Add(New node With {.ID = 4})
        V.Add(New node With {.ID = 5})
        V.Add(New node With {.ID = 6})
        V.Add(New node With {.ID = 7})
        V.Add(New node With {.ID = 8})

        Dim add = Sub(a%, b%)
                      E.Add(New edge With {.u = V(a - 1), .v = V(b - 1)})
                  End Sub

        add(1, 2)
        add(2, 3)
        add(2, 4)
        add(2, 5)
        add(2, 6)
        add(3, 6)
        add(4, 5)
        add(3, 5)
        add(6, 7)
        add(6, 8)

        Call SpringG(V.ToArray, E.ToArray)

        Pause()
    End Sub

    Public Sub SpringG(V As node(), E As edge())
        Dim force As New Dictionary(Of String, Force)

        For Each X As node In V
            force.Add(X.ID, New Force)
        Next

        For i As Integer = 0 To 1000

            For Each a In V
                For Each b In V.Where(Function(x) Not x Is a)
                    ' 节点之间存在斥力
                    Dim cl = Math.CoulombsLaw(a, b)

                    force(a.ID) += cl
                Next
            Next

            For Each l In E
                Dim a = l.u
                Dim b = l.v

                Dim d = a.Point - b.Point
                Dim f = Math.AttractiveForce(spring(d.SumMagnitude), a.Point, b.Point)

                force(a.ID) += f
                force(b.ID) += -f
            Next

            For Each u In V
                u.ApplyForce(force(u.ID), c4)
                force(u.ID).void()
            Next

            Call V.Select(Function(n) n.ToString).JoinBy("   ").__DEBUG_ECHO

        Next

        Using g = New Size(1000, 1000).CreateGDIDevice

            Dim polygon = V.Select(Function(n) n.Point.Vector2D.ToPoint).ToArray.Enlarge(0.2)
            Dim coffset = polygon.CentralOffset(g.Size).ToPoint



            For Each u In polygon
                Call g.DrawCircle(u.OffSet2D(coffset), 10, Brushes.Blue)
                Call cat(u.ToString)
            Next

            For Each uv In E
                Call g.DrawLine(Pens.Red, uv.u.Point.Vector2D.OffSet2D(coffset), uv.v.Point.Vector2D.OffSet2D(coffset))
            Next

            Call g.Save("x:\fsdfsdf.png", ImageFormats.Png)
        End Using


        Pause()
    End Sub

    Const c1# = 2
    Const c2# = 1

    Const c3# = 1
    Const c4# = 0.1

    ''' <summary>
    ''' inverse square law force
    ''' </summary>
    ''' <param name="d#"></param>
    ''' <returns></returns>
    Public Function repel(d As Vector) As Vector
        Return c3 / d ^ 2
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="d#">where d is the length of the spring</param>
    ''' <returns></returns>
    Public Function spring(d#) As Double
        Return c1 * Log(Abs(d) / c2)
    End Function

    Class edge
        Public u, v As node

        Public Overrides Function ToString() As String
            Return u.ID & ", " & v.ID
        End Function
    End Class

    Public Class node : Inherits MassPoint

        Public Property ID$

        Sub New()
            Point = New Vector({Rnd() * 100, Rnd() * 100})
            Charge = 0.01
        End Sub

        Public Overrides Function ToString() As String
            Return Point.Vector2D.ToString ' & " --> " & force.GetJson
        End Function
    End Class
End Module
