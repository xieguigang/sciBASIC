#Region "Microsoft.VisualBasic::5958117d0ea7042a55b8eb6553e86cfb, Data_science\MachineLearning\DeepLearning\Transformer\Optimizer.vb"

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

    '   Total Lines: 33
    '    Code Lines: 24 (72.73%)
    ' Comment Lines: 3 (9.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (18.18%)
    '     File Size: 1.09 KB


    '     Class Optimizer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: MakeTrainingStep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace Transformer
    ''' <summary>
    ''' Implements the Adam optimizer
    ''' </summary>
    Public Class Optimizer
        Private Const beta1 As Double = 0.9
        Private Const beta2 As Double = 0.999
        Private Const eps As Double = 0.00000001

        Private M As Tensor
        Private V As Tensor

        Public Sub New(T As Tensor)
            M = New Tensor(T) * 0
            V = New Tensor(T) * 0
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer, T As Tensor)
            M = beta1 * M + (1.0 - beta1) * T.GetDerivatives()
            V = beta2 * V + (1.0 - beta2) * T.GetDerivatives().Pow(2)
            Dim m_hat = M / (1.0 - std.Pow(beta1, [step]))
            Dim v_hat = V / (1.0 - std.Pow(beta2, [step]))
            Dim correction = -learningRate * m_hat / (v_hat.Pow(0.5) + eps)
            T.MatAdd(correction)
            T.ClearDerivatives()
        End Sub


    End Class
End Namespace

