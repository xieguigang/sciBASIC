Namespace NeuralNetwork
    Public Class DataSet
#Region "-- Properties --"
        Public Property Values() As Double()
            Get
                Return m_Values
            End Get
            Set
                m_Values = Value
            End Set
        End Property
        Private m_Values As Double()
        Public Property Targets() As Double()
            Get
                Return m_Targets
            End Get
            Set
                m_Targets = Value
            End Set
        End Property
        Private m_Targets As Double()
#End Region

#Region "-- Constructor --"
        Public Sub New(values__1 As Double(), targets__2 As Double())
            Values = values__1
            Targets = targets__2
        End Sub
#End Region
    End Class
End Namespace
