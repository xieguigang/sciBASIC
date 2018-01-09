Imports System.Runtime.CompilerServices

Namespace Algebra.LinearProgramming

    Public Module Extensions

        Friend Function copyOf(Of T)(original As T(), newLength%) As T()
            Dim copy As T() = New T(newLength - 1) {}
            Array.Copy(original, 0, copy, 0, VBMath.Min(original.Length, newLength))
            Return copy
        End Function

        ''' <summary>
        ''' String formatting helper function.
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function formatDecimals(d As Double) As String
            Return d.ToString("#,###.###")
        End Function

        <Extension>
        Friend Function ParseType(type As String) As OptimizationType
            Select Case LCase(type)
                Case "max", "Maximize"
                    Return OptimizationType.MAX
                Case "min", "Minimize"
                    Return OptimizationType.MIN
                Case Else
                    Throw New NotImplementedException(type)
            End Select
        End Function
    End Module
End Namespace