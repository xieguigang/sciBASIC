Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.factory
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.utils

Namespace nn.rbm.deep

    ''' <summary>
    ''' Created by kenny on 5/16/14.
    ''' 
    ''' A deep RBM can be configured in the standard way (one RBM per layer)
    '''  h_2     (o   o   o)
    '''            [X   X]
    '''  h_1     (o   o   o)
    '''            [X   X]
    '''  v       (o   o   o)
    ''' 
    ''' Or it can be configured as a Shape RBM (multiple RBMs per layer)
    '''  h_2     (o   o   o   o)
    '''            [X   X   X]
    '''  h_1     (o   o) (o   o)
    '''            [X   X   X]
    '''  v       (o) (o) (o) (o)
    ''' </summary>
    Public Class DeepRBM

        Private ReadOnly rbmLayersField As RBMLayer()

        Private ReadOnly visibleSizeField As Integer

        Private ReadOnly hiddenSizeField As Integer

        Public Sub New(layerParameters As LayerParameters(), rbmFactory As RBMFactory)
            checkLayerParameters(layerParameters)

            rbmLayersField = New RBMLayer(layerParameters.Length - 1) {}

            visibleSizeField = layerParameters(0).VisibleUnitsPerRBM * layerParameters(0).NumRBMS
            hiddenSizeField = layerParameters(layerParameters.Length - 1).HiddenUnitsPerRBM * layerParameters(layerParameters.Length - 1).NumRBMS

            For layer = 0 To layerParameters.Length - 1
                Dim layerParameter = layerParameters(layer)
                Dim rbms = New RBM(layerParameter.NumRBMS - 1) {}

                For i = 0 To layerParameter.NumRBMS - 1
                    rbms(i) = rbmFactory.build(layerParameter.VisibleUnitsPerRBM, layerParameter.HiddenUnitsPerRBM)
                Next
                rbmLayersField(layer) = New RBMLayer(rbms)
            Next
        End Sub

        Public Shared Sub checkLayerParameters(layerParameters As LayerParameters())

            'int visibleNodesIn = layerParameters[0].getNumRBMS() * layerParameters[0].getVisibleUnitsPerRBM();
            Dim hiddenNodesOut = layerParameters(0).NumRBMS * layerParameters(0).HiddenUnitsPerRBM
            For layer = 1 To layerParameters.Length - 1
                Dim currentLayerVisibleNodesIn = layerParameters(layer).NumRBMS * layerParameters(layer).VisibleUnitsPerRBM
                Dim currentLayerHiddenNodesIn = layerParameters(layer).NumRBMS * layerParameters(layer).HiddenUnitsPerRBM
                If hiddenNodesOut <> currentLayerVisibleNodesIn Then
                    Throw New ArgumentException("Layer: " & layer.ToString() & ", previous layer hidden nodes out (" & hiddenNodesOut.ToString() & ") does not match current layer visible layers in (" & currentLayerVisibleNodesIn.ToString() & ").")
                End If

                'visibleNodesIn = currentLayerVisibleNodesIn;
                hiddenNodesOut = currentLayerHiddenNodesIn
            Next

        End Sub

        Public Sub New(rbmLayers As RBMLayer())
            rbmLayersField = rbmLayers
            visibleSizeField = rbmLayers(0).size() * rbmLayers(0).getRBM(0).VisibleSize
            hiddenSizeField = rbmLayers(rbmLayers.Length - 1).size() * rbmLayers(rbmLayers.Length - 1).getRBM(0).VisibleSize
        End Sub

        Public Overridable ReadOnly Property RbmLayers As RBMLayer()
            Get
                Return rbmLayersField
            End Get
        End Property

        Public Overridable ReadOnly Property VisibleSize As Integer
            Get
                Return visibleSizeField
            End Get
        End Property

        Public Overridable ReadOnly Property HiddenSize As Integer
            Get
                Return hiddenSizeField
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return "DeepRBM{" & "rbmLayers=" & PrettyPrint.ToString(rbmLayersField) & "}"c.ToString()
        End Function
    End Class

End Namespace
