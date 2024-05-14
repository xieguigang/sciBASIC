#Region "Microsoft.VisualBasic::07bdbe1d3d2880e4c064a2630343ebfc, Data_science\Graph\Model\GridGraph\Spatial3D.vb"

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

    '   Total Lines: 134
    '    Code Lines: 89
    ' Comment Lines: 27
    '   Blank Lines: 18
    '     File Size: 5.29 KB


    '     Class Spatial3D
    ' 
    '         Properties: size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) CreateSpatial3D, GetData, GetDimensions, Query, ZLayers
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq

Namespace GridGraph

    ''' <summary>
    ''' a generic grid graph for fast query of the 3D geometry data
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Spatial3D(Of T)

        ''' <summary>
        ''' [z => 2d grid space]
        ''' </summary>
        ReadOnly matrix2D As Dictionary(Of Long, Grid(Of T))
        ReadOnly toPoint As Func(Of T, SpatialIndex3D)

        ''' <summary>
        ''' counts of all non-empty cell.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Aggregate layer As Grid(Of T)
                       In matrix2D.Values.AsParallel
                       Into Sum(layer.size)
            End Get
        End Property

        Default Public ReadOnly Property GetPoint(x As Integer, y As Integer, z As Integer) As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetData(x, y, z)
            End Get
        End Property

        Private Sub New(matrix2D As Dictionary(Of Long, Grid(Of T)), toPoint As Func(Of T, SpatialIndex3D))
            Me.matrix2D = matrix2D
            Me.toPoint = toPoint
        End Sub

        Public Function GetDimensions() As (xdims As Integer, ydims As Integer, zdims As Integer)
            Dim z As Integer = matrix2D.Keys.Max
            Dim x As Integer = Aggregate layer As Grid(Of T) In matrix2D.Values Into Max(layer.width)
            Dim y As Integer = Aggregate layer As Grid(Of T) In matrix2D.Values Into Max(layer.height)

            Return (x, y, z)
        End Function

        ''' <summary>
        ''' implements the 3d spatial data lookup helper
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="z"></param>
        ''' <param name="hit"></param>
        ''' <returns></returns>
        Public Function GetData(x As Integer, y As Integer, z As Integer, Optional ByRef hit As Boolean = False) As T
            Dim zl As Long = CLng(z)

            If Not matrix2D.ContainsKey(zl) Then
                hit = False
                Return Nothing
            Else
                Return matrix2D(zl).GetData(x, y, hit)
            End If
        End Function

        Public Iterator Function Query(x As Integer, y As Integer, z As Integer) As IEnumerable(Of T)
            Dim hit As Boolean = False
            Dim result As T

            For xi As Integer = x - 1 To x + 1
                For yi As Integer = y - 1 To y + 1
                    For zi As Integer = z - 1 To z + 1
                        hit = False
                        result = GetData(xi, yi, zi, hit)

                        If hit Then
                            Yield result
                        End If
                    Next
                Next
            Next
        End Function

        ''' <summary>
        ''' get the grid index for each layer on z axis
        ''' </summary>
        ''' <returns>a collection of the 2d grid object which are re-order by z axis
        ''' from zero to max(z). order in asc</returns>
        ''' <remarks>
        ''' the populate out layer has already been re-ordered by the z-axis order.
        ''' </remarks>
        Public Iterator Function ZLayers() As IEnumerable(Of Grid(Of T))
            For Each item In matrix2D.OrderBy(Function(lz) lz.Key)
                Yield item.Value
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateSpatial3D(Of E As {T, IPoint3D})(data As IEnumerable(Of T)) As Spatial3D(Of E)
            Return Spatial3D(Of E).CreateSpatial3D(data, Function(a) a.X, Function(a) a.Y, Function(a) a.Z)
        End Function

        Public Shared Function CreateSpatial3D(data As IEnumerable(Of T),
                                               getX As Func(Of T, Integer),
                                               getY As Func(Of T, Integer),
                                               getZ As Func(Of T, Integer)) As Spatial3D(Of T)
            Dim gridData As IEnumerable(Of T)
            Dim layers = data.SafeQuery _
                .Select(Function(a) (a, getZ(a))) _
                .GroupBy(Function(a) a.Item2)
            Dim z_index As New Dictionary(Of Long, Grid(Of T))

            For Each layer In layers
                gridData = layer.Select(Function(s) s.a)
                z_index.Add(layer.Key, Grid(Of T).Create(gridData, getX, getY))
            Next

            Return New Spatial3D(Of T)(
                matrix2D:=z_index,
                toPoint:=Function(s)
                             Return New SpatialIndex3D With {
                                .X = getX(s),
                                .Y = getY(s),
                                .Z = getZ(s)
                             }
                         End Function)
        End Function
    End Class
End Namespace
