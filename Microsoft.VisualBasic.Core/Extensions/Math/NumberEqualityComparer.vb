Imports System.Runtime.CompilerServices

Namespace Math

    ''' <summary>
    ''' 当两个数的误差值的绝对值小于阈值的时候认为两个数字相等
    ''' </summary>
    Public Class NumberEqualityComparer : Implements IEqualityComparer(Of Double)

        Public Property DeltaTolerance As Double

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(tolerance As Double)
            DeltaTolerance = tolerance
        End Sub

        Sub New()
            Call Me.New(0.00001)
        End Sub

        Public Overrides Function ToString() As String
            Return $"|a-b| <= {DeltaTolerance}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Equals() As GenericLambda(Of Double).IEquals
            Return AddressOf Equals
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Equals(x As Double, y As Double) As Boolean Implements IEqualityComparer(Of Double).Equals
            Return Math.Abs(x - y) <= _DeltaTolerance
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function GetHashCode(obj As Double) As Integer Implements IEqualityComparer(Of Double).GetHashCode
            Return obj.GetHashCode
        End Function
    End Class

End Namespace