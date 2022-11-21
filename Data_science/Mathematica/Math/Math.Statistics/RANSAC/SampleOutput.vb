Namespace RANSAC

    Public Class SampleOutput

        Public Property bestPlane As Double()
        Public Property bestSupport As Double
        Public Property bestStd As Double
        Public Property inliersPercentage As Double
        Public Property N As Integer

        Public ReadOnly Property lost_points As Double
            Get
                Return N * inliersPercentage - bestSupport
            End Get
        End Property

        Public ReadOnly Property Accuracy As Double
            Get
                Return bestSupport / (N * inliersPercentage)
            End Get
        End Property

        Public Shared Sub Print(x As SampleOutput)
            Console.WriteLine("###OUTPUT###")
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("Best plane: {0:F6} {1:F6} {2:F6} {3:F6}", x.bestPlane(0), x.bestPlane(1), x.bestPlane(2), x.bestPlane(3))
            Console.ResetColor()
            Console.WriteLine("Best support (i.e. matched points): {0}", x.bestSupport)
            Console.WriteLine("Best standard deviation: {0}" & vbLf, x.bestStd)
            Console.WriteLine("Lost points: {0}" & vbLf & "Accuracy: {1:F6}" & vbLf, x.lost_points, x.Accuracy)
        End Sub
    End Class
End Namespace