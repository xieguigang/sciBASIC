#Region "Microsoft.VisualBasic::e55e764b3b47258ebcc549bfbbe07ccc, gr\physics\GridDynamics.vb"

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

    '   Total Lines: 71
    '    Code Lines: 52 (73.24%)
    ' Comment Lines: 3 (4.23%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (22.54%)
    '     File Size: 2.40 KB


    ' Module GridDynamics
    ' 
    '     Function: EncodeGrid, GetCell2D, HashCell, (+2 Overloads) SpatialLookup
    ' 
    ' Interface IContainer
    ' 
    '     Properties: Entity, Height, Width
    ' 
    ' /********************************************************************************/

#End Region

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
    Public Iterator Function SpatialLookup(Of T As Layout2D)(grid As Grid(Of T()), tar As (x As Integer, y As Integer)) As IEnumerable(Of T)
        Dim q As T()
        Dim cx = tar.x
        Dim cy = tar.y

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
    Public Function SpatialLookup(Of T As Layout2D)(grid As Grid(Of T()), tar As T, radius As Single) As IEnumerable(Of T)
        Return grid.SpatialLookup((CInt(tar.X / radius), CInt(tar.Y / radius)))
    End Function

    Public Function GetCell2D(v As Layout2D, radius As Single) As (x As Integer, y As Integer)
        Return (CInt(v.X / radius), CInt(v.Y / radius))
    End Function

    <Extension>
    Public Function EncodeGrid(Of T As Layout2D)(field As IContainer(Of T), radius As Single) As Grid(Of T())
        Return field.Entity.EncodeGrid(radius)
    End Function

    <Extension>
    Public Function EncodeGrid(Of T As Layout2D)(field As IEnumerable(Of T), radius As Single) As Grid(Of T())
        Dim groups = field _
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
        Dim a As UInteger = cx * 15823UI
        Dim b As UInteger = cy * 9737333UI
        Return a + b
    End Function

End Module

Public Interface IContainer(Of T As Layout2D)

    ReadOnly Property Entity As IReadOnlyCollection(Of T)

    ReadOnly Property Width As Double
    ReadOnly Property Height As Double

End Interface
