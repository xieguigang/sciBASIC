#Region "Microsoft.VisualBasic::b23470dffe721a987b0be5bc805b3fc5, Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\Dataset.vb"

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

    '     Class DataSet
    ' 
    '         Properties: Targets, Values
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace NeuralNetwork

    ''' <summary>
    ''' The training dataset
    ''' </summary>
    Public Class DataSet

#Region "-- Properties --"

        ''' <summary>
        ''' Neuron network input parameters
        ''' </summary>
        ''' <returns></returns>
        Public Property Values() As Double()
        ''' <summary>
        ''' The network expected output values
        ''' </summary>
        ''' <returns></returns>
        Public Property Targets() As Double()
#End Region

#Region "-- Constructor --"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="values__1">Neuron network input parameters</param>
        ''' <param name="targets__2">The network expected output values</param>
        Public Sub New(values__1 As Double(), targets__2 As Double())
            Values = values__1
            Targets = targets__2
        End Sub

        Sub New()
        End Sub
#End Region

        Public Overrides Function ToString() As String
            Return Values.Join(Targets).GetJson
        End Function
    End Class
End Namespace
