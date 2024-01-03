Imports std = System.Math

Namespace evolution.measure
    Public NotInheritable Class ObjectiveFunction

        Public Shared ReadOnly MAE As ObjectiveFunction = New ObjectiveFunction("MAE", InnerEnum.MAE, New MeanAbsoluteError())
        Public Shared ReadOnly MSE As ObjectiveFunction = New ObjectiveFunction("MSE", InnerEnum.MSE, New MeanSquareError())
        Public Shared ReadOnly SAE As ObjectiveFunction = New ObjectiveFunction("SAE", InnerEnum.SAE, New SumAbsoluteError())
        Public Shared ReadOnly SSE As ObjectiveFunction = New ObjectiveFunction("SSE", InnerEnum.SSE, New SumSquareError())

        Private Shared ReadOnly valueList As IList(Of ObjectiveFunction) = New List(Of ObjectiveFunction)()

        Shared Sub New()
            valueList.Add(MAE)
            valueList.Add(MSE)
            valueList.Add(SAE)
            valueList.Add(SSE)
        End Sub

        Public Enum InnerEnum
            MAE
            MSE
            SAE
            SSE
        End Enum

        Public ReadOnly innerEnumValue As InnerEnum
        Private ReadOnly nameValue As String
        Private ReadOnly ordinalValue As Integer
        Private Shared nextOrdinal As Integer = 0

        Public ReadOnly objective As Objective

        Private Sub New(name As String, innerEnum As InnerEnum, objective As Objective)
            Me.objective = objective

            nameValue = name
            ordinalValue = std.Min(Threading.Interlocked.Increment(nextOrdinal), nextOrdinal - 1)
            innerEnumValue = innerEnum
        End Sub


        Public Shared Function values() As IList(Of ObjectiveFunction)
            Return valueList
        End Function

        Public Function ordinal() As Integer
            Return ordinalValue
        End Function

        Public Overrides Function ToString() As String
            Return nameValue
        End Function

        Public Shared Function valueOf(name As String) As ObjectiveFunction
            For Each enumInstance In valueList
                If Equals(enumInstance.nameValue, name) Then
                    Return enumInstance
                End If
            Next
            Throw New ArgumentException(name)
        End Function
    End Class

End Namespace
