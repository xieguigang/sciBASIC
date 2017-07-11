Imports Microsoft.VisualBasic.Language

''' <summary>
''' How does two <see cref="MassPoint"/> object reacting with each other.
''' </summary>
Public Module Reactions

    ''' <summary>
    ''' 两个点电荷之间的库仑力
    ''' </summary>
    ''' <param name="m1"></param>
    ''' <param name="m2"></param>
    ''' <param name="k#"></param>
    ''' <returns></returns>
    Public Function CoulombsLaw(m1 As MassPoint, m2 As MassPoint, Optional k# = 9000000000.0) As Force
        Dim d = m1.Point - m2.Point
        Dim f = Math.CoulombsLaw(m1.Charge, m2.Charge, d.SumMagnitude, k)

        With RepulsiveForce(f, m1.Point, m2.Point)
            .source = NameOf(CoulombsLaw)
            Return .ref
        End With
    End Function
End Module
