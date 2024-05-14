#Region "Microsoft.VisualBasic::e38a2264d159429d7192cdf3f47121dd, Microsoft.VisualBasic.Core\src\Extensions\Image\Math\VectorMath2D.vb"

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

    '   Total Lines: 182
    '    Code Lines: 163
    ' Comment Lines: 3
    '   Blank Lines: 16
    '     File Size: 8.79 KB


    '     Class VectorMath2D
    ' 
    '         Function: absAngle, absAngleDeg, angle, angleDeg, (+3 Overloads) createPointAuto
    '                   innerProduct, isNear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports std = System.Math

Namespace Imaging.Math2D

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class VectorMath2D

        Public Shared Function absAngle(v1 As Vector2D, v2 As Vector2D) As Double
            Dim d1 As Double = v1.Length()
            Dim d2 As Double = v2.Length()
            If d1 = 0.0 Then
                Return 0.0
            End If
            If d2 = 0.0 Then
                Return 0.0
            End If
            Dim d3 As Double = innerProduct(v1, v2) / d1 / d2
            If d3 >= 1.0 Then
                d3 = 1.0
            End If
            If d3 <= -1.0 Then
                d3 = -1.0
            End If
            Return std.Acos(d3)
        End Function

        Public Shared Function absAngleDeg(v1 As Vector2D, v2 As Vector2D) As Double
            Return absAngle(v1, v2) * 180.0 / 3.14159265358979
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
            Return angle(paramVector2D1, paramVector2D2) * 180.0 / std.PI
        End Function

        Public Shared Function innerProduct(paramVector2D1 As Vector2D, paramVector2D2 As Vector2D) As Double
            Return paramVector2D1.x * paramVector2D2.x + paramVector2D1.y * paramVector2D2.y
        End Function

        Public Shared Function isNear(paramDouble1 As Double,
                                      paramDouble2 As Double,
                                      paramDouble3 As Double,
                                      paramDouble4 As Double,
                                      paramDouble5 As Double,
                                      paramDouble6 As Double,
                                      paramDouble7 As Double) As Boolean

            If std.Min(paramDouble1, paramDouble3) > paramDouble5 + paramDouble7 Then
                Return False
            End If
            If std.Max(paramDouble1, paramDouble3) < paramDouble5 - paramDouble7 Then
                Return False
            End If
            If std.Min(paramDouble2, paramDouble4) > paramDouble6 + paramDouble7 Then
                Return False
            End If
            If std.Max(paramDouble2, paramDouble4) < paramDouble6 - paramDouble7 Then
                Return False
            End If
            Dim d1 As Double = paramDouble4 - paramDouble2
            Dim d2 As Double = paramDouble1 - paramDouble3
            Dim d3 As Double = paramDouble3 * paramDouble2 - paramDouble1 * paramDouble4
            Dim d4 As Double = (d1 * paramDouble5 + d2 * paramDouble6 + d3) * (d1 * paramDouble5 + d2 * paramDouble6 + d3) / (d1 * d1 + d2 * d2)
            Return d4 < paramDouble7 * paramDouble7
        End Function

        Public Shared Function createPointAuto(dimension1 As Rectangle2D, dimension2 As Rectangle2D, paramInt As Integer) As Rectangle2D
            Dim localDimension As New Rectangle2D()
            Dim i As Integer = dimension2.Width - dimension1.Width
            Dim j As Integer = dimension2.Height - dimension1.Height
            Dim d3 As Double = std.Sqrt(i * i + j * j)
            Dim d1 As Double = 0.8660254 * i - 0.5 * j
            Dim d2 As Double = -0.8660254 * i - 0.5 * j

            If std.Abs(d1) < std.Abs(d2) Then
                dimension1.Width += CInt(std.Truncate((-0.5 * i - 0.8660254 * j) * paramInt / d3))
                dimension1.Height += CInt(std.Truncate(d1 * paramInt / d3))
            Else
                dimension1.Width += CInt(std.Truncate((-0.5 * i + 0.8660254 * j) * paramInt / d3))
                dimension1.Height += CInt(std.Truncate(d2 * paramInt / d3))
            End If

            Return localDimension
        End Function

        Public Shared Function createPointAuto(paramDimension1 As Rectangle2D, paramDimension2 As Rectangle2D, paramDimension3 As Rectangle2D, paramInt As Integer) As Rectangle2D
            Dim localDimension As New Rectangle2D()
            Dim localVector2D2 As New Vector2D(paramDimension2.Width - paramDimension1.Width, paramDimension2.Height - paramDimension1.Height)
            Dim localVector2D3 As New Vector2D(paramDimension3.Width - paramDimension1.Width, paramDimension3.Height - paramDimension1.Height)
            localVector2D2 = localVector2D2 * (1.0 / localVector2D2.Length())
            localVector2D3 = localVector2D3 * (1.0 / localVector2D3.Length())
            Dim localVector2D1 As Vector2D = localVector2D2 + localVector2D3
            localVector2D1 = localVector2D1 * (paramInt / localVector2D1.Length())
            paramDimension1.Width -= CInt(std.Truncate(localVector2D1.x))
            paramDimension1.Height -= CInt(std.Truncate(localVector2D1.y))
            Return localDimension
        End Function

        Public Shared Function createPointAuto(paramDimension As Rectangle2D, paramVector As ArrayList, paramInt As Integer) As Rectangle2D
            Dim localDimension As New Rectangle2D()
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
            Dim localVector2D3 As Vector2D

            While i > 0
                localVector2D2 = New Vector2D(
                    DirectCast(paramVector(j), Rectangle2D).Width - paramDimension.Width,
                    DirectCast(paramVector(j), Rectangle2D).Height - paramDimension.Height)
                d2 = 360.0
                m = -1
                For k = 0 To paramVector.Count - 1
                    If (arrayOfBoolean(k) = 0) AndAlso (j <> k) Then
                        localVector2D3 = New Vector2D(
                            DirectCast(paramVector(k), Rectangle2D).Width - paramDimension.Width,
                            DirectCast(paramVector(k), Rectangle2D).Height - paramDimension.Height)
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
            localVector2D2 = New Vector2D(DirectCast(paramVector(n), Rectangle2D).Width - paramDimension.Width, DirectCast(paramVector(n), Rectangle2D).Height - paramDimension.Height)
            localVector2D3 = New Vector2D(DirectCast(paramVector(i1), Rectangle2D).Width - paramDimension.Width, DirectCast(paramVector(i1), Rectangle2D).Height - paramDimension.Height)
            localVector2D2 = localVector2D2 * (1.0 / localVector2D2.Length())
            localVector2D3 = localVector2D3 * (1.0 / localVector2D3.Length())
            Dim localVector2D1 As Vector2D = localVector2D2 + localVector2D3
            localVector2D1 = localVector2D1 * (paramInt / localVector2D1.Length())
            paramDimension.Width += CInt(std.Truncate(localVector2D1.x))
            paramDimension.Height += CInt(std.Truncate(localVector2D1.y))
            Return localDimension
        End Function
    End Class
End Namespace
