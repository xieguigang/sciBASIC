Namespace Drawing3D.IsoMetric

    ''' <summary>
    ''' A collection of the path3D data.
    ''' </summary>
    Public Class Shape3D

        Protected Friend paths As List(Of Path3D)

        Public Sub New()
            paths = New List(Of Path3D)
        End Sub

        Public Sub New(paths As Path3D())
            Me.paths = New List(Of Path3D)(paths)
        End Sub

        Public Overridable Sub push(___path As Path3D)
            Call paths.Add(___path)
        End Sub

        Public Overridable Sub push(paths As IEnumerable(Of Path3D))
            Call Me.paths.AddRange(paths)
        End Sub

        Public Overridable Function Translate(dx As Double, dy As Double, dz As Double) As Shape3D
            Dim ___paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim ___point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                ___point = Me.paths(i)
                ___paths(i) = ___point.Translate(dx, dy, dz)
            Next i
            Return New Shape3D(___paths)
        End Function

        Public Overridable Function rotateX(origin As Point3D, angle As Double) As Shape3D
            Dim ___paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim ___point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                ___point = Me.paths(i)
                ___paths(i) = ___point.RotateX(origin, angle)
            Next i
            Return New Shape3D(___paths)
        End Function

        Public Overridable Function rotateY(origin As Point3D, angle As Double) As Shape3D
            Dim ___paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim ___point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                ___point = Me.paths(i)
                ___paths(i) = ___point.RotateY(origin, angle)
            Next i
            Return New Shape3D(___paths)
        End Function

        Public Overridable Function rotateZ(origin As Point3D, angle As Double) As Shape3D
            Dim ___paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim ___point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                ___point = Me.paths(i)
                ___paths(i) = ___point.RotateZ(origin, angle)
            Next i
            Return New Shape3D(___paths)
        End Function

        Public Overridable Function scale(origin As Point3D, dx As Double, dy As Double, dz As Double) As Shape3D
            Dim ___paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim ___point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                ___point = Me.paths(i)
                ___paths(i) = ___point.Scale(origin, dx, dy, dz)
            Next i
            Return New Shape3D(___paths)
        End Function

        Public Overridable Function scale(origin As Point3D, dx As Double, dy As Double) As Shape3D
            Dim ___paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim ___point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                ___point = Me.paths(i)
                ___paths(i) = ___point.Scale(origin, dx, dy)
            Next i
            Return New Shape3D(___paths)
        End Function

        Public Overridable Function scale(origin As Point3D, dx As Double) As Shape3D
            Dim ___paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim ___point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                ___point = Me.paths(i)
                ___paths(i) = ___point.Scale(origin, dx)
            Next i
            Return New Shape3D(___paths)
        End Function

        Public Overridable Sub scalePath3Ds(origin As Point3D, dx As Double, dy As Double, dz As Double)
            Dim i As Integer = 0
            Dim length As Integer = paths.Count
            Do While i < length
                paths(i) = paths(i).Scale(origin, dx, dy, dz)
                i += 1
            Loop
        End Sub

        Public Overridable Sub scalePath3Ds(origin As Point3D, dx As Double, dy As Double)
            Dim i As Integer = 0
            Dim length As Integer = paths.Count
            Do While i < length
                paths(i) = paths(i).Scale(origin, dx, dy)
                i += 1
            Loop
        End Sub

        Public Overridable Sub scalePath3Ds(origin As Point3D, dx As Double)
            Dim i As Integer = 0
            Dim length As Integer = paths.Count
            Do While i < length
                paths(i) = paths(i).Scale(origin, dx)
                i += 1
            Loop
        End Sub

        Public Overridable Sub translatePath3Ds(dx As Double, dy As Double, dz As Double)
            Dim i As Integer = 0
            Dim length As Integer = paths.Count
            Do While i < length
                paths(i) = paths(i).Translate(dx, dy, dz)
                i += 1
            Loop
        End Sub

        ''' <summary>
        ''' Sort the list of faces by distance then map the entries, returning
        ''' only the path and not the added "further point" from earlier.
        ''' </summary>
        Public Overridable Function orderedPath3Ds() As Path3D()
            Dim depths As Double() = New Double(paths.Count - 1) {}
            For i As Integer = 0 To depths.Length - 1
                depths(i) = paths(i).Depth()
            Next i
            Dim swapped As Boolean = True
            Dim j As Integer = 0
            Dim tmp As Path3D
            Dim tmp2 As Double
            Do While swapped
                swapped = False
                j += 1
                For i As Integer = 0 To paths.Count - j - 1
                    If depths(i) < depths(i + 1) Then
                        tmp = paths(i)
                        tmp2 = depths(i)
                        paths(i) = paths(i + 1)
                        depths(i) = depths(i + 1)
                        paths(i + 1) = tmp
                        depths(i + 1) = tmp2
                        swapped = True
                    End If
                Next i
            Loop
            Return Me.paths.ToArray
        End Function

        Public Shared Function extrude(___path As Path3D) As Shape3D
            Return extrude(New Shape3D, ___path, 1)
        End Function

        Public Shared Function extrude(___path As Path3D, height As Double) As Shape3D
            Return extrude(New Shape3D, ___path, height)
        End Function

        Public Shared Function extrude(___shape As Shape3D, ___path As Path3D) As Shape3D
            Return extrude(___shape, ___path, 1)
        End Function

        Public Shared Function extrude(___shape As Shape3D, ___path As Path3D, height As Double) As Shape3D
            Dim topPath3D As Path3D = ___path.Translate(0, 0, height)
            Dim i As Integer
            Dim length As Integer = ___path.Points.Count

            Dim ___paths As Path3D() = New Path3D(length + 2 - 1) {}

            ' Push the top and bottom faces, top face must be oriented correctly 
            ___paths(0) = ___path.Reverse()
            ___paths(1) = topPath3D

            ' Push each side face 
            Dim points As Point3D()
            For i = 0 To length - 1
                points = New Point3D(3) {}
                points(0) = topPath3D.Points(i)
                points(1) = ___path.Points(i)
                points(2) = ___path.Points((i + 1) Mod length)
                points(3) = topPath3D.Points((i + 1) Mod length)
                ___paths(i + 2) = New Path3D(points)
            Next i
            ___shape.paths = ___paths.AsList
            Return ___shape
        End Function
    End Class
End Namespace