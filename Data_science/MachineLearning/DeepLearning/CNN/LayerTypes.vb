#Region "Microsoft.VisualBasic::cbdd3e19c0fe58b125803dae4b3935e9, Data_science\MachineLearning\DeepLearning\CNN\LayerTypes.vb"

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
    '    Code Lines: 24 (53.33%)
    ' Comment Lines: 15 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (13.33%)
    '     File Size: 1.63 KB


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

    ''' <summary>
    ''' Identifies the kind of a CNN layer; the <see cref="DescriptionAttribute"/> value is the tag persisted in the
    ''' model file.
    ''' </summary>
    Public Enum LayerTypes As Integer

        ''' <summary>Convolution layer (<c>conv</c>).</summary>
        <Description("conv")> Convolution
        ''' <summary>Input layer (<c>input</c>).</summary>
        <Description("input")> Input
        ''' <summary>Output layer (<c>output</c>).</summary>
        <Description("output")> Output
        ''' <summary>Max pooling layer (<c>pool</c>).</summary>
        <Description("pool")> Pool
        ''' <summary>Rectified linear unit activation (<c>relu</c>).</summary>
        <Description("relu")> ReLU
        ''' <summary>Softmax activation (<c>softmax</c>).</summary>
        <Description("softmax")> SoftMax
        ''' <summary>Sigmoid activation (<c>sigmoid</c>).</summary>
        <Description("sigmoid")> Sigmoid
        ''' <summary>Hyperbolic tangent activation (<c>tanh</c>).</summary>
        <Description("tanh")> Tanh
        ''' <summary>Maxout activation (<c>maxout</c>).</summary>
        <Description("maxout")> Maxout
        ''' <summary>Local response normalization (<c>lrn</c>).</summary>
        <Description("lrn")> LRN
        ''' <summary>Dropout regularization (<c>dropout</c>).</summary>
        <Description("dropout")> Dropout

        ''' <summary>
        ''' Fully connected (linear) layer (<c>fully_connect</c>).
        ''' </summary>
        ''' <remarks>
        ''' In a neural network the linear layer is the fully connected layer (often abbreviated to FC layer). Its
        ''' mathematical nature is a linear transformation <c>y = Wx + b</c>: because it applies no non-linear activation
        ''' of its own, it performs a purely linear mapping and is therefore called a "linear layer".
        ''' </remarks>
        <Description("fully_connect")> FullyConnected

        ''' <summary>Regression loss layer (<c>regression</c>).</summary>
        <Description("regression")> Regression
        ''' <summary>Support vector machine loss layer (<c>svm</c>).</summary>
        <Description("svm")> SVM
        ''' <summary>Transposed (de)convolution layer (<c>conv_transpose</c>).</summary>
        <Description("conv_transpose")> Conv2DTranspose
        ''' <summary>Fourier feature mapping layer (<c>fourier_feature</c>).</summary>
        <Description("fourier_feature")> FourierFeature
        ''' <summary>Leaky ReLU activation (<c>leaky_relu</c>).</summary>
        <Description("leaky_relu")> LeakyReLU
        ''' <summary>Gaussian activation (<c>gaussian</c>).</summary>
        <Description("gaussian")> Gaussian

        ''' <summary>Alias of <see cref="FullyConnected"/> (<c>linear</c>).</summary>
        <Description("linear")> Linear = FullyConnected
    End Enum
End Namespace
