#Region "Microsoft.VisualBasic::1eade07a9679872238837e71e3fe6b6f, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\deep\DeepRBM.vb"

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

    '   Total Lines: 96
    '    Code Lines: 57
    ' Comment Lines: 19
    '   Blank Lines: 20
    '     File Size: 4.02 KB


    '     Class DeepRBM
    ' 
    '         Properties: HiddenSize, RbmLayers, VisibleSize
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: checkLayerParameters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

        Public ReadOnly Property RbmLayers As RBMLayer()
            Get
                Return rbmLayersField
            End Get
        End Property

        Public ReadOnly Property VisibleSize As Integer
            Get
                Return visibleSizeField
            End Get
        End Property

        Public ReadOnly Property HiddenSize As Integer
            Get
                Return hiddenSizeField
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return "DeepRBM{" & "rbmLayers=" & PrettyPrint.ToString(rbmLayersField) & "}"c.ToString()
        End Function
    End Class

End Namespace
