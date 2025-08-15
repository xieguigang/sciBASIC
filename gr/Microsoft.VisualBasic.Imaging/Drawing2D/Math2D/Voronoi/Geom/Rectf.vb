#Region "Microsoft.VisualBasic::f9cc25890f0d697e1b1bf04179f7e859, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Voronoi\Geom\Rectf.vb"

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

    '   Total Lines: 54
    '    Code Lines: 44 (81.48%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (18.52%)
    '     File Size: 1.45 KB


    '     Structure Rectf
    ' 
    '         Properties: bottom, bottomRight, left, right, top
    '                     topLeft
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Drawing2D.Math2D.DelaunayVoronoi
    Public Structure Rectf

        Public Shared ReadOnly zero As Rectf = New Rectf(0, 0, 0, 0)
        Public Shared ReadOnly one As Rectf = New Rectf(1, 1, 1, 1)

        Public x, y, width, height As Single

        Public Sub New(x As Single, y As Single, width As Single, height As Single)
            Me.x = x
            Me.y = y
            Me.width = width
            Me.height = height
        End Sub

        Public ReadOnly Property left As Single
            Get
                Return x
            End Get
        End Property

        Public ReadOnly Property right As Single
            Get
                Return x + width
            End Get
        End Property

        Public ReadOnly Property top As Single
            Get
                Return y
            End Get
        End Property

        Public ReadOnly Property bottom As Single
            Get
                Return y + height
            End Get
        End Property

        Public ReadOnly Property topLeft As Vector2D
            Get
                Return New Vector2D(left, top)
            End Get
        End Property

        Public ReadOnly Property bottomRight As Vector2D
            Get
                Return New Vector2D(right, bottom)
            End Get
        End Property
    End Structure
End Namespace
