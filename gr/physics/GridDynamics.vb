Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph

Public Module GridDynamics

    ' [x-1,y-1] [x, y-1] [x+1, y-1]
    ' [x-1, y]    [c]    [x+1, y]
    ' [x-1,y+1] [x,y+1] [x+1,y+1]
    ReadOnly lookups As (dx As Integer, dy As Integer)() = {
        (-1, -1), (0, -1), (+1, -1),
        (-1, 0), (0, 0), (+1, 0),
        (-1, +1), (0, +1), (+1, +1)
    }

    <Extension>
    Public Iterator Function SpatialLookup(Of T As Layout2D)(grid As Grid(Of T()), tar As T, radius As Single) As IEnumerable(Of T)
        Dim cx As Integer = tar.X / radius
        Dim cy As Integer = tar.Y / radius
        Dim q As T()

        For Each dxdy In lookups
            q = grid(cx + dxdy.dx, cy + dxdy.dy)

            If Not q Is Nothing Then
                For i As Integer = 0 To q.Length - 1
                    Yield q(i)
                Next
            End If
        Next
    End Function

    <Extension>
    Public Function EncodeGrid(Of T As Layout2D)(field As IContainer(Of T), radius As Single) As Grid(Of T())
        Dim groups = field.Entity _
            .Select(Function(a)
                        Dim cx As Integer = a.X / radius
                        Dim cy As Integer = a.Y / radius

                        Return (cx, cy, hash:=HashCell(cx, cy), a)
                    End Function) _
            .GroupBy(Function(a) a.hash) _
            .Select(Function(a) (a.Select(Function(ti) ti.a).ToArray, a.First.cx, a.First.cy))

        Return Grid(Of T()).Create(groups)
    End Function

    Private Function HashCell(cx As Integer, cy As Integer) As UInteger
        Dim a As UInteger = cx * 15823
        Dim b As UInteger = cy * 9737333
        Return a + b
    End Function

End Module

Public Interface IContainer(Of T As Layout2D)

    ReadOnly Property Entity As IReadOnlyCollection(Of T)

    ReadOnly Property Width As Double
    ReadOnly Property Height As Double

End Interface