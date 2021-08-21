Namespace LinearAlgebra.LinearProgramming

    Module LPPDebugView

        Public Function ToString(lpp As LPP) As String
            Dim output As String = lpp.objectiveFunctionType.Description

            output = output & "  " & displayEqLine(lpp.objectiveFunctionCoefficients.ToArray, lpp.variableNames)
            output = output & ControlChars.Lf & "subject to the constraints:" & ControlChars.Lf

            For j As Integer = 0 To lpp.constraintRightHandSides.Length - 1
                Dim constraint() As Double = lpp.constraintCoefficients(j).ToArray
                output += displayEqLine(constraint, lpp.variableNames)
                output &= " " & lpp.constraintTypes(j)
                output &= " " & lpp.constraintRightHandSides(j).ToString(LPP.DecimalFormat)
                output += ControlChars.Lf
            Next

            Return output & ControlChars.Lf
        End Function

        Private Function displayEqLine(coefficients() As Double, variableNames As List(Of String)) As String
            Dim output As String = ""

            Dim startIndex As Integer = 1
            For i As Integer = 0 To variableNames.Count - 1
                If coefficients(i) <> 0 Then
                    output = output + coefficients(i).ToString(LPP.DecimalFormat) + variableNames(i)
                    Exit For
                Else
                    startIndex += 1
                End If
            Next

            For i As Integer = startIndex To variableNames.Count - 1
                Dim signString As String = " + "
                Dim sign As Double = 1.0

                If coefficients(i) < 0.0 Then
                    signString = " - "
                    sign = -1
                End If
                If coefficients(i) <> 0 Then
                    output = output + signString + (sign * coefficients(i)).ToString(LPP.DecimalFormat) + variableNames(i)
                End If
            Next

            Return output
        End Function
    End Module
End Namespace