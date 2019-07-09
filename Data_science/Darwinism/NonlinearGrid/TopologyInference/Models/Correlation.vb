Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' A linear correlation system
''' </summary>
Public Class Correlation : Implements ICloneable(Of Correlation)

    Public Property B As Vector

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Evaluate(X As Vector) As Double
        Return (B * X).Sum
    End Function

    Public Overrides Function ToString() As String
        Return B.ToString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Clone() As Correlation Implements ICloneable(Of Correlation).Clone
        Return New Correlation With {
            .B = New Vector(B.AsEnumerable)
        }
    End Function
End Class
