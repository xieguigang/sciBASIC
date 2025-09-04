#Region "Microsoft.VisualBasic::de830a1ddf5b692d346745c7172087ba, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Geom\Circle.vb"

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

    '   Total Lines: 18
    '    Code Lines: 14 (77.78%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (22.22%)
    '     File Size: 579 B


    '     Class Circle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Drawing2D.Math2D.DelaunayVoronoi
    Public Class Circle

        Public center As Vector2D
        Public radius As Single

        Public Sub New(centerX As Single, centerY As Single, radius As Single)
            center = New Vector2D(centerX, centerY)
            Me.radius = radius
        End Sub

        Public Overrides Function ToString() As String
            Return "Circle (center: " & center.ToString() & "; radius: " & radius.ToString() & ")"
        End Function
    End Class
End Namespace
