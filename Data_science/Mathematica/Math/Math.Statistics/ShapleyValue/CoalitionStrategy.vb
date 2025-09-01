Namespace ShapleyValue

    ''' 
    ''' <summary>
    ''' @author Franck Benault
    ''' 
    ''' @version	0.0.2
    ''' @since 0.0.2
    ''' 
    ''' </summary>
    Public NotInheritable Class CoalitionStrategy

        Public Shared ReadOnly SEQUENTIALField As CoalitionStrategy = New CoalitionStrategy("SEQUENTIAL", InnerEnum.SEQUENTIAL)

        Public Shared ReadOnly RANDOM As CoalitionStrategy = New CoalitionStrategy("RANDOM", InnerEnum.RANDOM)

        Private Shared ReadOnly valueList As List(Of CoalitionStrategy) = New List(Of CoalitionStrategy)()

        Shared Sub New()
            valueList.Add(SEQUENTIALField)
            valueList.Add(RANDOM)
        End Sub

        Public Enum InnerEnum
            SEQUENTIAL
            RANDOM
        End Enum

        Public ReadOnly innerEnumValue As InnerEnum
        Private ReadOnly nameValue As String
        Private ReadOnly ordinalValue As Integer
        Private Shared nextOrdinal As Integer = 0

        Private Sub New(name As String, innerEnum As InnerEnum)
            nameValue = name
            ordinalValue = Math.Min(Threading.Interlocked.Increment(nextOrdinal), nextOrdinal - 1)
            innerEnumValue = innerEnum
        End Sub

        Public ReadOnly Property Sequential As Boolean
            Get
                Return Equals(SEQUENTIALField)
            End Get
        End Property

        Public Shared Function values() As CoalitionStrategy()
            Return valueList.ToArray()
        End Function

        Public Function ordinal() As Integer
            Return ordinalValue
        End Function

        Public Overrides Function ToString() As String
            Return nameValue
        End Function

        Public Shared Function valueOf(name As String) As CoalitionStrategy
            For Each enumInstance In valueList
                If Equals(enumInstance.nameValue, name) Then
                    Return enumInstance
                End If
            Next
            Throw New ArgumentException(name)
        End Function
    End Class

End Namespace
