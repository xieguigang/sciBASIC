#Region "Microsoft.VisualBasic::19de6bd9898c3ec432f52570db8e30c0, Microsoft.VisualBasic.Core\src\Extensions\Math\NumeralSystem.vb"

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

    '   Total Lines: 26
    '    Code Lines: 15 (57.69%)
    ' Comment Lines: 6 (23.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (19.23%)
    '     File Size: 750 B


    '     Module NumeralSystem
    ' 
    '         Function: TranslateDecimal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math

    Public Module NumeralSystem

        ''' <summary>
        ''' A helper function for translate decimal number to the number of another kind of custom number system
        ''' </summary>
        ''' <param name="d"></param>
        ''' <param name="alphabets"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 将十进制数转换到另外的一个数进制
        ''' </remarks>
        <Extension>
        Public Function TranslateDecimal(d%, alphabets As Char()) As String
            Dim r = d Mod alphabets.Length
            Dim result$

            If (d - r = 0) Then
                result = alphabets(r)
            Else
                result = ((d - r) \ alphabets.Length).TranslateDecimal(alphabets) & alphabets(r)
            End If

            Return result
        End Function

        ''' <summary>
        ''' Method which finds root of specific degree of number.
        ''' </summary>
        ''' <param name="number">Source number.</param>
        ''' <param name="degree">Degree of root.</param>
        ''' <param name="precision">Precision with which the calculations are performed. value should be in range (0,1).</param>
        ''' <returns>Root of number.</returns>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when values of degree or precision are out of range.</exception>
        ''' <exception cref="ArgumentException">Thrown when root's degree is even for calculation with negative numbers.</exception>
        ''' <remarks>
        ''' 开n次方
        ''' </remarks>
        Public Function FindNthRoot(number As Double, degree As Integer, precision As Double) As Double
            If degree < 0 Then
                Throw New ArgumentOutOfRangeException($"{degree} is out of range.")
            End If

            If precision <= 0 OrElse precision >= 1 Then
                Throw New ArgumentOutOfRangeException($"{precision} is out of range.")
            End If

            If number < 0 AndAlso degree Mod 2 = 0 Then
                Throw New ArgumentException("Root's degree cannot be even for calculation with negative numbers.")
            End If

            If degree = 1 Then
                Return number
            End If

            Dim current As Double = 1
            Dim [next] = ((degree - 1) * current + number / std.Pow(current, degree - 1)) / degree

            While std.Abs([next] - current) > precision
                current = [next]
                [next] = ((degree - 1) * current + number / std.Pow(current, degree - 1)) / degree
            End While

            Return [next]
        End Function
    End Module
End Namespace
