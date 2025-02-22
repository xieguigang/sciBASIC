#Region "Microsoft.VisualBasic::d2ea574ad3073fb176f72036be9f6149, Data_science\MachineLearning\MachineLearning\test\simpleANNtest.vb"

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

    '   Total Lines: 85
    '    Code Lines: 68 (80.00%)
    ' Comment Lines: 1 (1.18%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (18.82%)
    '     File Size: 5.60 KB


    ' Module simpleANNtest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

Module simpleANNtest

    Sub Main()
        Dim samples As New List(Of Sample)
        Dim id As i32 = 1

        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}
        samples += New Sample({0, 0, 0, 0, 0, 1}) With {.ID = ++id, .target = {1, 1, 1, 1, 1}}

        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0.6}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 1, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 1, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 0, 1, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0.1, 0.61}}

        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0.68}}
        samples += New Sample({0, 1, 0, 0, 0, 0.6}) With {.ID = ++id, .target = {1, 0, 0, 0, 1}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 1}}
        samples += New Sample({0, 1, 0, 1, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 1}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 1}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0.96}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0.87}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0.99}}
        samples += New Sample({0, 1, 1, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0.87}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 1}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 1}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 1}}
        samples += New Sample({0, 1, 1, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0.88}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0, 0.9}}
        samples += New Sample({0, 1, 0, 0, 0, 0}) With {.ID = ++id, .target = {1, 0, 0, 0.1, 1}}

        Call New SampleList With {.items = samples}.CreateDataSet.GetXml.SaveTo("D:\GCModeller\src\runtime\sciBASIC#\Data_science\MachineLearning\MachineLearning\NeuralNetwork\Demo_data.Xml")


        Pause()

        'Dim trainer As New TrainingUtils(6, {100, 300, 30}, 5, momentum:=0.9)

        'Helpers.MaxEpochs = 5000

        '' trainer.SetDropOut(0.8)

        'Call samples.DoEach(Sub(dset) trainer.Add(dset))
        'Call trainer.Train(parallel:=True)

        'trainer.SetDropOut(0)

        'Dim predict1 = trainer.NeuronNetwork.Compute(0, 0, 0, 0, 0, 1)
        'Dim predict2 = trainer.NeuronNetwork.Compute(0.8, 0.002, 0, 0, 0, 0.0008)

        'Call Console.WriteLine(predict1.GetJson)
        'Call Console.WriteLine(predict2.GetJson)

        'Pause()
    End Sub
End Module
