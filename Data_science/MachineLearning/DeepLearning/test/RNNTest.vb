#Region "Microsoft.VisualBasic::8a1a89051504b76c3e804d791826e7e9, Data_science\MachineLearning\DeepLearning\test\RNNTest.vb"

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

    '   Total Lines: 14
    '    Code Lines: 9 (64.29%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (35.71%)
    '     File Size: 377 B


    ' Module RNNTest
    ' 
    '     Sub: Main2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RNN

Module RNNTest

    Sub Main2()
        Dim opts As New Options With {.inputFile = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\MachineLearning\DeepLearning\RNN\input.txt"}
        Dim net = CharRNN.initialize(opts)

        Call CharRNN.train(opts, net, "./aaa.rnn")


        Pause()
    End Sub
End Module
