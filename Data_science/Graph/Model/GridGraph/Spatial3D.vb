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

        ''' <summary>
        ''' get the grid index for each layer on z axis
        ''' </summary>
        ''' <returns></returns>
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