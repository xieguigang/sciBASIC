#Region "Microsoft.VisualBasic::f7ff2dbfad77ec1018ec20f24d0ffd10, gr\network-visualization\network_layout\Cola\Models\DirectedEdge.vb"

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

    '   Total Lines: 106
    '    Code Lines: 80 (75.47%)
    ' Comment Lines: 6 (5.66%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 20 (18.87%)
    '     File Size: 3.33 KB


    '     Structure DirectedEdge
    ' 
    ' 
    ' 
    '     Class PositionStats
    ' 
    '         Properties: Posn
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: addVariable
    ' 
    '     Class Constraint
    ' 
    '         Properties: slack
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class ConstraintOffset
    ' 
    ' 
    ' 
    '     Class Constraint
    ' 
    '         Properties: slack
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports number = System.Double

Namespace Cola

    ''' <summary>
    ''' An object with three point properties, the intersection with the
    ''' source rectangle (sourceIntersection), the intersection with then
    ''' target rectangle (targetIntersection), And the point an arrow
    ''' head of the specified size would need to start (arrowStart).
    ''' </summary>
    Public Structure DirectedEdge

        Public sourceIntersection As Point2D
        Public targetIntersection As Point2D
        Public arrowStart As Point2D

    End Structure

    Public Class PositionStats
        Public scale As Double

        Public AB As Double = 0
        Public AD As Double = 0
        Public A2 As Double = 0

        Public ReadOnly Property Posn As Double
            Get
                Return (AD - AB) / A2
            End Get
        End Property

        Sub New(scale As Double)
            Me.scale = scale
        End Sub

        Public Sub addVariable(v As Variable)
            Dim ai = scale / v.scale
            Dim bi = v.offset / v.scale
            Dim wi = v.weight

            AB += wi * ai * bi
            AD += wi * ai * v.desiredPosition
            A2 += wi * ai * ai
        End Sub
    End Class

    Public Class Constraint : Inherits Constraint(Of Variable)

        Public Overrides ReadOnly Property slack As Double
            Get
                If unsatisfiable Then
                    Return number.MaxValue
                Else
                    Return right.scale * right.position - gap - left.scale * left.position
                End If
            End Get
        End Property

        Sub New(left As Variable, right As Variable, gap As Double, Optional equality As Boolean = False)
            Call MyBase.New(left, right, gap, equality)
        End Sub
    End Class

    Public Class ConstraintOffset
        Public node As Integer
        Public offset As Double
    End Class

    Public Class Constraint(Of T)

        Public lm As Double
        Public active As Boolean = False
        Public unsatisfiable As Boolean = False

        Public left As T
        Public right As T
        Public gap As Double
        Public equality As Boolean? = False
        Public axis As String
        Public type As String
        Public offsets As ConstraintOffset()

        Sub New(left As T, right As T, gap As Double, Optional equality As Boolean = False)
            Me.left = left
            Me.right = right
            Me.gap = gap
            Me.equality = equality
        End Sub

        Public Overridable ReadOnly Property slack As Double
            Get
                If unsatisfiable OrElse GetType(T) Is GetType(Integer) Then
                    Return number.MaxValue
                Else
                    Dim left = DirectCast(CObj(Me.left), Variable)
                    Dim right = DirectCast(CObj(Me.right), Variable)

                    Return right.scale * right.position - gap - left.scale * left.position
                End If
            End Get
        End Property
    End Class
End Namespace
