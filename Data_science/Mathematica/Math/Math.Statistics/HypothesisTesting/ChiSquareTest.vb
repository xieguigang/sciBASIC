#Region "Microsoft.VisualBasic::846f72edd077ffd93a0420b2e363fc3d, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\ChiSquareTest.vb"

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

    '   Total Lines: 130
    '    Code Lines: 78 (60.00%)
    ' Comment Lines: 36 (27.69%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 16 (12.31%)
    '     File Size: 4.30 KB


    ' Class ChiSquareTest
    ' 
    '     Properties: chi_square, expected, observed, pvalue
    '     Enum ConfidenceLevels
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: ChiSquareValue, (+2 Overloads) Test, TranslateLevel
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.Statistics.Distributions

''' <summary>
''' 
''' </summary>
''' <remarks>
''' 卡方检验
''' 
''' 卡方检验用于确定预期频数（expected frequencies）和观察频数（observed frequencies）
''' 在一个或多个类别中是否存在显著差异。预期频数是指在假设两个变量独立的情况下，根据边际
''' 总数计算出的理论频数。观察频数则是在实际数据中观察到的频数。
''' </remarks>
Public Class ChiSquareTest

    Public Property observed As Double()()
    Public Property expected As Double()()
    Public Property chi_square As Double
    Public Property pvalue As Double

    ''' <summary>
    ''' 97.5%
    ''' </summary>
    Public Const C97_5 = 9.35
    ''' <summary>
    ''' 50%
    ''' </summary>
    Public Const C50 = 0.4549
    ''' <summary>
    ''' 75%
    ''' </summary>
    Public Const C75 = 1.3233
    ''' <summary>
    ''' 90%
    ''' </summary>
    Public Const C90 = 2.4477
    ''' <summary>
    ''' 95%
    ''' </summary>
    Public Const C95 = 5.9915
    ''' <summary>
    ''' 99%
    ''' </summary>
    Public Const C99 = 9.2103

    Public Enum ConfidenceLevels
        <Description("50%")> C50
        <Description("75%")> C75
        <Description("90%")> C90
        <Description("95%")> C95
        <Description("97.5%")> C97_5
        <Description("99%")> C99
    End Enum

    Public Shared Function TranslateLevel(level As Double) As ConfidenceLevels
        Dim lv As Integer = CInt(level * 100)

        Select Case lv
            Case 98 : Return ConfidenceLevels.C97_5
            Case 50 : Return ConfidenceLevels.C50
            Case 75 : Return ConfidenceLevels.C75
            Case 90 : Return ConfidenceLevels.C90
            Case 95 : Return ConfidenceLevels.C95
            Case 99 : Return ConfidenceLevels.C99
            Case Else
                Throw New NotImplementedException(level)
        End Select
    End Function

    Public Shared Function ChiSquareValue(level As ConfidenceLevels) As Double
        Select Case level
            Case ConfidenceLevels.C50 : Return C50
            Case ConfidenceLevels.C75 : Return C75
            Case ConfidenceLevels.C90 : Return C90
            Case ConfidenceLevels.C95 : Return C95
            Case ConfidenceLevels.C97_5 : Return C97_5
            Case ConfidenceLevels.C99 : Return C99
            Case Else
                Throw New NotImplementedException(level.ToString)
        End Select
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="observed">
    ''' shoudl be matrix with two rows and two columns.
    ''' </param>
    ''' <returns></returns>
    Public Shared Function Test(observed As Double()(), expected As Double()()) As ChiSquareTest
        Dim chi_squared_stat As Double = 0

        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                chi_squared_stat += (observed(i)(j) - expected(i)(j)) * (observed(i)(j) - expected(i)(j)) / expected(i)(j)
            Next
        Next

        ' 自由度（对于二联表始终为1）
        Const freedom As Integer = 1

        Dim p As Double = Distribution.ChiSquare(chi_squared_stat, freedom)

        Return New ChiSquareTest With {
            .chi_square = chi_squared_stat,
            .expected = expected,
            .observed = observed,
            .pvalue = p
        }
    End Function

    Public Shared Function Test(observed As Double()()) As ChiSquareTest
        Dim expected As Double()() = RectangularArray.Matrix(Of Double)(2, 2)
        Dim row_sums = observed.Select(Function(r) r.Sum).ToArray
        Dim col_sums = New Double(1) {}
        Dim total As Integer = row_sums.Sum

        col_sums(0) = observed.Select(Function(r) r(0)).Sum
        col_sums(1) = observed.Select(Function(r) r(1)).Sum

        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                expected(i)(j) = (row_sums(i) * col_sums(j)) / total
            Next
        Next

        Return Test(observed, expected)
    End Function
End Class
