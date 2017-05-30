#Region "Microsoft.VisualBasic::6fc60166566e0f2c309d81c6da30716f, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\TrainingUtils.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Language

Namespace NeuralNetwork

    ''' <summary>
    ''' Tools for training the neuron network
    ''' </summary>
    Public Class TrainingUtils

        Public Property TrainingType As TrainingType = TrainingType.Epoch
        Public ReadOnly Property NeuronNetwork As Network

        ReadOnly _dataSets As New List(Of DataSet)

        Public ReadOnly Property XP As Integer
            Get
                Return _dataSets.Count
            End Get
        End Property

        Public Sub Encouraging()
            Call Train()
        End Sub

        Sub New(net As Network)
            NeuronNetwork = net
        End Sub

        Public Sub RemoveLast()
            If Not _dataSets.Count = 0 Then
                Call _dataSets.RemoveLast
            End If
        End Sub

        Public Sub Add(input As Double(), output As Double())
            Call _dataSets.Add(New DataSet(input, output))
        End Sub

        Public Sub Add(x As DataSet)
            Call _dataSets.Add(x)
#If DEBUG Then
            Call _dataSets.Count.__DEBUG_ECHO
#End If
        End Sub

        Public Sub Train()
            Call Helpers.Train(NeuronNetwork, _dataSets, TrainingType)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="input">The inputs data</param>
        ''' <param name="convertedResults">The error outputs</param>
        ''' <param name="expectedResults">The corrects output</param>
        Public Sub Corrects(input As Double(), convertedResults As Double(), expectedResults As Double(), Optional train As Boolean = True)
            Dim offendingDataSet As DataSet = _dataSets.FirstOrDefault(
                Function(x) x.Values.SequenceEqual(input) AndAlso x.Targets.SequenceEqual(convertedResults))
            _dataSets.Remove(offendingDataSet)

            If Not _dataSets.Exists(Function(x) x.Values.SequenceEqual(input) AndAlso x.Targets.SequenceEqual(expectedResults)) Then
                Call _dataSets.Add(New DataSet(input, expectedResults))
            End If

            If train Then
                Call Me.Train()
            End If
        End Sub

        Public Sub Corrects(dataset As DataSet, expectedResults As Double(), Optional train As Boolean = True)
            Call Corrects(dataset.Values, dataset.Targets, expectedResults, train)
        End Sub
    End Class
End Namespace
