Namespace Dynamics

    Public Class SolverIterator

        Dim rk4 As RungeKutta4
        Dim solverEnumerator As IEnumerator(Of Integer)
        Dim triggers As New List(Of Action)

        Public ReadOnly Property RK4Solver As RungeKutta4
            Get
                Return rk4
            End Get
        End Property

        Sub New(rk4 As RungeKutta4)
            Me.rk4 = rk4
        End Sub

        Public Function Config(y0 As Double(), n As Integer, a As Double, b As Double) As SolverIterator
            solverEnumerator = rk4 _
                .solverIteration(y0, n, a, b) _
                .GetEnumerator

            Return Me
        End Function

        Public Function Bind(trigger As Action) As SolverIterator
            triggers.Add(trigger)
            Return Me
        End Function

        ''' <summary>
        ''' 这个方法接口主要是应用于模拟器计算
        ''' </summary>
        Public Sub Tick()
            Call solverEnumerator.MoveNext()

            For Each action In triggers
                Call action()
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return rk4.ToString
        End Function
    End Class
End Namespace