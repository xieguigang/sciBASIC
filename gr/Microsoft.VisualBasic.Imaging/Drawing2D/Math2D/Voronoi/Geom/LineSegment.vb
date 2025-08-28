#Region "Microsoft.VisualBasic::0a758e5e25291b61c8a6db927a613f1e, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Geom\LineSegment.vb"

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

    '   Total Lines: 45
    '    Code Lines: 36 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (20.00%)
    '     File Size: 1.50 KB


    '     Class LineSegment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CompareLengths, CompareLengths_MAX, VisibleLineSegments
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class LineSegment

        Public Shared Function VisibleLineSegments(edges As List(Of Edge)) As List(Of LineSegment)
            Dim segments As List(Of LineSegment) = New List(Of LineSegment)()

            For Each edge In edges
                If edge.Visible() Then
                    Dim p1 = edge.ClippedEnds(LR.LEFT)
                    Dim p2 = edge.ClippedEnds(LR.RIGHT)
                    segments.Add(New LineSegment(p1, p2))
                End If
            Next

            Return segments
        End Function

        Public Shared Function CompareLengths_MAX(segment0 As LineSegment, segment1 As LineSegment) As Single
            Dim length0 = (segment0.p0 - segment0.p1).Length
            Dim length1 = (segment1.p0 - segment1.p1).Length
            If length0 < length1 Then
                Return 1
            End If
            If length0 > length1 Then
                Return -1
            End If
            Return 0
        End Function

        Public Shared Function CompareLengths(edge0 As LineSegment, edge1 As LineSegment) As Single
            Return -CompareLengths_MAX(edge0, edge1)
        End Function

        Public p0 As Vector2D
        Public p1 As Vector2D

        Public Sub New(p0 As Vector2D, p1 As Vector2D)
            Me.p0 = p0
            Me.p1 = p1
        End Sub
    End Class
End Namespace
