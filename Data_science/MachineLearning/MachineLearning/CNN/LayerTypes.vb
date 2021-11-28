Imports System.ComponentModel

Namespace Convolutional

    Public Enum LayerTypes
        <Description("conv")> Convolution
        <Description("input")> Input
        <Description("output")> Output
        <Description("pool")> Pool
        <Description("relu")> ReLU
        <Description("softmax")> SoftMax
    End Enum
End Namespace