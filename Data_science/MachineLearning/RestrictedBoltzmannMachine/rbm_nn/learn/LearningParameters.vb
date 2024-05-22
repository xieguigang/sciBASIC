#Region "Microsoft.VisualBasic::77c238d8503e1f82ce2ae83ff86af079, Data_science\MachineLearning\RestrictedBoltzmannMachine\rbm_nn\learn\LearningParameters.vb"

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

    '   Total Lines: 83
    '    Code Lines: 59 (71.08%)
    ' Comment Lines: 3 (3.61%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 21 (25.30%)
    '     File Size: 2.61 KB


    '     Class LearningParameters
    ' 
    '         Properties: Epochs, LearningRate, Log, LogisticsFunction, Memory
    ' 
    '         Function: setEpochs, setLearningRate, setLog, setLogisticsFunction, setMemory
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions

Namespace nn.rbm.learn

    ''' <summary>
    ''' Created by kenny on 5/15/14.
    ''' </summary>
    Public Class LearningParameters

        Private learningRateField As Double = 0.1

        Private logisticsFunctionField As DoubleFunction = New Sigmoid()

        Private epochsField As Integer = 15000

        Private logField As Boolean = True

        Private memoryField As Integer = 1

        Public ReadOnly Property LearningRate As Double
            Get
                Return learningRateField
            End Get
        End Property

        Public Function setLearningRate(learningRate As Double) As LearningParameters
            learningRateField = learningRate
            Return Me
        End Function

        Public ReadOnly Property LogisticsFunction As DoubleFunction
            Get
                Return logisticsFunctionField
            End Get
        End Property

        Public Function setLogisticsFunction(logisticsFunction As DoubleFunction) As LearningParameters
            logisticsFunctionField = logisticsFunction
            Return Me
        End Function

        Public ReadOnly Property Epochs As Integer
            Get
                Return epochsField
            End Get
        End Property

        Public Function setEpochs(epochs As Integer) As LearningParameters
            epochsField = epochs
            Return Me
        End Function


        Public ReadOnly Property Log As Boolean
            Get
                Return logField
            End Get
        End Property

        Public Function setLog(log As Boolean) As LearningParameters
            logField = log
            Return Me
        End Function

        Public ReadOnly Property Memory As Integer
            Get
                Return memoryField
            End Get
        End Property

        Public Function setMemory(memory As Integer) As LearningParameters
            memoryField = memory
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return "LearningParameters{" & "learningRate=" & learningRateField.ToString() & ", logisticsFunction=" & logisticsFunctionField.ToString() & ", epochs=" & epochsField.ToString() & ", log=" & logField.ToString() & ", memory=" & memoryField.ToString() & "}"c.ToString()
        End Function

    End Class

End Namespace
