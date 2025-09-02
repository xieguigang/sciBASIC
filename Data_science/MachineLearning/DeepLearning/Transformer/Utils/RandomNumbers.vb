Imports std = System.Math

Namespace Transformer
    Public NotInheritable Class RandomNumbers
        Private Shared ReadOnly instanceField As RandomNumbers = New RandomNumbers()
        Public Shared ReadOnly Property Instance As RandomNumbers
            Get
                Return instanceField
            End Get
        End Property

        ' Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        Shared Sub New()
        End Sub

        Private Sub New()
        End Sub

        ' private Random _rand = new Random(DateTime.Now.Second);
        Private _rand As Random = New Random(0)

        Public Function GetNextUniformNumber() As Double
            Return _rand.NextDouble()
        End Function

        Public Function GetNextNormalNumber() As Double
            Dim u1 As Double = 1.0 - _rand.NextDouble() 'uniform(0,1] random doubles
            Dim u2 As Double = 1.0 - _rand.NextDouble()
            Dim randStdNormal = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2) 'random normal(0,1)

            Return randStdNormal
        End Function
    End Class
End Namespace
