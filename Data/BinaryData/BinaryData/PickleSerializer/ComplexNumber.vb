Namespace Pickle

    ''' <summary>
    ''' 表示 Python 的复数类型 (complex)。
    ''' Python 原生支持复数运算，.NET 没有内置复数类型（System.Numerics.Complex 
    ''' 需要额外引用），因此提供此轻量级实现。
    ''' </summary>
    Public Class ComplexNumber
        ''' <summary>实部</summary>
        Public ReadOnly Property Real As Double

        ''' <summary>虚部</summary>
        Public ReadOnly Property Imaginary As Double

        Public Sub New(real As Double, imaginary As Double)
            Me.Real = real
            Me.Imaginary = imaginary
        End Sub

        Public Overrides Function ToString() As String
            If Imaginary >= 0 Then
                Return $"{Real}+{Imaginary}j"
            Else
                Return $"{Real}{Imaginary}j"
            End If
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, ComplexNumber)
            If other Is Nothing Then Return False
            Return Real = other.Real AndAlso Imaginary = other.Imaginary
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return Real.GetHashCode() Xor Imaginary.GetHashCode()
        End Function
    End Class

End Namespace