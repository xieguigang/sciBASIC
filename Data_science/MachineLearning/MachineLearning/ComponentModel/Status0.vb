Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace ComponentModel

    Public MustInherit Class Status0

        Public MustOverride Function GetInitialValue() As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetVector(dims As Integer) As Vector
            Return New Vector(GetFloats(count:=dims))
        End Function

        Public Iterator Function GetFloats(count As Integer) As IEnumerable(Of Single)
            For i As Integer = 0 To count - 1
                Yield GetInitialValue()
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(a As Status0) As Single
            Return a.GetInitialValue
        End Operator

    End Class

    Public Class ConstantStatus0 : Inherits Status0

        Public Property C As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetInitialValue() As Single
            Return C
        End Function

        Public Overrides Function ToString() As String
            Return C.ToString
        End Function
    End Class

    Public Class RandomStatus0 : Inherits Status0

        Public Property Min As Double
        Public Property Max As Double

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetInitialValue() As Single
            Return randf.NextDouble(Min, Max)
        End Function

        Public Overrides Function ToString() As String
            Return $"randf({Min}, {Max}) = {GetInitialValue()}"
        End Function
    End Class
End Namespace