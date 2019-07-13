#Region "Microsoft.VisualBasic::0d5cc16ad5564e05de8dbf9761783f7e, Data_science\MachineLearning\MachineLearning\test\simpleANNtest.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module simpleANNtest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Microsoft.VisualBasic.Serialization.JSON

Module simpleANNtest

    Sub Main()
        Dim samples As New List(Of Sample)
        Dim id As VBInteger = 1

        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 1, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 1, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 1, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 1}, .target = {1, 1, 1, 1, 1}}

        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0.6}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 1, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 1, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 1, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = {0, 0, 0, 0, 0, 0}, .target = {1, 0, 0, 0.1, 0.61}}

        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0.68}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0.6}, .target = {1, 0, 0, 0, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 1, 0, 0}, .target = {1, 0, 0, 0, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0.96}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0.87}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0.99}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 1, 0, 0, 0}, .target = {1, 0, 0, 0, 0.87}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 1}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 1, 0, 0, 0}, .target = {1, 0, 0, 0, 0.88}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0, 0.9}}
        samples += New Sample With {.ID = ++id, .status = {0, 1, 0, 0, 0, 0}, .target = {1, 0, 0, 0.1, 1}}

        Call New SampleList With {.items = samples}.CreateDataSet.GetXml.SaveTo("D:\GCModeller\src\runtime\sciBASIC#\Data_science\MachineLearning\MachineLearning\NeuralNetwork\Demo_data.Xml")


        Pause()

        Dim trainer As New TrainingUtils(6, {100, 300, 30}, 5, momentum:=0.9)

        Helpers.MaxEpochs = 5000

        ' trainer.SetDropOut(0.8)

        Call samples.DoEach(Sub(dset) trainer.Add(dset))
        Call trainer.Train(parallel:=True)

        trainer.SetDropOut(0)

        Dim predict1 = trainer.NeuronNetwork.Compute(0, 0, 0, 0, 0, 1)
        Dim predict2 = trainer.NeuronNetwork.Compute(0.8, 0.002, 0, 0, 0, 0.0008)

        Call Console.WriteLine(predict1.GetJson)
        Call Console.WriteLine(predict2.GetJson)

        Pause()
    End Sub
End Module
