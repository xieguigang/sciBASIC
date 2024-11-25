#Region "Microsoft.VisualBasic::002752e9432384c651c3c6094b682d46, Data_science\MachineLearning\DeepLearning\CNN\trainers\AdaDeltaTrainer.vb"

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

    '   Total Lines: 45
    '    Code Lines: 29 (64.44%)
    ' Comment Lines: 6 (13.33%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 10 (22.22%)
    '     File Size: 1.53 KB


    '     Class AdaDeltaTrainer
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
    ''' Adaptive delta will look at the differences between the expected result and the current result to train the network.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class AdaDeltaTrainer : Inherits TrainerAlgorithm

        Dim ro As Double = 0.95

        Public Sub New(batch_size As Integer, l2_decay As Single, Optional ro As Double = 0.95)
            MyBase.New(batch_size, l2_decay)
            Me.ro = ro
        End Sub

        Public Overrides Sub initTrainData(bpr As BackPropResult)
            Dim newXSumArr = New Double(bpr.Weights.Length - 1) {}

            Call newXSumArr.fill(0)
            Call xsum.Add(newXSumArr)
        End Sub

        Public Overrides Sub update(i As Integer, j As Integer, gij As Double, p As Double())
            Dim gsumi = gsum(i)
            Dim xsumi = xsum(i)
            Dim dx As Double

            gsumi(j) = ro * gsumi(j) + (1 - ro) * gij * gij
            dx = (xsumi(j) + eps) / (gsumi(j) + eps)
            dx = -std.Sqrt(If(dx < 0, 0, dx)) * gij
            ' yes, xsum lags behind gsum by 1.
            xsumi(j) = ro * xsumi(j) + (1 - ro) * dx * dx
            p(j) += dx
        End Sub

        Public Overrides Function ToString() As String
            Return $"ada_delta(ro:{ro})"
        End Function
    End Class

End Namespace
