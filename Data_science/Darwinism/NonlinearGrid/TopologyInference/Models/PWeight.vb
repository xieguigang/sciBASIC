Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

Public Class PWeight : Implements ICloneable(Of PWeight)

    Public Property W As Vector

    Public Function Evaluate(X As Vector) As Double
        Return (X ^ W).Product
    End Function

    Public Overrides Function ToString() As String
        Return W.ToString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Clone() As PWeight Implements ICloneable(Of PWeight).Clone
        Return New PWeight With {
            .W = New Vector(W.AsEnumerable)
        }
    End Function
End Class
