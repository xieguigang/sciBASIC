
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.IsoMetric

    Public Class Path3D

        Public Property Points As Point3D()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Depth() As Double
            Get
                Dim length As Integer = Me.Points.Length
                Dim total As Double = 0

                For i As Integer = 0 To length - 1
                    total += Me.Points(i).Depth()
                Next

                If length = 0 Then
                    Return total
                Else
                    Return total / length
                End If
            End Get
        End Property

        Public Sub New()
        End Sub

        Public Sub New(points As Point3D())
            Me.Points = points
        End Sub

        Public Sub Push(___point As Point3D)
            If Points Is Nothing Then Points = New Point3D() {}
            Points = add(___point, Points)
        End Sub

        Public Shared Function add(___point As Point3D, values As Point3D()) As Point3D()
            Dim anotherArray As Point3D() = New Point3D(values.Length) {}
            Array.Copy(values, 0, anotherArray, 0, values.Length)
            anotherArray(values.Length) = ___point
            Return anotherArray
        End Function

        Private Shared Function concat(a As Point3D(), b As Point3D()) As Point3D()
            Dim c As Point3D() = New Point3D(a.Length + b.Length - 1) {}
            Array.Copy(a, 0, c, 0, a.Length)
            Array.Copy(b, 0, c, a.Length, b.Length)
            Return c
        End Function

        Private Shared Function reverseArray(array As Point3D()) As Point3D()
            If array.Length = 0 Then
                Return {}
            ElseIf array.Length = 1 Then
                Return array
            Else
                ' recursion: concatenate the reverse of the end of the array
                ' to the first element (put at the end)
                Return concat(reverseArray(java.util.Arrays.copyOfRange(array, 1, array.Length)), New Point3D() {array(0)})
            End If
        End Function

        ''' <summary>
        ''' Returns a new path with the points in reverse order
        ''' </summary>
        Public Function Reverse() As Path3D
            Return New Path3D(reverseArray(Me.Points))
        End Function

        Public Function Translate(dx As Double, dy As Double, dz As Double) As Path3D
            Dim ___points As Point3D() = New Point3D(Me.Points.Length - 1) {}
            Dim ___point As Point3D
            For i As Integer = 0 To Me.Points.Length - 1
                ___point = Me.Points(i)
                ___points(i) = ___point.Translate(dx, dy, dz)
            Next i
            Return New Path3D(___points)
        End Function

        Public Function rotateX(origin As Point3D, angle As Double) As Path3D
            Dim ___points As Point3D() = New Point3D(Me.Points.Length - 1) {}
            Dim ___point As Point3D
            For i As Integer = 0 To Me.Points.Length - 1
                ___point = Me.Points(i)
                ___points(i) = ___point.RotateX(origin, angle)
            Next i
            Return New Path3D(___points)
        End Function

        Public Function rotateY(origin As Point3D, angle As Double) As Path3D
            Dim ___points As Point3D() = New Point3D(Me.Points.Length - 1) {}
            Dim ___point As Point3D
            For i As Integer = 0 To Me.Points.Length - 1
                ___point = Me.Points(i)
                ___points(i) = ___point.RotateY(origin, angle)
            Next i
            Return New Path3D(___points)
        End Function

        Public Function rotateZ(origin As Point3D, angle As Double) As Path3D
            Dim ___points As Point3D() = New Point3D(Me.Points.Length - 1) {}
            Dim ___point As Point3D
            For i As Integer = 0 To Me.Points.Length - 1
                ___point = Me.Points(i)
                ___points(i) = ___point.RotateZ(origin, angle)
            Next i
            Return New Path3D(___points)
        End Function

        Public Function Scale(origin As Point3D, dx As Double, dy As Double, dz As Double) As Path3D
            Dim ___points As Point3D() = New Point3D(Me.Points.Length - 1) {}
            Dim ___point As Point3D
            For i As Integer = 0 To Me.Points.Length - 1
                ___point = Me.Points(i)
                ___points(i) = ___point.Scale(origin, dx, dy, dz)
            Next i
            Return New Path3D(___points)
        End Function

        Public Function Scale(origin As Point3D, dx As Double, dy As Double) As Path3D
            Dim ___points As Point3D() = New Point3D(Me.Points.Length - 1) {}
            Dim ___point As Point3D
            For i As Integer = 0 To Me.Points.Length - 1
                ___point = Me.Points(i)
                ___points(i) = ___point.Scale(origin, dx, dy)
            Next i
            Return New Path3D(___points)
        End Function

        Public Function Scale(origin As Point3D, dx As Double) As Path3D
            Dim ___points As Point3D() = New Point3D(Me.Points.Length - 1) {}
            Dim ___point As Point3D
            For i As Integer = 0 To Me.Points.Length - 1
                ___point = Me.Points(i)
                ___points(i) = ___point.Scale(origin, dx)
            Next i
            Return New Path3D(___points)
        End Function

        Public Function TranslatePoints(dx As Double, dy As Double, dz As Double) As Path3D
            Dim ___point As Point3D
            For i As Integer = 0 To Me.Points.Length - 1
                ___point = Me.Points(i)
                Points(i) = ___point.Translate(dx, dy, dz)
            Next i
            Return Me
        End Function

        ''' <summary>
        ''' If pathB ("this") is closer from the observer than pathA, it must be drawn after.
        ''' It is closer if one of its vertices and the observer are on the same side of the plane defined by pathA.
        ''' </summary>
        Public Function CloserThan(pathA As Path3D, observer As Point3D) As Integer
            Return pathA.CountCloserThan(Me, observer) - Me.CountCloserThan(pathA, observer)
        End Function

        ''' <summary>
        ''' The plane containing <paramref name="pathA"/> is defined by the three points A, B, C
        ''' </summary>
        ''' <param name="pathA"></param>
        ''' <param name="observer"></param>
        ''' <returns></returns>
        Public Function CountCloserThan(pathA As Path3D, observer As Point3D) As Integer
            Dim AB As Point3D = pathA.Points(0) - pathA.Points(1)
            Dim AC As Point3D = pathA.Points(0) - pathA.Points(2)
            Dim n As Point3D = VectorMath.CrossProduct(AB, AC)

            Dim OA As Point3D = Math3D.Transformation.ORIGIN - pathA.Points(0)
            Dim OU As Point3D = Math3D.Transformation.ORIGIN - observer ' U = user = observer

            ' Plane defined by pathA such as ax + by + zc = d
            ' Here d = nx*x + ny*y + nz*z = n.OA
            Dim d As Double = n.DotProduct(OA)
            Dim observerPosition As Double = n.dotProduct(OU) - d
            Dim result As Integer = 0
            Dim result0 As Integer = 0
            Dim length As Integer = Me.Points.Length

            For i As Integer = 0 To length - 1
                Dim OP As Point3D = Math3D.Transformation.ORIGIN - Me.Points(i)
                Dim pPosition As Double = n.dotProduct(OP) - d

                ' careful with rounding approximations result += 1
                If observerPosition * pPosition >= 0.000000001 Then
                    If observerPosition * pPosition >= -0.000000001 AndAlso observerPosition * pPosition < 0.000000001 Then
                        result0 += 1
                    End If
                End If
            Next

            If result = 0 Then
                Return 0
            Else
                Return ((result + result0) \ length)
            End If
        End Function
    End Class
End Namespace