#Region "Microsoft.VisualBasic::5cb7751b803d34fbe2445c30ae2859eb, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Delaunay\Triangle.vb"

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

    '   Total Lines: 15
    '    Code Lines: 11 (73.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (26.67%)
    '     File Size: 368 B


    '     Class Triangle
    ' 
    '         Properties: Sites
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class Triangle

        Public ReadOnly Property Sites As List(Of Site)

        Public Sub New(a As Site, b As Site, c As Site)
            Sites = New List(Of Site)() From {a, b, c}
        End Sub

        Public Sub Dispose()
            Sites.Clear()
        End Sub
    End Class
End Namespace
