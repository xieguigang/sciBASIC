#Region "Microsoft.VisualBasic::dfbe095a80503fa2abc15aec9a86d6b3, Data_science\Visualization\Visualization\Testing\ANNVisualize.vb"

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

    '   Total Lines: 91
    '    Code Lines: 69 (75.82%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (24.18%)
    '     File Size: 3.27 KB


    ' Module ANNVisualize
    ' 
    '     Sub: Main, test2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.Activations
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Module ANNVisualize

    Sub Main()

        Call test2()

        Dim ANN As New TrainingUtils(5, {10, 13, 50}, 3)

        Call ANN.Add({1, 1, 1, 1, 1}, {1, 1, 1})
        Call ANN.Add({1, 1, 1, 1, 1}, {1, 1, 1})
        Call ANN.Add({1, 1, 1, 1, 1}, {1, 1, 1})
        Call ANN.Add({1, 1, 1, 1, 1}, {1, 1, 1})
        Call ANN.Add({1, 1, 1, 1, 1}, {1, 1, 1})
        Call ANN.Add({1, 1, 1, 1, 1}, {1, 1, 1})
        Call ANN.Add({1, 1, 1, 1, 1}, {1, 1, 1})
        Call ANN.Add({1, 1, 1, 1, 1}, {1, 1, 1})
        Call ANN.Add({0, 1, 1, 1, 1}, {0, 0, 0})
        Call ANN.Add({0, 1, 1, 1, 1}, {0, 0, 0})
        Call ANN.Add({0, 1, 1, 1, 0.2}, {0, 0, 0})
        Call ANN.Add({0, 1, 1, 1, 1}, {0, 0, 0})
        Call ANN.Add({0, 1, 1, 1, 1}, {0, 0, 0})
        Call ANN.Add({0, 1, 0.5, 1, 1}, {0, 0, 0})
        Call ANN.Add({0, 1, 0.4, 1, 1}, {0, 0, 0})
        Call ANN.Add({1, 1, 1, 0, 1}, {1, 0, 1})
        Call ANN.Add({1, 1, 1, 0, 1}, {1, 0, 1})
        Call ANN.Add({1, 1, 1, 0, 1}, {1, 0, 1})
        Call ANN.Add({1, 0, 1, 0, 1}, {1, 0.3, 1})
        Call ANN.Add({1, 1, 1, 0, 1}, {1, 0, 1})
        Call ANN.Add({1, 0, 1, 0, 1}, {1, 0.3, 1})
        Call ANN.Add({1, 1, 1, 0, 1}, {1, 0, 1})
        Call ANN.Add({1, 1, 1, 0, 0}, {1, 0, 1})
        Call ANN.Add({1, 1, 0, 0, 1}, {1, 0, 1})


        Call ANN.SetLayerNormalize(True)

        Call ANN.Train()

        Call ANN.NeuronNetwork.Compute(1, 1, 1, 1, 1).GetJson.__DEBUG_ECHO
        Call ANN.NeuronNetwork.Compute(0, 1, 1, 1, 0).GetJson.__DEBUG_ECHO
        Call ANN.NeuronNetwork.Compute(1, 0, 1, 0, 1).GetJson.__DEBUG_ECHO


        Call ANN.TakeSnapshot.GetXml.SaveTo("./ANN_snapshot.Xml")
        Call ANN.NeuronNetwork.VisualizeModel(0.98).Save("./ANN_network/")

        Pause()
    End Sub

    Sub test2()
        Dim activations As New LayerActives With {
            .input = New SigmoidFunction,
            .output = New BipolarSigmoid,
            .hiddens = New Sigmoid
        }
        Dim ANN As New TrainingUtils(5, {10, 13, 60, 30, 6}, 3, momentum:=0.9, active:=activations)

        For i As Integer = 0 To 6
            Call ANN.Add(rand(5, {85, 125}), {1, 1, 1})
        Next

        For i As Integer = 0 To 8
            Call ANN.Add(rand(4, {85, 130}).AsList + 0, {0, 0, 0})
        Next

        For i As Integer = 0 To 10
            Dim v = rand(5, {90, 120})
            v(3) = Rnd()

            Call ANN.Add(v, {1, 0, 1})
        Next

        Call ANN.Train(parallel:=False)

        Call ANN.NeuronNetwork.Compute(100, 109, 110, 89, 93).GetJson.__DEBUG_ECHO
        Call ANN.NeuronNetwork.Compute(130, 75, 98, 89, 0).GetJson.__DEBUG_ECHO
        Call ANN.NeuronNetwork.Compute(100, 10, 93, -8, 122).GetJson.__DEBUG_ECHO


        Call ANN.TakeSnapshot.GetXml.SaveTo("./ANN_snapshot.Xml")
        Call ANN.NeuronNetwork.VisualizeModel.Save("./ANN_network/")

        Pause()
    End Sub
End Module
