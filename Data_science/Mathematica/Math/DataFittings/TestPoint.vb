Imports System.Drawing
Imports System.Runtime.CompilerServices

Public Structure TestPoint

    Public Property X As Double
    Public Property Y As Double
    Public Property Yfit As Double

    Public ReadOnly Property Err As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Y - Yfit
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{X.ToString("F2")}, {Y.ToString("F2")}] {Yfit.ToString("F2")}"
    End Function

    Public Shared Narrowing Operator CType(point As TestPoint) As PointF
        Return New PointF(point.X, point.Y)
    End Operator
End Structure