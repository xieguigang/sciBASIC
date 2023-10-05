Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph

Public Module GridDynamics

    <Extension>
    Public Function SpatialLookup(Of T As Layout2D)(grid As Grid(Of T()), tar As T, radius As Single) As IEnumerable(Of T)

    End Function

    <Extension>
    Public Function EncodeGrid(Of T As Layout2D)(field As IContainer(Of T), radius As Single) As Grid(Of T())
        Dim sq_w As Single = radius * 2
        Dim groups = field.Entity _
            .Select(Function(a)
                        Dim cx As Integer = a.X / sq_w
                        Dim cy As Integer = a.Y / sq_w

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