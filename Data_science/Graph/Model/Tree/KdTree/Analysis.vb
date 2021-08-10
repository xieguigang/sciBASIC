Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations.DistanceMethods
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace KdTree

    ''' <summary>
    ''' K Nearest Neighbour Search
    ''' 
    ''' Uses a kd-tree to find the p number of near neighbours for each point in an input/output dataset.
    ''' </summary>
    Public Module ApproximateNearNeighbor

        Public Function FindNeighbors(data As GeneralMatrix, Optional k As Integer = 30) As IEnumerable(Of (size As Integer, indices As Integer(), weights As Double()))
            Return data _
                .RowVectors _
                .SeqIterator _
                .Select(Function(d)
                            Return New TagVector With {.index = d.i, .vector = CType(d, Vector).ToArray}
                        End Function) _
                .FindNeighbors(k)
        End Function

        <Extension>
        Public Iterator Function FindNeighbors(data As IEnumerable(Of TagVector), Optional k As Integer = 30) As IEnumerable(Of (size As Integer, indices As Integer(), weights As Double()))
            Dim allData As TagVector() = data.ToArray
            Dim tree As New KdTree(Of TagVector)(allData, RowMetric(ncols:=allData(Scan0).size))

            For Each row As TagVector In allData
                Dim nn2 = tree _
                    .nearest(row, maxNodes:=k) _
                    .OrderBy(Function(i) i.distance) _
                    .ToArray
                Dim index As Integer() = nn2.Select(Function(xi) xi.node.data.index).ToArray
                Dim weights As Double() = nn2.Select(Function(xi) xi.distance).ToArray

                Yield (index.Length, index, weights)
            Next
        End Function

        Private Function RowMetric(ncols As Integer) As KdNodeAccessor(Of TagVector)
            Return New VectorAccessor(ncols)
        End Function

    End Module

    Public Structure TagVector

        Dim index As Integer
        Dim vector As Double()

        Public ReadOnly Property size As Integer
            Get
                Return vector.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{index}] {vector.Take(6).JoinBy(", ")}..."
        End Function

    End Structure

    Friend Class VectorAccessor : Inherits KdNodeAccessor(Of TagVector)

        Dim dims As Dictionary(Of String, Integer)
        Dim dimKeys As String()

        ''' <summary>
        ''' create an accessor for access the n-dimension vector
        ''' </summary>
        ''' <param name="m"></param>
        Sub New(m As Integer)
            dims = m _
                .Sequence _
                .ToDictionary(Function(k) k.ToString,
                              Function(k)
                                  Return k
                              End Function)
            dimKeys = dims.Keys.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub setByDimensin(x As TagVector, dimName As String, value As Double)
            x.vector(dims(dimName)) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDimensions() As String()
            Return dimKeys
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function metric(a As TagVector, b As TagVector) As Double
            Return a.vector.EuclideanDistance(b.vector)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getByDimension(x As TagVector, dimName As String) As Double
            Return x.vector(dims(dimName))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function nodeIs(a As TagVector, b As TagVector) As Boolean
            Return a.index = b.index
        End Function

        Public Overrides Function activate() As TagVector
            Return New TagVector With {.vector = 0.0.Repeats(times:=dims.Count).ToArray, .index = -1}
        End Function
    End Class
End Namespace