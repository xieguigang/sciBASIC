Imports System.Runtime.CompilerServices

Friend NotInheritable Class OptimizationState

    Public CurrentEpoch As Integer = 0
    Public Head As Integer() = New Integer(-1) {}
    Public Tail As Integer() = New Integer(-1) {}
    Public EpochsPerSample As Single() = New Single(-1) {}
    Public EpochOfNextSample As Single() = New Single(-1) {}
    Public EpochOfNextNegativeSample As Single() = New Single(-1) {}
    Public EpochsPerNegativeSample As Single() = New Single(-1) {}
    Public MoveOther As Boolean = True
    Public InitialAlpha As Single = 1
    Public Alpha As Single = 1
    Public Gamma As Single = 1
    Public A As Single = 1.57694352F
    Public B As Single = 0.8950609F
    Public [Dim] As Integer = 2
    Public NEpochs As Integer = 500
    Public NVertices As Integer = 0

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetDistanceFactor(distSquared As Single) As Single
        Return 1.0F / ((0.001F + distSquared) * CSng(A * (distSquared ^ B) + 1))
    End Function
End Class