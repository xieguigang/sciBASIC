#Region "Microsoft.VisualBasic::b7a38f375afa824c0d5309b49db8ee61, Data_science\DataMining\DataMining\Evaluation\RegressionROC.vb"

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
'    Code Lines: 35 (68.63%)
' Comment Lines: 7 (13.73%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 9 (17.65%)
'     File Size: 1.73 KB


'     Module RegressionROC
' 
'         Function: ROC
' 
' 
' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace Evaluation

    Public Module RegressionROC

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
        ''' <param name="n"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function ROC(test As IEnumerable(Of RegressionClassify),
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

            Dim d As Double = 1 / n
            Dim i As Double = 0
            Dim validate As New RegressionHelper With {.allTest = allTest, .eps = eps}

            Do While i <= 1.0
                Yield Validation.Calc(
                    entity:=allTest,
                    getValidate:=Function(any) validate.label(any, i),
                    getPredict:=Function(any) validate.predict(any, i),
                    percentile:=i
                )

                i += d
            Loop
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
