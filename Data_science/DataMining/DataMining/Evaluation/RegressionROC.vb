#Region "Microsoft.VisualBasic::4b62438d4bcd653b5f3a6212110858da, Data_science\DataMining\DataMining\Evaluation\RegressionROC.vb"

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

    '   Total Lines: 94
    '    Code Lines: 73 (77.66%)
    ' Comment Lines: 7 (7.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (14.89%)
    '     File Size: 3.48 KB



    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace Evaluation

    ''' <summary>
    ''' 回归模型的 ROC 评估。
    ''' 
    ''' 通过把连续真值按照阈值切分为二分类问题，从而为回归结果构造 ROC 曲线。
    ''' 曲线的生成统一委托到 <see cref="RocBuilder.SweepThresholds(Of T)"/>，
    ''' 本模块只保留「如何把回归误差转换为二分类判定」的业务逻辑。
    ''' </summary>
    Public Module RegressionROC

        ''' <summary>
        ''' 由 ``(预测值, 真值)`` 序列构建回归 ROC 曲线。
        ''' </summary>
        ''' <param name="predicts"></param>
        ''' <param name="labels"></param>
        ''' <param name="range">真值的取值范围（为空时自动推断）</param>
        ''' <param name="eps">判定「命中」的误差容差</param>
        ''' <param name="n">阈值扫描步数</param>
        ''' <returns></returns>
        Public Function ROC(predicts As Double(), labels As Double(),
                                     Optional range As DoubleRange = Nothing,
                                     Optional eps As Double = 0.1,
                                     Optional n As Integer = 25) As IEnumerable(Of Validation)

            Return predicts _
                .Select(Function(fx, i)
                            Return New RegressionClassify With {
                                .predicts = fx,
                                .actual = labels(i),
                                .sampleID = $"v_{i + 1}"
                            }
                        End Function) _
                .ROC(range, eps, n)
        End Function

        ''' <summary>
        ''' Evaluate the regression model ROC
        ''' </summary>
        ''' <param name="test"></param>
        ''' <param name="range">the value range of the label</param>
        ''' <param name="eps">判定「命中」的误差容差</param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ROC(test As IEnumerable(Of RegressionClassify),
                            Optional range As DoubleRange = Nothing,
                            Optional eps As Double = 0.1,
                            Optional n As Integer = 25) As IEnumerable(Of Validation)

            Dim allTest As RegressionClassify() = test.ToArray

            If range Is Nothing Then
                range = allTest _
                    .Select(Function(t) t.actual) _
                    .Range
            ElseIf range.Length = 0 Then
                Throw New InvalidConstraintException("label value range can not be empty!")
            End If

            Dim steps As Integer = If(n <= 0, 25, n)
            Dim d As Double = 1 / steps
            Dim cutoffs As New List(Of Double)()
            Dim i As Double = 0

            Do While i <= 1.0
                cutoffs.Add(i)
                i += d
            Loop

            Dim validate As New RegressionHelper With {.allTest = allTest, .eps = eps}

            Return RocBuilder.SweepThresholds(
                entity:=allTest,
                getValidate:=Function(any, cutoff) validate.label(any, cutoff),
                getPredict:=Function(any, cutoff) validate.predict(any, cutoff),
                cutoffs:=cutoffs
            )
        End Function

        Private Class RegressionHelper

            Public allTest As RegressionClassify()
            Public eps As Double

            Public Function label(i As RegressionClassify, cutoff As Double) As Boolean
                If i.actual >= cutoff Then
                    Return True
                Else
                    Return False
                End If
            End Function

            Public Function predict(i As RegressionClassify, cutoff As Double) As Boolean
                If i.errors <= eps Then
                    If i.actual >= cutoff Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    If i.actual >= cutoff Then
                        Return False
                    Else
                        Return True
                    End If
                End If
            End Function

        End Class
    End Module
End Namespace
