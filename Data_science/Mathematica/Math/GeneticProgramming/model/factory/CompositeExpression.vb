Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.impl
Imports std = System.Math

Namespace model.factory

    Public NotInheritable Class CompositeExpression

        Public Shared ReadOnly PLUS As CompositeExpression = New CompositeExpression("PLUS", InnerEnum.PLUS, GetType(Plus))
        Public Shared ReadOnly MINUS As CompositeExpression = New CompositeExpression("MINUS", InnerEnum.MINUS, GetType(Minus))
        Public Shared ReadOnly MULTIPLY As CompositeExpression = New CompositeExpression("MULTIPLY", InnerEnum.MULTIPLY, GetType(Multiply))
        Public Shared ReadOnly DIVIDE As CompositeExpression = New CompositeExpression("DIVIDE", InnerEnum.DIVIDE, GetType(Divide))
        Public Shared ReadOnly POWER As CompositeExpression = New CompositeExpression("POWER", InnerEnum.POWER, GetType(Power))
        Public Shared ReadOnly SQUAREROOT As CompositeExpression = New CompositeExpression("SQUAREROOT", InnerEnum.SQUAREROOT, GetType(SquareRoot))
        Public Shared ReadOnly LOGARITHM As CompositeExpression = New CompositeExpression("LOGARITHM", InnerEnum.LOGARITHM, GetType(Logarithm))
        Public Shared ReadOnly EXPONENTIAL As CompositeExpression = New CompositeExpression("EXPONENTIAL", InnerEnum.EXPONENTIAL, GetType(Exponential))
        Public Shared ReadOnly SINE As CompositeExpression = New CompositeExpression("SINE", InnerEnum.SINE, GetType(Sine))
        Public Shared ReadOnly COSINE As CompositeExpression = New CompositeExpression("COSINE", InnerEnum.COSINE, GetType(Cosine))
        Public Shared ReadOnly TANGENT As CompositeExpression = New CompositeExpression("TANGENT", InnerEnum.TANGENT, GetType(Tangent))

        Private Shared ReadOnly valueList As IList(Of CompositeExpression) = New List(Of CompositeExpression)()

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

        Private Sub New(name As String, innerEnum As InnerEnum, type As Type)
            Me.type = type

            nameValue = name
            ordinalValue = std.Min(Threading.Interlocked.Increment(nextOrdinal), nextOrdinal - 1)
            innerEnumValue = innerEnum
        End Sub

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
                If Equals(enumInstance.nameValue, name) Then
                    Return enumInstance
                End If
            Next
            Throw New ArgumentException(name)
        End Function
    End Class

End Namespace
