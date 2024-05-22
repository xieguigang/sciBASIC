#Region "Microsoft.VisualBasic::60afca04da066478a71c8717bff8e9a7, Data_science\MachineLearning\TensorFlow\nn.vb"

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

    '   Total Lines: 26
    '    Code Lines: 21 (80.77%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (19.23%)
    '     File Size: 962 B


    ' Module nn
    ' 
    '     Function: log_softmax, sigmoid_cross_entropy_with_logits
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow

Public Module nn

    Public Function sigmoid_cross_entropy_with_logits(Optional _sentinel As Object = Nothing,
                                                      Optional labels As Vector = Nothing,
                                                      Optional logits As Vector = Nothing,
                                                      Optional name As String = Nothing) As Vector

        Dim x = logits, z = labels
        Dim logistic_loss = x - x * z + tf.log(1 + tf.exp(-x))

        Return logistic_loss
    End Function

    Public Function log_softmax(
    logits As Vector,
  Optional axis As Integer = Nothing,
  Optional name As String = Nothing,
  Optional [dim] As Object = Nothing
)
        Return logits - std.Log(tf.reduce_sum(tf.exp(logits)))
    End Function
End Module
