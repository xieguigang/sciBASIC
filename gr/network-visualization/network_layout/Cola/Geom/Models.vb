#Region "Microsoft.VisualBasic::2c4a1953649c1f7bc0e9761c50f8eafb, gr\network-visualization\network_layout\Cola\Geom\Models.vb"

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

    '   Total Lines: 89
    '    Code Lines: 58 (65.17%)
    ' Comment Lines: 8 (8.99%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (25.84%)
    '     File Size: 2.26 KB


    '     Class PolyPoint
    ' 
    '         Properties: polyIndex
    ' 
    '     Class tangentPoly
    ' 
    '         Properties: ltan, rtan
    ' 
    '     Class BiTangent
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class BiTangents
    ' 
    ' 
    ' 
    '     Class TVGPoint
    ' 
    ' 
    ' 
    '     Class VisibilityVertex
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class VisibilityEdge
    ' 
    '         Properties: length
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.My.JavaScript
Imports stdNum = System.Math

Namespace Cola

    Public Class PolyPoint : Inherits Point2D

        Public Property polyIndex() As Integer

    End Class

    Public Class tangentPoly

        ''' <summary>
        ''' index of rightmost tangent point V[rtan]
        ''' </summary>
        ''' <returns></returns>
        Public Property rtan() As Integer
        ''' <summary>
        ''' index of leftmost tangent point V[ltan]
        ''' </summary>
        ''' <returns></returns>
        Public Property ltan() As Integer

    End Class

    Public Class BiTangent

        Public t1 As Integer, t2 As Integer

        Public Sub New()
        End Sub

        Public Sub New(t1 As Integer, t2 As Integer)
            Me.t1 = t1
            Me.t2 = t2
        End Sub
    End Class

    Public Class BiTangents : Inherits JavaScriptObject

        Public rl As BiTangent
        Public lr As BiTangent
        Public ll As BiTangent
        Public rr As BiTangent

    End Class

    Public Class TVGPoint : Inherits Point2D
        Public vv As VisibilityVertex
    End Class

    Public Class VisibilityVertex

        Public id As Integer
        Public polyid As Double
        Public polyvertid As Double
        Public p As TVGPoint

        Public Sub New(id As Integer, polyid As Double, polyvertid As Double, p As TVGPoint)
            Me.id = id
            Me.polyid = polyid
            Me.polyvertid = polyvertid
            Me.p = p

            p.vv = Me
        End Sub
    End Class

    Public Class VisibilityEdge

        Public source As VisibilityVertex
        Public target As VisibilityVertex

        Public ReadOnly Property length() As Double
            Get
                Dim dx = Me.source.p.X - Me.target.p.X
                Dim dy = Me.source.p.Y - Me.target.p.Y
                Return stdNum.Sqrt(dx * dx + dy * dy)
            End Get
        End Property

        Sub New(source As VisibilityVertex, target As VisibilityVertex)
            Me.source = source
            Me.target = target
        End Sub
    End Class
End Namespace
