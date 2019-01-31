#Region "Microsoft.VisualBasic::ffb4938fd261d8e92f530f459a3327cd, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Models\DirectedEdge.vb"

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
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports number = System.Double

Namespace Layouts.Cola

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

    Public Class Constraint
        Public lm As Double
        Public active As Boolean = False
        Public unsatisfiable As Boolean = False

        Public left As Variable
        Public right As Variable
        Public gap As Double
        Public equality As Boolean = False

        Sub New(left As Variable, right As Variable, gap As Double, Optional equality As Boolean = False)
            Me.left = left
            Me.right = right
            Me.gap = gap
            Me.equality = equality
        End Sub

        Public ReadOnly Property slack As Double
            Get
                If unsatisfiable Then
                    Return number.MaxValue
                Else
                    Return right.scale * right.position - gap - left.scale * left.position
                End If
            End Get
        End Property
    End Class
End Namespace
