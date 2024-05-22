#Region "Microsoft.VisualBasic::5d1365fbbbb8dd008dda8946a5f524c1, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\DynamicProgramming\Levenshtein\DamerauLevenshteinDistance.vb"

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

    '   Total Lines: 39
    '    Code Lines: 32 (82.05%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (17.95%)
    '     File Size: 1.70 KB


    '     Module DamerauLevenshteinDistance
    ' 
    '         Function: DamerauLevenshtein
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Algorithm.DynamicProgramming

    Public Module DamerauLevenshteinDistance

        Public Function DamerauLevenshtein(Of T)(down As T(), across As T(), equals As GenericLambda(Of T).IEquals, insert#, remove#, substitute As Func(Of T, T, Double), transpose As Func(Of T, T, Double)) As Double
            Dim matrix As New List(Of Double())

            For i As Integer = 0 To down.Length - 1
                Dim ds As Double() = New Double(across.Length - 1) {}
                Dim d = down(i)

                For j As Integer = 0 To across.Length - 1
                    Dim a = across(j)

                    If i = 0 AndAlso j = 0 Then
                        ds(j) = 0
                    ElseIf i = 0 Then
                        ds(j) = ds(j - 1) + insert
                    ElseIf j = 0 Then
                        ds(j) = matrix(i - 1)(j) + remove
                    Else
                        ds(j) = {
                            matrix(i - 1)(j) + remove,
                            ds(j - 1) + insert,
                            matrix(i - 1)(j - 1) + If(equals(d, a), 0, substitute(d, a))
                        }.Min

                        If i > 1 AndAlso j > 1 AndAlso equals(down(i - 1), a) AndAlso equals(d, across(j - 1)) Then
                            ds(j) = System.Math.Min(ds(j), matrix(i - 1)(j - 2) + If(equals(d, a), 0, transpose(d, down(i - 1))))
                        End If
                    End If
                Next
            Next

            Dim distance = matrix(down.Length - 1)(across.Length - 1)
            Return distance
        End Function
    End Module
End Namespace
