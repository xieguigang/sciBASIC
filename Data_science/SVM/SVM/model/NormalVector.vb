Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Model

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public Class NormalVector : Implements ICloneable

        Public Property W1 As Double
        Public Property W2 As Double

        Public Sub New(w1 As Double, w2 As Double)
            Me.W1 = w1
            Me.W2 = w2
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Overrides Function Equals(o As Object) As Boolean
            If TypeOf o Is NormalVector Then
                Dim vector As NormalVector = CType(o, NormalVector)
                Dim res As Double = (vector.W2 / vector.W1) / (W2 / W1)
                Return res < 1.0001 AndAlso res > 0.999
            Else
                Return MyBase.Equals(o)
            End If
        End Function

        Public Function Clone() As NormalVector
            Return New NormalVector(W1, W2)
        End Function

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone
            Return clone()
        End Function
    End Class
End Namespace