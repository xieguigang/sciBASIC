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
    '    Code Lines: 35
    ' Comment Lines: 7
    '   Blank Lines: 9
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

        ''' <summary>
        ''' Evaluate the regression model ROC
        ''' </summary>
        ''' <param name="test"></param>
        ''' <param name="range"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function ROC(test As IEnumerable(Of RegressionClassify),
                                     Optional range As DoubleRange = Nothing,
                                     Optional n As Integer = 25) As IEnumerable(Of Validation)

            Dim allTest As RegressionClassify() = test.ToArray

            If range Is Nothing Then
                range = allTest _
                    .Select(Function(t) t.errors) _
                    .Range
            ElseIf range.Length = 0 Then
                Throw New InvalidConstraintException("error range can not be empty!")
            End If

            Dim d As Double = 1 / n
            Dim cutoff As Double
            Dim i As Double = 0
            Dim width As Double = range.Length
            Dim validate As Func(Of RegressionClassify, Boolean) = Function(any) True

            Do While i <= 1.0
                cutoff = (1 - i) * width

                Yield Validation.Calc(
                    entity:=allTest,
                    getValidate:=validate,
                    getPredict:=Function(t) t.errors < cutoff,
                    percentile:=i
                )

                i += d
            Loop
        End Function
    End Module
End Namespace
