Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace Quantile

    Public Structure DataQuartile

        Public ReadOnly Property Q1 As Double
        Public ReadOnly Property Q2 As Double
        Public ReadOnly Property Q3 As Double
        Public ReadOnly Property IQR As Double
        Public ReadOnly Property range As DoubleRange

        Public Sub New(Q1#, Q2#, Q3#, IQR#, range As DoubleRange)
            Me.Q1 = Q1
            Me.Q2 = Q2
            Me.Q3 = Q3
            Me.IQR = IQR
            Me.range = range
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Outlier(samples As IEnumerable(Of Double)) As (normal As Double(), outlier As Double())
            Return samples.AsVector.Outlier(Me)
        End Function
    End Structure

    Public Enum QuartileLevels As Integer
        Q1 = 1
        Q2 = 2
        Q3 = 3
    End Enum
End Namespace