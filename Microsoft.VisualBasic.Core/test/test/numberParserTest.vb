#Region "Microsoft.VisualBasic::a26adfd48eee1a7ad4ef845b3eff9888, Microsoft.VisualBasic.Core\test\test\numberParserTest.vb"

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

    '   Total Lines: 133
    '    Code Lines: 92 (69.17%)
    ' Comment Lines: 8 (6.02%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 33 (24.81%)
    '     File Size: 3.78 KB


    ' Module numberParserTest
    ' 
    '     Function: IsDoubleNumber, IsInteger
    ' 
    '     Sub: hexTest, Main1, testInt, testInt2
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Module numberParserTest

    Sub hexTest()

        Dim xx As Double = Double.MaxValue

        Call Console.WriteLine(xx.ToString("G17"))

        Dim x As f64 = Double.MaxValue
        Dim hex = x.Hex

        Call Console.WriteLine(hex)

        Pause()
    End Sub

    Sub Main1()

        Call Console.WriteLine("6.24551029215526E-06".IsNumeric)

        Call hexTest()

        Dim dbl1 = "-4.6465E+65"
        Dim dbl2 = "553453453445"
        Dim dbl3 = ".423423"
        Dim dbl4 = "+.364534e53"
        Dim invalidDbl = "244.24.234"

        Call Console.WriteLine(IsDoubleNumber(dbl1) & vbTab & PrimitiveParser.IsNumeric(dbl1) & vbTab & Val(dbl1))
        Call Console.WriteLine(IsDoubleNumber(dbl2) & vbTab & PrimitiveParser.IsNumeric(dbl2) & vbTab & Val(dbl2))
        Call Console.WriteLine(IsDoubleNumber(dbl3) & vbTab & PrimitiveParser.IsNumeric(dbl3) & vbTab & Val(dbl3))
        Call Console.WriteLine(IsDoubleNumber(dbl4) & vbTab & PrimitiveParser.IsNumeric(dbl4) & vbTab & Val(dbl4))
        Call Console.WriteLine(IsDoubleNumber(invalidDbl) & vbTab & PrimitiveParser.IsNumeric(invalidDbl) & vbTab & Val(invalidDbl))


        Dim int1 = "654646"
        Dim int2 = "+5453"
        Dim int3 = "-3434534"
        Dim invalidInt = "-3E-10"

        Call Console.WriteLine(IsInteger(int1))
        Call Console.WriteLine(IsInteger(int2))
        Call Console.WriteLine(IsInteger(int3))
        Call Console.WriteLine(IsInteger(invalidInt))


        Call New Action(AddressOf testInt).BENCHMARK
        Call New Action(AddressOf testInt2).BENCHMARK

        Pause()
    End Sub

    Private Sub testInt2()
        Dim test As Boolean

        For i As Integer = 0 To 100000000
            test = Regex.Match("234234233", "\d+").Success = "234234233"
        Next
    End Sub

    Private Sub testInt()
        Dim test As Boolean

        For i As Integer = 0 To 100000000
            test = IsInteger("234234233")
        Next
    End Sub

    <Extension>
    Public Function IsDoubleNumber(num As String) As Boolean
        Dim dotCheck As Boolean = False
        Dim c As Char = num(Scan0)
        Dim offset As Integer = 0

        If c = "-"c OrElse c = "+"c Then
            ' check for number sign symbol
            '
            ' +3.0
            ' -3.0
            offset = 1
        ElseIf c = "."c Then
            ' check for 
            ' 
            ' .1 (0.1)
            offset = 1
            dotCheck = True
        End If

        For i As Integer = offset To num.Length - 1
            c = num(i)

            If Not (c >= ZERO AndAlso c <= NINE) Then
                If c = "."c Then
                    If dotCheck Then
                        Return False
                    Else
                        dotCheck = True
                    End If
                ElseIf c = "E"c OrElse c = "e"c Then
                    Return IsInteger(num, i + 1)
                End If
            End If
        Next

        Return True
    End Function

    Const ZERO As Char = "0"c
    Const NINE As Char = "9"c

    Public Function IsInteger(num As String, Optional offset As Integer = 0) As Boolean
        Dim c As Char = num(Scan0)

        ' check for number sign symbol
        If c = "-"c OrElse c = "+"c Then
            offset += 1
        End If

        For i As Integer = offset To num.Length - 1
            c = num(i)

            If Not (c >= ZERO AndAlso c <= NINE) Then
                Return False
            End If
        Next

        Return True
    End Function
End Module
