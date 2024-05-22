#Region "Microsoft.VisualBasic::85d7b453fbabb9e5747f0f40e397400b, gr\network-visualization\network_layout\Cola\Models\Variable.vb"

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

    '   Total Lines: 51
    '    Code Lines: 42 (82.35%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (17.65%)
    '     File Size: 1.67 KB


    '     Class Variable
    ' 
    '         Properties: dfdv, position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: visitNeighbours
    ' 
    '         Operators: (+2 Overloads) IsFalse, (+2 Overloads) IsTrue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Cola

    Public Class Variable

        Public index As Integer
        Public offset As Double = 0
        Public block As Block
        Public cIn As Constraint()
        Public cOut As Constraint()
        Public weight As Double = 1
        Public scale As Double = 1
        Public desiredPosition As Double

        Public ReadOnly Property dfdv As Double
            Get
                Return 2 * weight * (position - desiredPosition)
            End Get
        End Property

        Public ReadOnly Property position As Double
            Get
                Return (block.ps.scale * block.posn + offset) / scale
            End Get
        End Property

        Sub New(desiredPosition As Double, Optional weight As Double = 1, Optional scale As Double = 1)
            Me.desiredPosition = desiredPosition
            Me.weight = weight
            Me.scale = scale
        End Sub

        Public Sub visitNeighbours(prev As Variable, f As Action(Of Constraint, Variable))
            Dim ff = Sub(c As Constraint, [next] As Variable)
                         If c.active AndAlso Not prev Is [next] Then
                             Call f(c, [next])
                         End If
                     End Sub

            cOut.ForEach(Sub(c, i) ff(c, c.right))
            cIn.ForEach(Sub(c, i) ff(c, c.left))
        End Sub

        Public Shared Operator IsTrue(v As Variable) As Boolean
            Return Not v Is Nothing
        End Operator

        Public Shared Operator IsFalse(v As Variable) As Boolean
            Return v Is Nothing
        End Operator
    End Class
End Namespace
