Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
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
    ''' Generates a list of candidate matches by finding the nearest neighbor in descriptor space.
    ''' </summary>
    Public Shared Function GenerateCandidateMatches(ByRef sourceDesc As PointWithDescriptor(), ByRef targetDesc As PointWithDescriptor()) As List(Of (source As PointF, target As PointF))
        Dim matches As New List(Of (source As PointF, target As PointF))()
        Dim bar As Tqdm.ProgressBar = Nothing

        Call $"Generates a list of candidate matches by finding the nearest neighbor in descriptor space.".debug
        Call $"matrix size: [{sourceDesc.Length} x {targetDesc.Length}]".info

        For Each sPt As PointWithDescriptor In Tqdm.Wrap(sourceDesc, bar:=bar, wrap_console:=App.EnableTqdm)
            Dim q = targetDesc.AsParallel _
                .Select(Function(tPt As PointWithDescriptor)
                            ' Simple Euclidean distance in descriptor space (r, theta)
                            ' We might want to weight angle more than distance, but this is a start.
                            Dim dr = sPt.Descriptor.r - tPt.Descriptor.r
                            Dim dtheta = sPt.Descriptor.theta - tPt.Descriptor.theta
                            Dim pd As Double = If(sPt.properties.IsNullOrEmpty OrElse tPt.properties.IsNullOrEmpty, 0, sPt.properties.SquareDistance(tPt.properties))

                            ' Normalize angle difference
                            While dtheta > std.PI : dtheta -= 2 * std.PI : End While
                            While dtheta < -std.PI : dtheta += 2 * std.PI : End While

                            Dim distSq = dr * dr + dtheta * dtheta + pd * 0.5

                            Return (distSq, tPt)
                        End Function) _
                .ToArray
            Dim min = q.OrderBy(Function(a) a.distSq).First
            Dim minDist As Double = min.distSq
            Dim bestMatch As PointWithDescriptor = min.tPt

            If Not minDist.IsNaNImaginary Then
                Call matches.Add((sPt.Pt, bestMatch.Pt))
                Call bar.SetLabel($"({sPt.Pt.X},{sPt.Pt.Y}) = ({bestMatch.Pt.X},{bestMatch.Pt.Y}) [min sq_dist:{minDist}]")
            End If
        Next

        Call $"find {matches.Count} candidate matches!".debug

        Return matches
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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="shape"></param>
    ''' <param name="getMetadata">should be normalized to [0,1]</param>
    ''' <returns></returns>
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

    Public Shared Function NormalizeProperties(points As PointWithDescriptor()) As PointWithDescriptor()
        Dim max As Double() = New Double(points(0).properties.Length - 1) {}

        For Each pt As PointWithDescriptor In points
            Dim v As Double() = pt.properties

            For i As Integer = 0 To v.Length - 1
                If v(i) > max(i) Then
                    max(i) = v(i)
                End If
            Next
        Next

        For i As Integer = 0 To points.Length - 1
            points(i) = New PointWithDescriptor With {
                .Descriptor = points(i).Descriptor,
                .Pt = points(i).Pt,
                .properties = SIMD.Divide.f64_op_divide_f64(points(i).properties, max)
            }
        Next

        Return points
    End Function
End Structure
