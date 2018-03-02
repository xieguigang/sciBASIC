Imports System.Runtime.CompilerServices

Public Module Extensions

    ''' <summary>
    ''' Small delta for GA mutations
    ''' </summary>
    ''' <param name="x#"></param>
    ''' <param name="d#"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 1 = 10 ^ 0  ~  0.1 = 10 ^ 1 * 0.1
    ''' 10 = 10 ^ 1  ~ 1 = 10 ^ 2 * 0.1
    ''' </remarks>
    <Extension>
    Public Function Delta(x#, Optional d# = 1 / 10) As Double
        Dim p10 = Fix(Math.Log10(x))
        Dim small = (10 ^ (p10 + 1)) * d
        Return small
    End Function
End Module
