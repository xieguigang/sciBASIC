#Region "Microsoft.VisualBasic::46435599aba1c11cb026403238fe1026, ..\visualbasic_App\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\Helpers.vb"

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

Imports System.Runtime.CompilerServices

Namespace NeuralNetwork

    Public Module Helpers

        Public Const MaxEpochs As Integer = 5000
        Public Const MinimumError As Double = 0.01

#Region "-- Globals --"
        Private ReadOnly Random As New Random()
#End Region

#Region "-- Helpers --"

        Public Function GetRandom() As Double
            Return 2 * Random.NextDouble() - 1
        End Function
#End Region

        <Extension>
        Public Sub Train(ByRef neuron As Network, data As List(Of DataSet), Optional trainingType As TrainingType = TrainingType.Epoch)
            If trainingType = TrainingType.Epoch Then
                Call neuron.Train(data, Helpers.MaxEpochs)
            Else
                Call neuron.Train(data, Helpers.MinimumError)
            End If
        End Sub
    End Module

    Public Enum TrainingType
        ''' <summary>
        ''' <see cref="Helpers.MaxEpochs"/>
        ''' </summary>
        Epoch
        ''' <summary>
        ''' <see cref="Helpers.MinimumError"/>
        ''' </summary>
        MinimumError
    End Enum

    Public Class Encoder(Of T)

        Dim maps As New Dictionary(Of T, Double)

        Default Public Property item(x As T) As Double
            Get
                If maps.ContainsKey(x) Then
                    Return maps(x)
                End If
                Return Nothing
            End Get
            Set(value As Double)
                maps(x) = value
            End Set
        End Property

        Public Sub AddMap(x As T, value As Double)
            Call maps.Add(x, value)
        End Sub

        Public Function Encode(x As T) As Double
            Return maps(x)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="out">神经网络的输出值</param>
        ''' <returns></returns>
        Public Function Decode(out As Double) As T
            Dim minX As T, minD As Double = 9999

            For Each x In maps
                Dim d As Double = Math.Abs(x.Value - out)

                If d < minD Then
                    minD = d
                    minX = x.Key
                End If
            Next

            Return minX
        End Function
    End Class
End Namespace
