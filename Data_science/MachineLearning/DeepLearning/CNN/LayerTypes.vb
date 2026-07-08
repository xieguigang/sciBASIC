#Region "Microsoft.VisualBasic::90ef35d3c1daed44617a0a3923f04e62, Data_science\MachineLearning\DeepLearning\CNN\LayerTypes.vb"

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

    '   Total Lines: 36
    '    Code Lines: 23 (63.89%)
    ' Comment Lines: 9 (25.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (11.11%)
    '     File Size: 1.09 KB


    '     Enum LayerTypes
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace CNN

    Public Enum LayerTypes As Integer

        ''' <summary>
        ''' conv
        ''' </summary>
        <Description("conv")> Convolution
        ''' <summary>
        ''' input
        ''' </summary>
        <Description("input")> Input
        ''' <summary>
        ''' output
        ''' </summary>
        <Description("output")> Output
        <Description("pool")> Pool
        <Description("relu")> ReLU
        <Description("softmax")> SoftMax
        <Description("sigmoid")> Sigmoid
        <Description("tanh")> Tanh
        <Description("maxout")> Maxout
        <Description("lrn")> LRN
        <Description("dropout")> Dropout

        ''' <summary>
        ''' linear activation kind
        ''' </summary>
        ''' <remarks>
        ''' 在神经网络中，Linear 层指的就是 Fully Connected 层（全连接层，常简称为 FC 层）。这一层的数学本质是一个线性变换，公式为 y=Wx+b。因为没有非线性激活函数（虽然后面通常会接一个激活函数如 ReLU），它执行的是纯粹的线性映射，因此称为“线性层”。
        ''' </remarks>
        <Description("fully_connect")> FullyConnected

        <Description("regression")> Regression
        <Description("svm")> SVM
        <Description("conv_transpose")> Conv2DTranspose
        <Description("fourier_feature")> FourierFeature
        <Description("leaky_relu")> LeakyReLU
        <Description("gaussian")> Gaussian

        <Description("linear")> Linear = FullyConnected
    End Enum
End Namespace
