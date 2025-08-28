#Region "Microsoft.VisualBasic::6ba68d294f99468b30e1d1f0c4d729d2, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\SiteList.vb"

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

    '   Total Lines: 108
    '    Code Lines: 84 (77.78%)
    ' Comment Lines: 5 (4.63%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (17.59%)
    '     File Size: 3.47 KB


    '     Class SiteList
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [Next], Add, Circles, Count, GetSitesBounds
    '                   Regions, SiteCoords
    ' 
    '         Sub: Dispose, ResetListIndex, SortList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class SiteList

        Private sites As List(Of Site)
        Private currentIndex As Integer

        Private sorted As Boolean

        Public Sub New()
            sites = New List(Of Site)()
            sorted = False
        End Sub

        Public Sub Dispose()
            sites.Clear()
        End Sub

        Public Function Add(site As Site) As Integer
            sorted = False
            sites.Add(site)
            Return sites.Count
        End Function

        Public Function Count() As Integer
            Return sites.Count
        End Function

        Public Function [Next]() As Site
            If Not sorted Then
                Throw New Exception("SiteList.Next(): sites have not been sorted")
            End If
            If currentIndex < sites.Count Then
                Return sites(std.Min(Threading.Interlocked.Increment(currentIndex), currentIndex - 1))
            Else
                Return Nothing
            End If
        End Function

        Public Function GetSitesBounds() As Rectf
            If Not sorted Then
                SortList()
                ResetListIndex()
            End If
            Dim xmin, xmax, ymin, ymax As Single
            If sites.Count = 0 Then
                Return Rectf.zero
            End If
            xmin = Single.MaxValue
            xmax = Single.MinValue
            For Each site In sites
                If site.x < xmin Then xmin = site.x
                If site.x > xmax Then xmax = site.x
            Next
            ' here's where we assume that the sites have been sorted on y:
            ymin = sites(0).y
            ymax = sites(sites.Count - 1).y

            Return New Rectf(xmin, ymin, xmax - xmin, ymax - ymin)
        End Function

        Public Function SiteCoords() As List(Of Vector2D)
            Dim coords As List(Of Vector2D) = New List(Of Vector2D)()
            For Each site In sites
                coords.Add(site.Coord)
            Next

            Return coords
        End Function

        ' 
        ' * 
        ' * @return the largest circle centered at each site that fits in its region;
        ' * if the region is infinite, return a circle of radius 0.

        Public Function Circles() As List(Of Circle)
            Dim lCircles As List(Of Circle) = New List(Of Circle)()
            For Each site In sites
                Dim radius As Single = 0
                Dim nearestEdge As Edge = site.NearestEdge()

                If Not nearestEdge.IsPartOfConvexHull() Then radius = nearestEdge.SitesDistance() * 0.5F
                lCircles.Add(New Circle(site.x, site.y, radius))
            Next
            Return lCircles
        End Function

        Public Function Regions(plotBounds As Rectf) As List(Of List(Of Vector2D))
            Dim lRegions As List(Of List(Of Vector2D)) = New List(Of List(Of Vector2D))()
            For Each site In sites
                lRegions.Add(site.Region(plotBounds))
            Next
            Return lRegions
        End Function

        Public Sub ResetListIndex()
            currentIndex = 0
        End Sub

        Public Sub SortList()
            Site.SortSites(sites)
            sorted = True
        End Sub
    End Class
End Namespace
