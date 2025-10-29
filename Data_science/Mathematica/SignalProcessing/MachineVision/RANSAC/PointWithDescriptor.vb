Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

''' <summary>
''' A point structure that includes its feature descriptor.
''' </summary>
Public Structure PointWithDescriptor

    Public Pt As PointF

    ''' <summary>
    ''' (distance to centroid, angle)
    ''' </summary>
    Public Descriptor As (r As Double, theta As Double)

    ''' <summary>
    ''' other property that used for weight score which is generated from the raw data
    ''' </summary>
    Public [properties] As Double()

    Public Overrides Function ToString() As String
        Return $"({Pt.X},{Pt.Y}) radius:{Descriptor.r}, theta:{Descriptor.theta}, properties:{properties.GetJson}"
    End Function

    ''' <summary>
    ''' Computes a simple (distance, angle) descriptor for each point relative to the polygon's centroid.
    ''' </summary>
    Public Shared Iterator Function ComputeDescriptors(poly As Polygon2D) As IEnumerable(Of PointWithDescriptor)
        If poly.length = 0 Then
            Return
        End If

        ' Calculate centroid (using only valid points)
        Dim centroid As PointF = poly.centroid
        Dim centroidX As Double = centroid.X
        Dim centroidY As Double = centroid.Y

        ' Compute descriptor for each point
        For i As Integer = 0 To poly.length - 1
            Dim pt As PointF = poly(i)
            Dim dx = pt.X - centroidX
            Dim dy = pt.Y - centroidY
            Dim r = std.Sqrt(dx * dx + dy * dy)
            Dim theta = std.Atan2(dy, dx)

            Yield New PointWithDescriptor With {
                .Pt = pt, .Descriptor = (r, theta)
            }
        Next
    End Function

    Public Shared Iterator Function ComputeDescriptors(Of T As Layout2D)(shape As IEnumerable(Of T), getMetadata As Func(Of T, Double())) As IEnumerable(Of PointWithDescriptor)
        Dim pool As T() = shape.SafeQuery.ToArray

        If pool.Length = 0 Then
            Return
        End If

        Dim poly As New Polygon2D(pool.X, pool.Y)
        ' Calculate centroid (using only valid points)
        Dim centroid As PointF = poly.centroid

        ' Compute descriptor for each point
        For i As Integer = 0 To poly.length - 1
            Dim pt As PointF = poly(i)
            Dim dx = pt.X - centroid.X
            Dim dy = pt.Y - centroid.Y
            Dim r = std.Sqrt(dx * dx + dy * dy)
            Dim theta = std.Atan2(dy, dx)

            Yield New PointWithDescriptor With {
                .Pt = pt,
                .Descriptor = (r, theta),
                .properties = getMetadata(pool(i))
            }
        Next
    End Function
End Structure
