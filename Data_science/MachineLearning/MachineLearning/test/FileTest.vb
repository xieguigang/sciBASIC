Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.Serialization.JSON

Module FileTest
    Sub Main()

        Dim samples As New List(Of Sample)
        Dim id As VBInteger = 0

        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 0, 1, 1}}, .target = {0, 0, 0, 0}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}
        samples += New Sample With {.ID = ++id, .status = New NumericVector With {.vector = {1, 1, 1, 1, 1}}, .target = {1, 1, 1, 1}}

        Dim trainer As New TrainingUtils(5, {10, 100, 30, 50}, 4)

        Call samples.DoEach(Sub(dset) trainer.Add(dset))
        Call trainer.Train()

        Call trainer.TakeSnapshot.GetXml.SaveTo("./format1.Xml")
        Call trainer.TakeSnapshot.GetJson.SaveTo("./format2.json")

        Call trainer.TakeSnapshot.ScatteredStore("./scatters/")
    End Sub
End Module
