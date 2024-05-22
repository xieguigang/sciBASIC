#Region "Microsoft.VisualBasic::76494f390c5141754f0a41a14e6f172b, Data_science\Mathematica\Math\GeneticProgramming\model\ExpressionWrapper.vb"

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

    '   Total Lines: 93
    '    Code Lines: 74 (79.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 19 (20.43%)
    '     File Size: 3.55 KB


    '     Class ExpressionWrapper
    ' 
    '         Properties: Binary, Child, Depth, Expression, LeftChild
    '                     RightChild, Terminal, Unary
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: duplicate, eval, swapChild, swapLeftChild, swapRightChild
    '                   ToString, toStringExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace model

    Public Class ExpressionWrapper
        Implements Expression, UnaryExpression, BinaryExpression

        Public Overridable Property Expression As Expression

        Public Overridable ReadOnly Property Unary As Boolean
            Get
                Return TypeOf _Expression Is UnaryExpression
            End Get
        End Property

        Public Overridable ReadOnly Property Binary As Boolean
            Get
                Return TypeOf _Expression Is BinaryExpression
            End Get
        End Property

        Public Overridable Property LeftChild As Expression Implements BinaryExpression.LeftChild
            Get
                Return DirectCast(_Expression, BinaryExpression).LeftChild
            End Get
            Set(value As Expression)
                DirectCast(_Expression, BinaryExpression).LeftChild = value
            End Set
        End Property

        Public Overridable Property RightChild As Expression Implements BinaryExpression.RightChild
            Get
                Return DirectCast(_Expression, BinaryExpression).RightChild
            End Get
            Set(value As Expression)
                DirectCast(_Expression, BinaryExpression).RightChild = value
            End Set
        End Property

        Public Overridable Property Child As Expression Implements UnaryExpression.Child
            Get
                Return CType(Expression, UnaryExpression).Child
            End Get
            Set(value As Expression)
                CType(Expression, UnaryExpression).Child = value
            End Set
        End Property

        Public Overridable ReadOnly Property Terminal As Boolean Implements Expression.Terminal
            Get
                Return Expression.Terminal
            End Get
        End Property

        Public Overridable ReadOnly Property Depth As Integer Implements Expression.Depth
            Get
                Return Expression.Depth
            End Get
        End Property

        Public Sub New(expression As Expression)
            _Expression = expression
        End Sub

        Public Overridable Function duplicate() As Expression Implements Expression.duplicate
            Return New ExpressionWrapper(_Expression.duplicate())
        End Function

        Public Overridable Function swapLeftChild(newLeftChild As Expression) As Expression Implements BinaryExpression.swapLeftChild
            Return CType(Expression, BinaryExpression).swapLeftChild(newLeftChild)
        End Function

        Public Overridable Function swapRightChild(newRightChild As Expression) As Expression Implements BinaryExpression.swapRightChild
            Return CType(Expression, BinaryExpression).swapRightChild(newRightChild)
        End Function

        Public Overridable Function swapChild(newChild As Expression) As Expression Implements UnaryExpression.swapChild
            Return CType(Expression, UnaryExpression).swapChild(newChild)
        End Function

        Public Overridable Function eval(x As Double) As Double Implements Expression.eval
            Return Expression.eval(x)
        End Function

        Public Overridable Function toStringExpression() As String Implements Expression.toStringExpression
            Return Expression.toStringExpression()
        End Function

        Public Overrides Function ToString() As String
            Return Expression.ToString()
        End Function

    End Class

End Namespace
