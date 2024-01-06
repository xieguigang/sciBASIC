Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Drawing2D.HeatMap

    Public Class RasterMatrix : Implements IRasterGrayscaleHeatmap

        Dim m As GeneralMatrix

        ''' <summary>
        ''' is dense matrix?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DenseMatrix As Boolean
            Get
                Return Not TypeOf m Is SparseMatrix
            End Get
        End Property

        Sub New(m As GeneralMatrix)
            Me.m = m
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRowScans() As IEnumerable(Of Vector)
            Return m.RowVectors
        End Function

        Public Shared Function CreateDenseMatrix(Of T As Pixel)(datas As IEnumerable(Of T), w As Integer, h As Integer) As RasterMatrix
            Dim data As Double
            Dim matrix As Dictionary(Of UInteger, Dictionary(Of UInteger, Double)) = GetIndexedScales(datas)
            Dim m As New NumericMatrix(h, w)

            For i As UInteger = 0 To CUInt(w) - 1
                For j As UInteger = 0 To CUInt(h) - 1
                    data = 0

                    If matrix.ContainsKey(i) Then
                        If matrix(key:=i).ContainsKey(j) Then
                            data = matrix(key:=i)(key:=j)
                        End If
                    End If

                    m(i, j) = data
                Next
            Next

            Return New RasterMatrix(m)
        End Function

        Public Iterator Function GetRasterPixels() As IEnumerable(Of Pixel) Implements IRasterGrayscaleHeatmap.GetRasterPixels
            Dim y As Integer = 0

            For Each row As Vector In m.RowVectors
                Dim v = row.Array

                For x As Integer = 0 To v.Length - 1
                    Yield New PixelData With {
                        .Scale = v(x),
                        .X = x,
                        .Y = y
                    }
                Next

                y += 1
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function MatrixRows(Of T As Pixel)(cols As IEnumerable(Of T)) As Dictionary(Of UInteger, Double)
            Return cols _
                .GroupBy(Function(b) b.Y) _
                .ToDictionary(Function(y) CUInt(y.Key),
                            Function(g)
                                If g.Count = 1 Then
                                    Return g.First.Scale
                                Else
                                    Return g.Average(Function(p) p.Scale)
                                End If
                            End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function GetIndexedScales(Of T As Pixel)(data As IEnumerable(Of T)) As Dictionary(Of UInteger, Dictionary(Of UInteger, Double))
            Return data _
                .GroupBy(Function(a) a.X) _
                .ToDictionary(Function(x) CUInt(x.Key),
                              Function(a)
                                  Return MatrixRows(a)
                              End Function)
        End Function

        ''' <summary>
        ''' cast the sparse raster data as dense matrx liked raster data
        ''' </summary>
        ''' <param name="datas"></param>
        ''' <param name="w">the matrix width, ncols</param>
        ''' <param name="h">the matrix height, nrows</param>
        ''' <returns>the full scan <see cref="PixelData"/> spot data, number of 
        ''' the returns collection equals to <paramref name="w"/> * <paramref name="h"/>.
        ''' </returns>
        Public Shared Iterator Function PopulateDenseRasterMatrix(Of T As Pixel)(datas As IEnumerable(Of T), w As Integer, h As Integer) As IEnumerable(Of Pixel)
            Dim data As Double
            Dim matrix As Dictionary(Of UInteger, Dictionary(Of UInteger, Double)) = GetIndexedScales(datas)

            For i As UInteger = 0 To CUInt(w) - 1
                For j As UInteger = 0 To CUInt(h) - 1
                    data = 0

                    If matrix.ContainsKey(i) Then
                        If matrix(key:=i).ContainsKey(j) Then
                            data = matrix(key:=i)(key:=j)
                        End If
                    End If

                    Yield New PixelData With {
                        .X = i,
                        .Y = j,
                        .Scale = data
                    }
                Next
            Next
        End Function
    End Class
End Namespace