#Region "Microsoft.VisualBasic::9357288f5948680e42a848e3191edbad, Data_science\Mathematica\Math\GeneticProgramming\model\factory\CompositeExpression.vb"

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

    '   Total Lines: 99
    '    Code Lines: 82 (82.83%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (17.17%)
    '     File Size: 3.97 KB


    '     Class CompositeExpression
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Enum InnerEnum
    ' 
    '             COSINE, DIVIDE, EXPONENTIAL, LOGARITHM, MINUS
    '             MULTIPLY, PLUS, POWER, SINE, SQUAREROOT
    '             TANGENT
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: BinaryTypes, UnaryTypes
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ordinal, ToString, valueOf, values
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.impl
Imports std = System.Math

Namespace model.factory

    Public NotInheritable Class CompositeExpression

        Public Shared ReadOnly PLUS As New CompositeExpression("PLUS", InnerEnum.PLUS, GetType(Plus))
        Public Shared ReadOnly MINUS As New CompositeExpression("MINUS", InnerEnum.MINUS, GetType(Minus))
        Public Shared ReadOnly MULTIPLY As New CompositeExpression("MULTIPLY", InnerEnum.MULTIPLY, GetType(Multiply))
        Public Shared ReadOnly DIVIDE As New CompositeExpression("DIVIDE", InnerEnum.DIVIDE, GetType(Divide))
        Public Shared ReadOnly POWER As New CompositeExpression("POWER", InnerEnum.POWER, GetType(Power))
        Public Shared ReadOnly SQUAREROOT As New CompositeExpression("SQUAREROOT", InnerEnum.SQUAREROOT, GetType(SquareRoot))
        Public Shared ReadOnly LOGARITHM As New CompositeExpression("LOGARITHM", InnerEnum.LOGARITHM, GetType(Logarithm))
        Public Shared ReadOnly EXPONENTIAL As New CompositeExpression("EXPONENTIAL", InnerEnum.EXPONENTIAL, GetType(Exponential))
        Public Shared ReadOnly SINE As New CompositeExpression("SINE", InnerEnum.SINE, GetType(Sine))
        Public Shared ReadOnly COSINE As New CompositeExpression("COSINE", InnerEnum.COSINE, GetType(Cosine))
        Public Shared ReadOnly TANGENT As New CompositeExpression("TANGENT", InnerEnum.TANGENT, GetType(Tangent))

        Private Shared ReadOnly valueList As New List(Of CompositeExpression)()

        Shared Sub New()
            valueList.Add(PLUS)
            valueList.Add(MINUS)
            valueList.Add(MULTIPLY)
            valueList.Add(DIVIDE)
            valueList.Add(POWER)
            valueList.Add(SQUAREROOT)
            valueList.Add(LOGARITHM)
            valueList.Add(EXPONENTIAL)
            valueList.Add(SINE)
            valueList.Add(COSINE)
            valueList.Add(TANGENT)
        End Sub

        Public Enum InnerEnum
            PLUS
            MINUS
            MULTIPLY
            DIVIDE
            POWER
            SQUAREROOT
            LOGARITHM
            EXPONENTIAL
            SINE
            COSINE
            TANGENT
        End Enum

        Public ReadOnly innerEnumValue As InnerEnum
        Private ReadOnly nameValue As String
        Private ReadOnly ordinalValue As Integer
        Private Shared nextOrdinal As Integer = 0

        Public ReadOnly type As Type

        Public Shared ReadOnly Property UnaryTypes As CompositeExpression()
            Get
                Return New CompositeExpression() {SQUAREROOT, LOGARITHM, EXPONENTIAL, SINE, COSINE, TANGENT}
            End Get
        End Property

        Public Shared ReadOnly Property BinaryTypes As CompositeExpression()
            Get
                Return New CompositeExpression() {PLUS, MINUS, MULTIPLY, DIVIDE, POWER}
            End Get
        End Property

        Private Sub New(name As String, innerEnum As InnerEnum, type As Type)
            Me.type = type

            nameValue = name
            ordinalValue = std.Min(Threading.Interlocked.Increment(nextOrdinal), nextOrdinal - 1)
            innerEnumValue = innerEnum
        End Sub

        Public Shared Function values() As IList(Of CompositeExpression)
            Return valueList
        End Function

        Public Function ordinal() As Integer
            Return ordinalValue
        End Function

        Public Overrides Function ToString() As String
            Return nameValue
        End Function

        Public Shared Function valueOf(name As String) As CompositeExpression
            For Each enumInstance In valueList
                If enumInstance.nameValue = name Then
                    Return enumInstance
                End If
            Next
            Throw New ArgumentException(name)
        End Function
    End Class

End Namespace
