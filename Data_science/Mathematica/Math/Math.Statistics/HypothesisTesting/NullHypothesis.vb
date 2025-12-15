#Region "Microsoft.VisualBasic::1ea80d3fda02b4507d0f6b1c800c45b5, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\NullHypothesis.vb"

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

'   Total Lines: 62
'    Code Lines: 45 (72.58%)
' Comment Lines: 8 (12.90%)
'    - Xml Docs: 50.00%
' 
'   Blank Lines: 9 (14.52%)
'     File Size: 2.50 KB


'     Class NullHypothesis
' 
'         Properties: Permutation
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Pvalue
' 
' 
' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Hypothesis

    Public MustInherit Class NullHypothesis(Of T)

        Public ReadOnly Property Permutation As Integer

        Sub New(Optional permutation As Integer = 1000)
            _Permutation = permutation
        End Sub

        ''' <summary>
        ''' generates the random set with <see cref="permutation"/> elements.
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function ZeroSet() As IEnumerable(Of T)
        Public MustOverride Function Score(x As T) As Double

        Public Function Pvalue(score As Double, Optional alternative As Hypothesis = Hypothesis.Greater) As Double
            Dim zero As T() = ZeroSet.ToArray
            Dim n As Integer

            Select Case alternative
                Case Hypothesis.Greater
                    ' mu > mu0
                    n = Aggregate x As T
                    In zero.AsParallel
                    Let per_score As Double = Me.Score(x)
                    Where per_score >= score
                    Into Count
                Case Hypothesis.Less
                    ' mu < mu0
                    n = Aggregate x As T
                    In zero.AsParallel
                    Let per_score As Double = Me.Score(x)
                    Where per_score <= score
                    Into Count
                Case Hypothesis.TwoSided
                    ' 计算观测统计量的绝对值，用于双侧检验判断极端性
                    Dim abs_score As Double = std.Abs(score)

                    ' 双侧检验：置换统计量的绝对值 >= 观测统计量的绝对值
                    n = Aggregate x As T
                    In zero.AsParallel
                    Let per_score As Double = Me.Score(x)
                    Let abs_per_score As Double = std.Abs(per_score)
                    Where abs_per_score >= abs_score
                    Into Count
                Case Else
                    Throw New InvalidProgramException($"unknown alternative hypothesis: {alternative}!")
            End Select

            If alternative = Hypothesis.TwoSided Then
                Return 2 * (n + 1) / (Permutation + 1)
            Else
                Return (n + 1) / (Permutation + 1)
            End If
        End Function
    End Class
End Namespace
