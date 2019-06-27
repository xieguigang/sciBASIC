#Region "Microsoft.VisualBasic::764bb4b55498d061560b2d276415672d, Data_science\MachineLearning\MachineLearning\test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: functionParserTest, Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning

Module Module1

    Sub Main()
        Call functionParserTest()
    End Sub

    Sub functionParserTest()

        Dim func As New NeuralNetwork.Activations.BipolarSigmoid(alpha:=3)
        Dim str = func.ToString
        Dim model = NeuralNetwork.StoreProcedure.ActiveFunction.Parse(str)
        Dim func2 As NeuralNetwork.Activations.BipolarSigmoid = model

        Pause()
    End Sub

End Module
