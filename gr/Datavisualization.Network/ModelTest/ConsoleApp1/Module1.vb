Imports System.Drawing
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Imaging
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

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


        For i = 0 To 100

            For Each u In V
                For Each u2 In V
                    If Not u Is u2 Then
                        Dim d = u2.pos - u.pos
                        u.displacement += d.Unit * fr(d.SumMagnitude)
                    End If
                Next
            Next

            For Each uv In E
                With uv
                    Dim d = .u.pos - .v.pos
                    .v.displacement += d.Unit * fa(d.Mod)
                End With
            Next

            For Each u In V
                u.pos += u.displacement.Unit * u.displacement.SumMagnitude
                u.displacement *= 0R
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

    Class edge
        Public u, v As node

        Public Overrides Function ToString() As String
            Return u.ID & ", " & v.ID
        End Function
    End Class

    Class node
        Public ID$
        Public displacement As New Vector(2)
        Public pos As New Vector({Rnd() * 10, Rnd() * 10})

        Public Overrides Function ToString() As String
            Return pos.Vector2D.ToString & " --> " & displacement.GetJson
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
