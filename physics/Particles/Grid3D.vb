#Region "Microsoft.VisualBasic::de52ef65fc3ec5eb7e1116cda2b11694, gr\physics\Grid3D.vb"

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

    '   Total Lines: 84
    '    Code Lines: 43 (51.19%)
    ' Comment Lines: 28 (33.33%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 13 (15.48%)
    '     File Size: 3.38 KB


    ' Module Grid3D
    ' 
    '     Function: EncodeGrid3D, GetCell3D, PopulateLookups, SpatialLookup3D
    ' 
    ' Interface IContainer3D
    ' 
    '     Properties: BoxSize, Entity
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

''' <summary>
''' The 3D counterpart of the 2D <see cref="GridDynamics"/> spatial hash.
''' 
''' Particles are bucketed into cubic cells of side length equal to the SPH
''' smoothing radius. A neighbour query then only needs to visit the 27 cells
''' surrounding the query cell (3x3x3), which reduces the neighbour search
''' cost from O(n^2) down to roughly O(n * k).
''' </summary>
Public Module Grid3D

    ' the 3x3x3 = 27 neighbour cell offsets surrounding a center cell.
    ReadOnly lookups As (dx As Integer, dy As Integer, dz As Integer)() = PopulateLookups()

    Private Function PopulateLookups() As (dx As Integer, dy As Integer, dz As Integer)()
        Dim offsets As New List(Of (dx As Integer, dy As Integer, dz As Integer))

        For dz As Integer = -1 To 1
            For dy As Integer = -1 To 1
                For dx As Integer = -1 To 1
                    offsets.Add((dx, dy, dz))
                Next
            Next
        Next

        Return offsets.ToArray
    End Function

    ''' <summary>
    ''' compute the integer cell coordinate of a given position under the
    ''' given cell size(which equals to the SPH smoothing radius).
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetCell3D(v As Vector3, radius As Single) As (x As Integer, y As Integer, z As Integer)
        Return (CInt(System.Math.Floor(v.x / radius)), CInt(System.Math.Floor(v.y / radius)), CInt(System.Math.Floor(v.z / radius)))
    End Function

    ''' <summary>
    ''' Build the spatial hash of the given particle set, particles are grouped
    ''' into cubic cells keyed by their integer cell coordinate.
    ''' </summary>
    <Extension>
    Public Function EncodeGrid3D(field As IEnumerable(Of Particle3D), radius As Single) As Dictionary(Of (Integer, Integer, Integer), Particle3D())
        Return field _
            .GroupBy(Function(a) GetCell3D(a.PredictedPosition, radius)) _
            .ToDictionary(Function(a) a.Key,
                          Function(a) a.ToArray)
    End Function

    ''' <summary>
    ''' Enumerate all of the particles that located inside the 27 cells which
    ''' surrounding the target cell.
    ''' </summary>
    <Extension>
    Public Iterator Function SpatialLookup3D(grid As Dictionary(Of (Integer, Integer, Integer), Particle3D()),
                                             tar As (x As Integer, y As Integer, z As Integer)) As IEnumerable(Of Particle3D)
        Dim q As Particle3D() = Nothing
        Dim cx = tar.x, cy = tar.y, cz = tar.z

        For Each d As (dx As Integer, dy As Integer, dz As Integer) In lookups
            If grid.TryGetValue((cx + d.dx, cy + d.dy, cz + d.dz), q) Then
                For i As Integer = 0 To q.Length - 1
                    Yield q(i)
                Next
            End If
        Next
    End Function

End Module

''' <summary>
''' the 3D volume container of the particles(the 3D counterpart of the 2D
''' <see cref="IContainer(Of T)"/>).
''' </summary>
Public Interface IContainer3D(Of T)

    ReadOnly Property Entity As IReadOnlyCollection(Of T)
    ''' <summary>
    ''' the size(x/y/z extent) of the box volume that holds the fluid.
    ''' </summary>
    Property BoxSize As Vector3

End Interface
