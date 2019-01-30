Imports Microsoft.VisualBasic.Language.JavaScript

Namespace Layouts.Cola

    Public Class Link3D

        Public length As Double
        Public source As Integer
        Public target As Integer

        Sub New()
        End Sub

        Public Sub New(source As Integer, target As Integer)
            Me.source = source
            Me.target = target
        End Sub

        Public Function actualLength(x As Double()()) As Double
            Return Math.Sqrt(x.Reduce(Function(c As Double, v As Double())
                                          Dim dx = v(Me.target) - v(Me.source)
                                          Return c + dx * dx
                                      End Function, 0))
        End Function
    End Class
End Namespace