#Region "Microsoft.VisualBasic::a9788f3e0defd184a08a212df24d66a2, Data_science\Mathematica\Math\GeneticProgramming\model\impl\AbstractUnaryExpression.vb"

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

    '   Total Lines: 55
    '    Code Lines: 42
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 1.81 KB


    '     Class AbstractUnaryExpression
    ' 
    '         Properties: Child, Depth, Terminal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: duplicate, swapChild
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace model.impl

    Public MustInherit Class AbstractUnaryExpression
        Inherits AbstractExpression
        Implements UnaryExpression
        Public MustOverride Overrides Function toStringExpression() As String Implements Expression.toStringExpression
        Public MustOverride Overrides Function eval(x As Double) As Double Implements Expression.eval

        Private ReadOnly constructor As ConstructorInfo

        Protected Friend m_child As Expression

        Public Sub New(child As Expression)
            m_child = child
            constructor = [GetType]().GetConstructorInfo(GetType(Expression))
        End Sub

        Public Overrides Function duplicate() As Expression
            Return constructor.Invoke(New Object() {m_child.duplicate()})
        End Function

        Public Overridable Property Child As Expression Implements UnaryExpression.Child
            Get
                Return m_child
            End Get
            Set(value As Expression)
                m_child = value
            End Set
        End Property


        Public Overridable Function swapChild(newChild As Expression) As Expression Implements UnaryExpression.swapChild
            Dim old = m_child
            m_child = newChild
            Return old
        End Function

        Public Overrides ReadOnly Property Terminal As Boolean Implements Expression.Terminal
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Depth As Integer Implements Expression.Depth
            Get
                Return 1 + m_child.Depth
            End Get
        End Property

    End Class

End Namespace
