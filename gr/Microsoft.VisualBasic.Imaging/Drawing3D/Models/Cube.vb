#Region "Microsoft.VisualBasic::914ec7070d284f6e36d529e524c5b79d, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Cube.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language

Namespace Drawing3D

    Public Structure Cube : Implements I3DModel

        ''' <summary>
        ''' Create an array representing the 6 faces of a cube. Each face is composed by indices to the vertex array
        ''' above.
        ''' </summary>
        Shared ReadOnly _faces%()() = {
            {0, 1, 2, 3},
            {1, 5, 6, 2},
            {5, 4, 7, 6},
            {4, 0, 3, 7},
            {0, 4, 5, 1},
            {3, 2, 6, 7}
        }.ToVectorList

        Dim faces As Surface()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="d%">正方形的边的长度</param>
        ''' <param name="colors"></param>
        Sub New(Optional d% = 1, Optional colors As Color() = Nothing)
            Me.New(
                vertices:={  ' Create the cube vertices.
                    New Point3D(-1 * d, 1 * d, -1 * d),
                    New Point3D(1 * d, 1 * d, -1 * d),
                    New Point3D(1 * d, -1 * d, -1 * d),
                    New Point3D(-1 * d, -1 * d, -1 * d),
                    New Point3D(-1 * d, 1 * d, 1 * d),
                    New Point3D(1 * d, 1 * d, 1 * d),
                    New Point3D(1 * d, -1 * d, 1 * d),
                    New Point3D(-1 * d, -1 * d, 1 * d)
                },
                colors:=If(
                colors.IsNullOrEmpty,
                {   ' Define the colors of each face.
                    Color.BlueViolet,
                    Color.Cyan,
                    Color.Green,
                    Color.Yellow,
                    Color.Violet,
                    Color.LightSkyBlue
                }, colors))
        End Sub

        Sub New(vertices As Point3D(), colors As Color())
            Me.New(
                vertices,
                colors.ToArray(Function(c) New SolidBrush(c)))
        End Sub

        Sub New(vertices As Point3D(), brushes As Brush())
            ' Create the brushes to draw each face. Brushes are used to draw filled polygons.
            faces = New Surface(5) {}

            For i% = 0 To 5
                Dim side%() = _faces(i)

                faces(i) = New Surface With {
                    .vertices = {
                        vertices(side(0)),
                        vertices(side(1)),
                        vertices(side(2)),
                        vertices(side(3))
                    },
                    .brush = brushes(i)
                }
                faces(i).Allocation()
            Next
        End Sub

        Public Sub Draw(ByRef canvas As Graphics, camera As Camera) Implements I3DModel.Draw
            Dim avgZ(6) As Single
            Dim order(6) As Integer
            Dim tmp As Single
            Dim iMax As Integer

            ' Compute the average Z value of each face.
            For i% = 0 To 5
                Dim face As Surface = faces(i)
                avgZ(i) = face.Average(Function(pt) pt.Z)
                order(i) = i
            Next

            ' Next we sort the faces in descending order based on the Z value.
            ' The objective is to draw distant faces first. This is called
            ' the PAINTERS ALGORITHM. So, the visible faces will hide the invisible ones.
            ' The sorting algorithm used is the SELECTION SORT.
            For i = 0 To 4
                iMax = i
                For j = i + 1 To 5
                    If avgZ(j) > avgZ(iMax) Then
                        iMax = j
                    End If
                Next
                If iMax <> i Then
                    tmp = avgZ(i)
                    avgZ(i) = avgZ(iMax)
                    avgZ(iMax) = tmp

                    tmp = order(i)
                    order(i) = order(iMax)
                    order(iMax) = tmp
                End If
            Next

            ' Draw the faces using the PAINTERS ALGORITHM (distant faces first, closer faces last).
            For i = 0 To 5
                Call faces(order(i)).Draw(canvas, camera)
            Next
        End Sub

        Public Function Copy(data As IEnumerable(Of Point3D)) As I3DModel Implements I3DModel.Copy
            Return New Cube(data.ToArray, faces.ToArray(Function(s) s.brush))
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' ```
        ''' 0 - {0, 1, 2, 3} 0,1,2,3 = 0,1,2,3
        ''' 1 - {1, 5, 6, 2}   1,2   = 5,6
        ''' 2 - {5, 4, 7, 6}   1,2   = 4,7
        ''' 3 - {4, 0, 3, 7}
        ''' 4 - {0, 4, 5, 1}
        ''' 5 - {3, 2, 6, 7}
        ''' ```
        ''' </remarks>
        Public Iterator Function GetEnumerator() As IEnumerator(Of Point3D) Implements IEnumerable(Of Point3D).GetEnumerator
            Dim f As Surface

            f = faces(0)

            Yield f.vertices(0)
            Yield f.vertices(1)
            Yield f.vertices(2)
            Yield f.vertices(3)

            f = faces(2)

            Yield f.vertices(1)

            f = faces(1)

            Yield f.vertices(1)
            Yield f.vertices(2)

            f = faces(2)

            Yield f.vertices(2)
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Structure
End Namespace
