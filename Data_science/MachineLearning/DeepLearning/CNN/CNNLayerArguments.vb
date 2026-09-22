Imports System.Runtime.CompilerServices

Namespace CNN

    ''' <summary>
    ''' The "layer specification": a deferred description of a layer that has not been created yet.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This type exists to support the fluent network building style: the factory functions in
    ''' <see cref="CNNLayers"/> produce specification objects and the <see cref="LayerBuilder"/> <c>+</c> operator
    ''' instantiates and appends them at the right time:
    ''' </para>
    ''' <code>
    ''' Dim cnn As LayerBuilder = New LayerBuilder() +
    '''     input_layer({28, 28}, 1) +
    '''     conv_layer(5, 32, 1, 2) +
    '''     pool_layer(2, 2, 0) +
    '''     relu_layer() +
    '''     full_connected_layer(10) +
    '''     softmax_layer()
    ''' </code>
    ''' <para>
    ''' Deferred creation is necessary because a layer needs the <see cref="data.OutputDefinition"/> of the previous
    ''' layer at construction time (to derive its own input size), and that object is internal state of the
    ''' <see cref="LayerBuilder"/> which cannot be referenced before the chained expression has been evaluated.
    ''' </para>
    ''' <para>
    ''' Unlike the same named type in the R# MLkit
    ''' (<c>studio\Rsharp_kit\MLkit\MachineLearning\CNNLayerArguments.vb</c>), which carries parameters through a
    ''' <c>type</c> string and an <c>args</c> dictionary for weakly typed scripting, this VB version uses a delegate
    ''' and is therefore type safe: the parameters are captured at compile time and there is no runtime
    ''' "unknown layer type" branch.
    ''' </para>
    ''' </remarks>
    Public Class CNNLayerArguments

        ''' <summary>
        ''' A readable description of the specification, for example
        ''' <c>conv_layer(sx=5, filters=32, stride=1, padding=2)</c>.
        ''' </summary>
        Public ReadOnly Property Name As String

        ''' <summary>The action that actually instantiates the layer and appends it to the builder.</summary>
        Friend ReadOnly factory As Func(Of LayerBuilder, LayerBuilder)

        Friend Sub New(name As String, factory As Func(Of LayerBuilder, LayerBuilder))
            Me.Name = name
            Me.factory = factory
        End Sub

        ''' <summary>
        ''' Creates the layer described by this specification and appends it to the given builder.
        ''' </summary>
        ''' <param name="builder">The network builder being constructed.</param>
        ''' <returns>The same builder, so the fluent chain can continue.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateLayer(builder As LayerBuilder) As LayerBuilder
            Return factory(builder)
        End Function

        ''' <summary>Returns the human readable specification name.</summary>
        ''' <returns>The <see cref="Name"/> of this specification.</returns>
        Public Overrides Function ToString() As String
            Return $"{Name}"
        End Function

    End Class
End Namespace
