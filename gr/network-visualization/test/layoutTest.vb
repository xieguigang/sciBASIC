#Region "Microsoft.VisualBasic::9728a83c5c5a63bfb62f215976e9126a, gr\network-visualization\test\layoutTest.vb"

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

    ' Module layoutTest
    ' 
    '     Function: repel, spring, Vector2D
    ' 
    '     Sub: Main, SpringG
    '     Class edge
    ' 
    '         Function: ToString
    ' 
    '     Class node2
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module layoutTest

    <Extension>
    Public Function Vector2D(v As Vector) As PointF
        Return New PointF(v(0), v(1))
    End Function

    ''' <summary>
    ''' 每一个节点之间都存在斥力，只有有相互连线边的节点才会存在引力
    ''' </summary>
    Sub Main()

        Call Randomize()

        Dim V As New List(Of node2)
        Dim E As New List(Of edge)

        V.Add(New node2 With {.ID = 1, .Point = {100, 100}})
        V.Add(New node2 With {.ID = 2, .Point = {-100, -100}})
        V.Add(New node2 With {.ID = 3, .Point = {100, -100}})
        V.Add(New node2 With {.ID = 4})
        V.Add(New node2 With {.ID = 5})
        V.Add(New node2 With {.ID = 6})
        V.Add(New node2 With {.ID = 7})
        V.Add(New node2 With {.ID = 8})
        V.Add(New node2 With {.ID = 9})
        V.Add(New node2 With {.ID = 10})
        V.Add(New node2 With {.ID = 11})

        Dim add = Sub(a%, b%)
                      E.Add(New edge With {.u = V(a - 1), .v = V(b - 1)})
                  End Sub

        add(1, 2)
        add(2, 3)
        add(1, 3)
        add(2, 4)
        add(2, 5)
        add(2, 6)
        add(3, 6)
        add(4, 5)
        add(3, 5)
        add(6, 7)
        add(6, 8)
        add(9, 8)
        add(9, 10)
        add(10, 11)

        Call SpringG(V.ToArray, E.ToArray)

        Pause()
    End Sub

    Public Sub SpringG(V As node2(), E As edge())
        Dim force As New Dictionary(Of String, List(Of Force))

        For Each X As node2 In V
            force.Add(X.ID, New List(Of Force))
        Next

        Try
            Call FileSystem.RmDir("X:\fffff")
        Catch ex As Exception

        End Try



        For i As Integer = 0 To 1000

            cat($"{i}:\n")

            For Each a In V
                For Each b In V.Where(Function(x) Not x Is a)
                    ' 节点之间存在斥力
                    Dim cl = Math.CoulombsLaw(a, b)


                    cl.strength *= 50

                    ' 斥力部分只需要添加一个就行了
                    ' 因为这两个嵌套的for循环会出现 a-b b-a 这两种刚好互补的情况
                    force(a.ID).Add(cl)
                    ' force(b.ID).Add(-cl)
                Next
            Next

            For Each l In E
                Dim a = l.u
                Dim b = l.v

                Dim d = a.Point - b.Point
                Dim springF = spring(d.SumMagnitude)
                Dim f = Math.AttractiveForce(springF, a.Point, b.Point)
                f.strength /= 15
                force(a.ID).Add(f)  ' 对一个是正向力，对另外一个节点就刚好反过来才会使正向力
                force(b.ID).Add(-f)
            Next

            Using g = New Size(2000, 2000).CreateGDIDevice

                Dim polygon = V.Select(Function(n)
                                           Try
                                               Return n.Point.Vector2D.ToPoint
                                           Catch ex As Exception
                                               Return New Point
                                           End Try
                                       End Function).ToArray
                Dim coffset = polygon.CentralOffset(g.Size).ToPoint



                For n As Integer = 0 To V.Length - 1
                    Dim u = polygon(n)
                    Call g.DrawCircle(u.OffSet2D(coffset), 10, Brushes.Blue)
                    Call cat(u.ToString & " ")
                    Try
                        Call V(n).ShowForce(g, force(V(n).ID), coffset)
                    Catch ex As Exception

                    End Try
                Next

                For Each uv In E
                    Try
                        Call g.DrawLine(Pens.Green, uv.u.Point.Vector2D.OffSet2D(coffset), uv.v.Point.Vector2D.OffSet2D(coffset))
                    Catch ex As Exception

                    End Try
                Next

                Call g.Save($"x:\fffff\f{i.FormatZero("00000")}.png", ImageFormats.Png)
                cat("\n")

                For Each u In V
                    Dim F = force(u.ID).Sum
                    F.strength /= 10

                    u += F
                    u.Displacement()
                    force(u.ID).Clear()
                Next
            End Using
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
    Public Function spring(d#) As Double
        Return c1 * Abs(d) / c2
    End Function

    Class edge
        Public u, v As node2

        Public Overrides Function ToString() As String
            Return u.ID & ", " & v.ID
        End Function
    End Class

    Public Class node2 : Inherits MassPoint

        Sub New()
            Point = New Vector(shorts:={Rnd() * 1000, Rnd() * 1000})
            Charge = 0.01
        End Sub

        Public Overrides Function ToString() As String
            Return Point.Vector2D.ToString ' & " --> " & force.GetJson
        End Function
    End Class
End Module
