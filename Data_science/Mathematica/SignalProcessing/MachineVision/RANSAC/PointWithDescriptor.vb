Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Math2D
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
End Structure
