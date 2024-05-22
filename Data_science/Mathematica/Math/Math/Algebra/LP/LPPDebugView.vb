#Region "Microsoft.VisualBasic::e66d55fd732345001f5701d3735e0fb5, Data_science\Mathematica\Math\Math\Algebra\LP\LPPDebugView.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 51
    '    Code Lines: 41 (80.39%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (19.61%)
    '     File Size: 2.06 KB


    '     Module LPPDebugView
    ' 
    '         Function: displayEqLine, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
