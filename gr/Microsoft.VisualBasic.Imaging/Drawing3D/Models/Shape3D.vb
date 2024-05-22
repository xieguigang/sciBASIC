#Region "Microsoft.VisualBasic::6c23d9f8ecfccb3c99ae00aea03e5a3a, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shape3D.vb"

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

    '   Total Lines: 223
    '    Code Lines: 165 (73.99%)
    ' Comment Lines: 16 (7.17%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 42 (18.83%)
    '     File Size: 7.87 KB


    '     Class Shape3D
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+4 Overloads) Extrude, OrderedPath3Ds, RotateX, RotateY, RotateZ
    '                   (+3 Overloads) Scale, Translate
    ' 
    '         Sub: (+2 Overloads) Push, (+3 Overloads) ScalePath3Ds, TranslatePath3Ds
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D.Models.Isometric

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

        Public Sub Push(path As Path3D)
            Call paths.Add(path)
        End Sub

        Public Sub Push(paths As IEnumerable(Of Path3D))
            Call Me.paths.AddRange(paths)
        End Sub

        Public Function Translate(dx As Double, dy As Double, dz As Double) As Shape3D
            Dim paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim point As Path3D

            For i As Integer = 0 To Me.paths.Count - 1
                point = Me.paths(i)
                paths(i) = point.Translate(dx, dy, dz)
            Next

            Return New Shape3D(paths)
        End Function

        Public Function RotateX(origin As Point3D, angle As Double) As Shape3D
            Dim paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim point As Path3D

            For i As Integer = 0 To Me.paths.Count - 1
                point = Me.paths(i)
                paths(i) = point.RotateX(origin, angle)
            Next

            Return New Shape3D(paths)
        End Function

        Public Function RotateY(origin As Point3D, angle As Double) As Shape3D
            Dim paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim point As Path3D

            For i As Integer = 0 To Me.paths.Count - 1
                point = Me.paths(i)
                paths(i) = point.RotateY(origin, angle)
            Next

            Return New Shape3D(paths)
        End Function

        Public Function RotateZ(origin As Point3D, angle As Double) As Shape3D
            Dim paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim point As Path3D

            For i As Integer = 0 To Me.paths.Count - 1
                point = Me.paths(i)
                paths(i) = point.RotateZ(origin, angle)
            Next

            Return New Shape3D(paths)
        End Function

        Public Function Scale(origin As Point3D, dx As Double, dy As Double, dz As Double) As Shape3D
            Dim paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim point As Path3D

            For i As Integer = 0 To Me.paths.Count - 1
                point = Me.paths(i)
                paths(i) = point.Scale(origin, dx, dy, dz)
            Next

            Return New Shape3D(paths)
        End Function

        Public Function Scale(origin As Point3D, dx As Double, dy As Double) As Shape3D
            Dim paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                point = Me.paths(i)
                paths(i) = point.Scale(origin, dx, dy)
            Next
            Return New Shape3D(paths)
        End Function

        Public Function Scale(origin As Point3D, dx As Double) As Shape3D
            Dim paths As Path3D() = New Path3D(Me.paths.Count - 1) {}
            Dim point As Path3D
            For i As Integer = 0 To Me.paths.Count - 1
                point = Me.paths(i)
                paths(i) = point.Scale(origin, dx)
            Next
            Return New Shape3D(paths)
        End Function

        Public Sub ScalePath3Ds(origin As Point3D, dx As Double, dy As Double, dz As Double)
            Dim i As Integer = 0
            Dim length As Integer = paths.Count
            Do While i < length
                paths(i) = paths(i).Scale(origin, dx, dy, dz)
                i += 1
            Loop
        End Sub

        Public Sub ScalePath3Ds(origin As Point3D, dx As Double, dy As Double)
            Dim i As Integer = 0
            Dim length As Integer = paths.Count
            Do While i < length
                paths(i) = paths(i).Scale(origin, dx, dy)
                i += 1
            Loop
        End Sub

        Public Sub ScalePath3Ds(origin As Point3D, dx As Double)
            Dim i As Integer = 0
            Dim length As Integer = paths.Count

            Do While i < length
                paths(i) = paths(i).Scale(origin, dx)
                i += 1
            Loop
        End Sub

        Public Sub TranslatePath3Ds(dx As Double, dy As Double, dz As Double)
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
        Public Function OrderedPath3Ds() As Path3D()
            Dim depths#() = New Double(paths.Count - 1) {}

            For i As Integer = 0 To depths.Length - 1
                depths(i) = paths(i).Depth()
            Next

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
                Next
            Loop

            Return Me.paths.ToArray
        End Function

        Public Shared Function Extrude(path As Path3D) As Shape3D
            Return Extrude(New Shape3D, path, 1)
        End Function

        Public Shared Function Extrude(path As Path3D, height As Double) As Shape3D
            Return Extrude(New Shape3D, path, height)
        End Function

        Public Shared Function Extrude(shape As Shape3D, path As Path3D) As Shape3D
            Return Extrude(shape, path, 1)
        End Function

        ''' <summary>
        ''' 将一个面平移一段距离，最后和通过平移新的到的平面，构成一个三维物体
        ''' </summary>
        ''' <param name="shape"></param>
        ''' <param name="path"></param>
        ''' <param name="height"></param>
        ''' <returns></returns>
        Public Shared Function Extrude(shape As Shape3D, path As Path3D, height As Double) As Shape3D
            Dim topPath3D As Path3D = path.Translate(0, 0, height)
            Dim length As Integer = path.Points.Count
            Dim paths As Path3D() = New Path3D(length + 2 - 1) {}

            ' Push the top and bottom faces, top face must be oriented correctly 
            paths(0) = path.Reverse()
            paths(1) = topPath3D

            ' Push each side face 
            Dim points As Point3D()

            For i As Integer = 0 To length - 1
                points = New Point3D(3) {}
                points(0) = topPath3D.Points(i)
                points(1) = path.Points(i)
                points(2) = path.Points((i + 1) Mod length)
                points(3) = topPath3D.Points((i + 1) Mod length)
                paths(i + 2) = New Path3D(points)
            Next

            shape.paths = paths.AsList

            Return shape
        End Function
    End Class
End Namespace
