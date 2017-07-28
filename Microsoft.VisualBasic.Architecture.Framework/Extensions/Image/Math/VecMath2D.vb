Imports sys = System.Math

Namespace Imaging.Math2D


    Public Class VecMath2D
        Public Shared Function add(paramVector2D1 As Vector2D, paramVector2D2 As Vector2D) As Vector2D
            Return New Vector2D(paramVector2D1.x + paramVector2D2.x, paramVector2D1.y + paramVector2D2.y)
        End Function

        Public Shared Function subtract(paramVector2D1 As Vector2D, paramVector2D2 As Vector2D) As Vector2D
            Return New Vector2D(paramVector2D1.x - paramVector2D2.x, paramVector2D1.y - paramVector2D2.y)
        End Function

        Public Shared Function length(paramVector2D As Vector2D) As Double
            Return Math.Sqrt(paramVector2D.x * paramVector2D.x + paramVector2D.y * paramVector2D.y)
        End Function

        Public Shared Function absAngle(paramVector2D1 As Vector2D, paramVector2D2 As Vector2D) As Double
            Dim d1 As Double = paramVector2D1.length()
            Dim d2 As Double = paramVector2D2.length()
            If d1 = 0.0 Then
                Return 0.0
            End If
            If d2 = 0.0 Then
                Return 0.0
            End If
            Dim d3 As Double = innerProduct(paramVector2D1, paramVector2D2) / d1 / d2
            If d3 >= 1.0 Then
                d3 = 1.0
            End If
            If d3 <= -1.0 Then
                d3 = -1.0
            End If
            Return Math.Acos(d3)
        End Function

        Public Shared Function absAngleDeg(paramVector2D1 As Vector2D, paramVector2D2 As Vector2D) As Double
            Return absAngle(paramVector2D1, paramVector2D2) * 180.0 / 3.14159265358979
        End Function

        Public Shared Function angle(paramVector2D1 As Vector2D, paramVector2D2 As Vector2D) As Double
            Dim d2 As Double = absAngle(paramVector2D1, paramVector2D2)
            If paramVector2D1.y = 0.0 Then
                If paramVector2D1.x > 0.0 Then
                    If paramVector2D2.y < 0.0 Then
                        Return d2
                    End If
                    Return -d2
                End If
                If paramVector2D2.y < 0.0 Then
                    Return -d2
                End If
                Return d2
            End If
            Dim d1 As Double = paramVector2D2.y * paramVector2D1.x - paramVector2D2.x * paramVector2D1.y
            If d1 < 0.0 Then
                Return d2
            End If
            Return -d2
        End Function

        Public Shared Function angleDeg(paramVector2D1 As Vector2D, paramVector2D2 As Vector2D) As Double
            Return angle(paramVector2D1, paramVector2D2) * 180.0 / 3.14159265358979
        End Function

        Public Shared Function innerProduct(paramVector2D1 As Vector2D, paramVector2D2 As Vector2D) As Double
            Return paramVector2D1.x * paramVector2D2.x + paramVector2D1.y * paramVector2D2.y
        End Function

        Public Shared Function isNear(paramDouble1 As Double, paramDouble2 As Double, paramDouble3 As Double, paramDouble4 As Double, paramDouble5 As Double, paramDouble6 As Double,
            paramDouble7 As Double) As Boolean
            If sys.Min(paramDouble1, paramDouble3) > paramDouble5 + paramDouble7 Then
                Return False
            End If
            If Math.Max(paramDouble1, paramDouble3) < paramDouble5 - paramDouble7 Then
                Return False
            End If
            If sys.Min(paramDouble2, paramDouble4) > paramDouble6 + paramDouble7 Then
                Return False
            End If
            If Math.Max(paramDouble2, paramDouble4) < paramDouble6 - paramDouble7 Then
                Return False
            End If
            Dim d1 As Double = paramDouble4 - paramDouble2
            Dim d2 As Double = paramDouble1 - paramDouble3
            Dim d3 As Double = paramDouble3 * paramDouble2 - paramDouble1 * paramDouble4
            Dim d4 As Double = (d1 * paramDouble5 + d2 * paramDouble6 + d3) * (d1 * paramDouble5 + d2 * paramDouble6 + d3) / (d1 * d1 + d2 * d2)
            Return d4 < paramDouble7 * paramDouble7
        End Function

        Public Shared Function createPointAuto(paramDimension1 As java.awt.Dimension, paramDimension2 As java.awt.Dimension, paramInt As Integer) As java.awt.Dimension
            Dim localDimension As New java.awt.Dimension()
            Dim i As Integer = paramDimension2.width - paramDimension1.width
            Dim j As Integer = paramDimension2.height - paramDimension1.height
            Dim d3 As Double = Math.Sqrt(i * i + j * j)
            Dim d1 As Double = 0.8660254 * i - 0.5 * j
            Dim d2 As Double = -0.8660254 * i - 0.5 * j
            If Math.Abs(d1) < Math.Abs(d2) Then
                paramDimension1.width += CInt(Math.Truncate((-0.5 * i - 0.8660254 * j) * paramInt / d3))
                paramDimension1.height += CInt(Math.Truncate(d1 * paramInt / d3))
            Else
                paramDimension1.width += CInt(Math.Truncate((-0.5 * i + 0.8660254 * j) * paramInt / d3))
                paramDimension1.height += CInt(Math.Truncate(d2 * paramInt / d3))
            End If
            Return localDimension
        End Function

        Public Shared Function createPointAuto(paramDimension1 As java.awt.Dimension, paramDimension2 As java.awt.Dimension, paramDimension3 As java.awt.Dimension, paramInt As Integer) As java.awt.Dimension
            Dim localDimension As New java.awt.Dimension()
            Dim localVector2D2 As New Vector2D(paramDimension2.width - paramDimension1.width, paramDimension2.height - paramDimension1.height)
            Dim localVector2D3 As New Vector2D(paramDimension3.width - paramDimension1.width, paramDimension3.height - paramDimension1.height)
            localVector2D2 = localVector2D2.multiple(1.0 / localVector2D2.length())
            localVector2D3 = localVector2D3.multiple(1.0 / localVector2D3.length())
            Dim localVector2D1 As Vector2D = add(localVector2D2, localVector2D3)
            localVector2D1 = localVector2D1.multiple(paramInt / localVector2D1.length())
            paramDimension1.width -= CInt(Math.Truncate(localVector2D1.x))
            paramDimension1.height -= CInt(Math.Truncate(localVector2D1.y))
            Return localDimension
        End Function

        Public Shared Function createPointAuto(paramDimension As java.awt.Dimension, paramVector As ArrayList, paramInt As Integer) As java.awt.Dimension
            Dim localDimension As New java.awt.Dimension()
            Dim d1 As Double = 360.0
            Dim d2 As Double = 360.0
            Dim d3 As Double = 0.0
            Dim i As Integer = paramVector.Count
            Dim j As Integer = 0
            Dim k As Integer = 1
            Dim m As Integer = -1
            Dim n As Integer = -1
            Dim i1 As Integer = -1
            Dim arrayOfBoolean As Boolean() = New Boolean(paramVector.Count - 1) {}
            For i2 As Integer = 0 To paramVector.Count - 1
                arrayOfBoolean(i2) = False
            Next

            Dim localVector2D2 As Vector2D

            While i > 0
                localVector2D2 = New Vector2D(DirectCast(paramVector(j), java.awt.Dimension).width - paramDimension.width, DirectCast(paramVector(j), java.awt.Dimension).height - paramDimension.height)
                d2 = 360.0
                m = -1
                For k = 0 To paramVector.Count - 1
                    If (arrayOfBoolean(k) = 0) AndAlso (j <> k) Then
                        localVector2D3 = New Vector2D(DirectCast(paramVector(k), java.awt.Dimension).width - paramDimension.width, DirectCast(paramVector(k), java.awt.Dimension).height - paramDimension.height)
                        d1 = angleDeg(localVector2D2, localVector2D3)
                        If d1 < 0.0 Then
                            d1 += 360.0
                        End If
                        If d1 < d2 Then
                            m = k
                            d2 = d1
                        End If
                    End If
                Next
                If d3 < d2 Then
                    n = j
                    i1 = m
                    d3 = d2
                End If
                j = m
                arrayOfBoolean(m) = True
                i -= 1
            End While
            localVector2D2 = New Vector2D(DirectCast(paramVector(n), java.awt.Dimension).width - paramDimension.width, DirectCast(paramVector(n), java.awt.Dimension).height - paramDimension.height)
            Dim localVector2D3 As New Vector2D(DirectCast(paramVector(i1), java.awt.Dimension).width - paramDimension.width, DirectCast(paramVector(i1), java.awt.Dimension).height - paramDimension.height)
            localVector2D2 = localVector2D2.multiple(1.0 / localVector2D2.length())
            localVector2D3 = localVector2D3.multiple(1.0 / localVector2D3.length())
            Dim localVector2D1 As Vector2D = add(localVector2D2, localVector2D3)
            localVector2D1 = localVector2D1.multiple(paramInt / localVector2D1.length())
            paramDimension.width += CInt(Math.Truncate(localVector2D1.x))
            paramDimension.height += CInt(Math.Truncate(localVector2D1.y))
            Return localDimension
        End Function
    End Class
End Namespace
