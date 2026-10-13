#Region "Microsoft.VisualBasic::, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\Core\ShapExplanation.vb"

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

    '   Total Lines: 0
    '    Code Lines: 0 (NaN%)
    ' Comment Lines: 0 (NaN%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 0 (NaN%)
    '     File Size: 0 B


    ' /********************************************************************************/

#End Region



Imports System.Linq

Namespace ShapleyValue

    ''' <summary>
    ''' 单个样本的 SHAP 解释结果。
    ''' </summary>
    Public Class ShapExplanation

        ''' <summary>
        ''' 特征名字，长度与 <see cref="Contributions"/> 一致。
        ''' </summary>
        ''' <returns></returns>
        Public Property FeatureNames As String()

        ''' <summary>
        ''' 逐特征的 SHAP 贡献值（不包含基线值）。
        ''' </summary>
        ''' <returns></returns>
        Public Property Contributions As Double()

        ''' <summary>
        ''' 基线值 / 期望值。
        ''' </summary>
        ''' <returns></returns>
        Public Property Baseline As Double

        ''' <summary>
        ''' 模型的样本 ID。
        ''' </summary>
        ''' <returns></returns>
        Public Property ID As String

        ''' <summary>
        ''' 模型在加性空间之中的取值：``Baseline + sum(Contributions)``。
        ''' </summary>
        ''' <returns></returns>
        Public Property AdditiveValue As Double

        ''' <summary>
        ''' 模型的原始输出。对于逻辑回归而言是类别概率，
        ''' 其与 <see cref="AdditiveValue"/> 之间相差一个非线性连接函数。
        ''' </summary>
        ''' <returns></returns>
        Public Property Output As Double

        ''' <summary>
        ''' 特征的数量。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Integer
            Get
                Return If(Contributions Is Nothing, 0, Contributions.Length)
            End Get
        End Property

        ''' <summary>
        ''' 按照贡献值的绝对值从大到小对特征进行排序。
        ''' </summary>
        ''' <returns></returns>
        Public Function Ranked() As (name As String, value As Double)()
            If Contributions Is Nothing Then
                Return New (name As String, value As Double)() {}
            End If

            Dim items As New List(Of (name As String, value As Double))()

            For i As Integer = 0 To Contributions.Length - 1
                Dim name As String = FeatureName(i)
                items.Add((name, Contributions(i)))
            Next

            Return items.OrderByDescending(Function(c) System.Math.Abs(c.value)).ToArray
        End Function

        ''' <summary>
        ''' 获取指定下标位置的特征名字；名字缺失的时候自动生成 ``F1..Fn``。
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Function FeatureName(i As Integer) As String
            If FeatureNames Is Nothing OrElse i >= FeatureNames.Length Then
                Return $"F{i + 1}"
            Else
                Return FeatureNames(i)
            End If
        End Function

        ''' <summary>
        ''' 获取指定名字的特征的贡献值；如果特征不存在则返回 0。
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function GetContribution(name As String) As Double
            If FeatureNames Is Nothing OrElse Contributions Is Nothing Then
                Return 0
            End If

            For i As Integer = 0 To FeatureNames.Length - 1
                If String.Equals(FeatureNames(i), name, StringComparison.OrdinalIgnoreCase) Then
                    Return Contributions(i)
                End If
            Next

            Return 0
        End Function

    End Class

End Namespace
