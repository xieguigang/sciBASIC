Imports Microsoft.VisualBasic.MachineLearning.RNN

Module RNNTest

    Sub Main()
        Dim opts As New Options With {.inputFile = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\MachineLearning\DeepLearning\RNN\input.txt"}
        Dim net = CharRNN.initialize(opts)

        Call CharRNN.train(opts, net, "./aaa.rnn")


        Pause()
    End Sub
End Module
