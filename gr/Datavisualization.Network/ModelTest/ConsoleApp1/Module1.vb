Imports System.Drawing
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Imaging
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Mathematical.SyntaxAPI.MathExtension

Public Module Module1

    <Extension>
    Public Function Vector2D(v As Vector) As PointF
        Return New PointF(v(0), v(1))
    End Function

    ''' <summary>
    ''' 每一个节点之间都存在斥力，只有有相互连线边的节点才会存在引力
    ''' </summary>
    Sub Main()

        Dim V As New List(Of node)
        Dim E As New List(Of edge)

        V.Add(New node With {.ID = 1})
        V.Add(New node With {.ID = 2})
        V.Add(New node With {.ID = 3})
        V.Add(New node With {.ID = 4})

        Dim add = Sub(a%, b%)
                      E.Add(New edge With {.u = V(a - 1), .v = V(b - 1)})
                  End Sub

        add(1, 2)
        add(2, 3)
        add(2, 4)

        For Each u In V
            Call cat(u.ToString)
        Next


        Call SpringG(V.ToArray, E.ToArray)


        For i = 0 To 100

            For Each u In V
                For Each u2 In V
                    If Not u Is u2 Then
                        Dim d = u2.pos - u.pos
                        u.force += d.Unit * fr(d.SumMagnitude)
                    End If
                Next
            Next

            For Each uv In E
                With uv
                    Dim d = .u.pos - .v.pos
                    .v.force += d.Unit * fa(d.Mod)
                End With
            Next

            For Each u In V
                u.pos += u.force.Unit * u.force.SumMagnitude
                u.force *= 0R
            Next
        Next



        Using g = New Size(1000, 1000).CreateGDIDevice

            For Each u In V
                Call g.DrawCircle(u.pos.Vector2D, 10, Brushes.Blue)
                Call cat(u.ToString)
            Next

            For Each uv In E
                Call g.DrawLine(Pens.Red, uv.u.pos.Vector2D, uv.v.pos.Vector2D)
            Next

            Call g.Save("x:\fsdfsdf.png", ImageFormats.Png)
        End Using

        Pause()
    End Sub

    Public Sub SpringG(V As node(), E As edge())


        For i As Integer = 0 To 100

            For Each a In V
                For Each b In V.Where(Function(x) Not x Is a)
                    ' 节点之间存在斥力
                    Dim d = (a.pos - b.pos)
                    a.force += repel(d)
                Next
            Next

            For Each l In E
                Dim a = l.u
                Dim b = l.v

                Dim d = spring(a.pos - b.pos)
                a.force -= d
                b.force -= d
            Next

            For Each u In V
                u.pos += c4 * u.force
                u.force *= 0R
            Next

            Call V.GetJson.__DEBUG_ECHO

        Next


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
    Public Function spring(d As Vector) As Vector
        Return c1 * VectorMath.Log(d / c2)
    End Function

    Class edge
        Public u, v As node

        Public Overrides Function ToString() As String
            Return u.ID & ", " & v.ID
        End Function
    End Class

    Public Class node
        Public Property ID$
        Public Property force As New Vector(2)
        Public Property pos As New Vector({Rnd() * 10, Rnd() * 10})

        Public Overrides Function ToString() As String
            Return pos.Vector2D.ToString & " --> " & force.GetJson
        End Function
    End Class

    Const k = 0.0000000000001

    ''' <summary>
    ''' attractive force
    ''' </summary>
    ''' <param name="d#"></param>
    ''' <returns></returns>
    Public Function fa(d#) As Double
        Return d ^ 2 / k
    End Function

    ''' <summary>
    ''' repulsive force
    ''' </summary>
    ''' <param name="d#"></param>
    ''' <returns></returns>
    Public Function fr(d#) As Double
        Return -k ^ 2 / d
    End Function

End Module
