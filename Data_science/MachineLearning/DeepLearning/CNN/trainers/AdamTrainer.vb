#Region "Microsoft.VisualBasic::80d72199a5b983abbe2c16f7c0b397b1, Data_science\MachineLearning\DeepLearning\CNN\trainers\AdamTrainer.vb"

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

    '   Total Lines: 47
    '    Code Lines: 32
    ' Comment Lines: 6
    '   Blank Lines: 9
    '     File Size: 1.96 KB


    '     Class AdamTrainer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: initTrainData, update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace CNN.trainers

    ''' <summary>
    ''' Adaptive Moment Estimation is an update to RMSProp optimizer. In this running average of both the
    ''' gradients and their magnitudes are used.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>

    Public Class AdamTrainer : Inherits TrainerAlgorithm

        Private ReadOnly beta1 As Double = 0.9
        Private ReadOnly beta2 As Double = 0.999

        Public Sub New(batch_size As Integer, l2_decay As Single, Optional beta1 As Double = 0.9, Optional beta2 As Double = 0.999)
            MyBase.New(batch_size, l2_decay)
            Me.beta1 = beta1
            Me.beta2 = beta2
        End Sub

        Public Overrides Sub initTrainData(bpr As BackPropResult)
            Dim newXSumArr = New Double(bpr.Weights.Length - 1) {}
            newXSumArr.fill(0)
            xsum.Add(newXSumArr)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)
            Dim xsumi = xsum(i)
            gsumi(j) = gsumi(j) * beta1 + (1 - beta1) * gij ' update biased first moment estimate
            xsumi(j) = xsumi(j) * beta2 + (1 - beta2) * gij * gij ' update biased second moment estimate
            Dim biasCorr1 = gsumi(j) * (1 - std.Pow(beta1, k)) ' correct bias first moment estimate
            Dim biasCorr2 = xsumi(j) * (1 - std.Pow(beta2, k)) ' correct bias second moment estimate
            Dim dx = -learning_rate * biasCorr1 / (std.Sqrt(biasCorr2) + eps)
            p(j) += dx
        End Sub

        Public Overrides Function ToString() As String
            Return $"adam(batch_size:{batch_size}, l2_decay:{l2_decay}, beta1:{beta1}, beta2:{beta2})"
        End Function
    End Class

End Namespace
