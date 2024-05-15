#Region "Microsoft.VisualBasic::10e6602431168e7a3225952f595a7881, Data_science\Mathematica\Math\GeneticProgramming\model\impl\AbstractBinaryExpression.vb"

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

    '   Total Lines: 76
    '    Code Lines: 57
    ' Comment Lines: 0
    '   Blank Lines: 19
    '     File Size: 2.74 KB


    '     Class AbstractBinaryExpression
    ' 
    '         Properties: Depth, LeftChild, RightChild, Terminal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: duplicate, swapLeftChild, swapRightChild
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports std = System.Math

Namespace model.impl

    Public MustInherit Class AbstractBinaryExpression : Inherits AbstractExpression
        Implements BinaryExpression

        Public MustOverride Overrides Function toStringExpression() As String Implements Expression.toStringExpression
        Public MustOverride Overrides Function eval(x As Double) As Double Implements Expression.eval

        Private ReadOnly constructor As ConstructorInfo

        Protected Friend leftChildField As Expression
        Protected Friend rightChildField As Expression

        Public Sub New(leftChild As Expression, rightChild As Expression)
            leftChildField = leftChild
            rightChildField = rightChild
            constructor = [GetType]().GetConstructorInfo(GetType(Expression), GetType(Expression))
        End Sub

        Public Overrides Function duplicate() As Expression

            Return CType(constructor.Invoke(New Object() {leftChildField.duplicate(), rightChildField.duplicate()}), BinaryExpression)

        End Function

        Public Overridable Property LeftChild As Expression Implements BinaryExpression.LeftChild
            Get
                Return leftChildField
            End Get
            Set(value As Expression)
                leftChildField = value
            End Set
        End Property

        Public Overridable Property RightChild As Expression Implements BinaryExpression.RightChild
            Get
                Return rightChildField
            End Get
            Set(value As Expression)
                rightChildField = value
            End Set
        End Property



        Public Overridable Function swapLeftChild(newLeftChild As Expression) As Expression Implements BinaryExpression.swapLeftChild
            Dim old = leftChildField
            leftChildField = newLeftChild
            Return old
        End Function

        Public Overridable Function swapRightChild(newRightChild As Expression) As Expression Implements BinaryExpression.swapRightChild
            Dim old = rightChildField
            rightChildField = newRightChild
            Return old
        End Function

        Public Overrides ReadOnly Property Terminal As Boolean Implements Expression.Terminal
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Depth As Integer Implements Expression.Depth
            Get
                Return 1 + std.Max(leftChildField.Depth, rightChildField.Depth)
            End Get
        End Property

    End Class

End Namespace
