#Region "Microsoft.VisualBasic::8a10d75058ff95df676c911fc67f969f, Data_science\MachineLearning\DeepLearning\NeuralNetwork\StoreProcedure\Models\Layers.vb"

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

    '   Total Lines: 44
    '    Code Lines: 32
    ' Comment Lines: 3
    '   Blank Lines: 9
    '     File Size: 1.54 KB


    '     Class NeuronLayer
    ' 
    '         Properties: activation, id, neurons
    ' 
    '         Function: getCollection, getSize
    ' 
    '     Class HiddenLayer
    ' 
    '         Properties: activation, layers
    ' 
    '         Function: getCollection, getSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.Activations
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' Layer对象之中只放置神经元节点的引用唯一编号
    ''' </summary>
    <XmlType("layer")> Public Class NeuronLayer : Inherits ListOf(Of String)
        Implements INamedValue

        <XmlAttribute>
        Public Property id As String Implements IKeyedEntity(Of String).Key
        Public Property activation As ActiveFunction
        <XmlElement("neuron")>
        Public Property neurons As String()

        Protected Overrides Function getSize() As Integer
            Return neurons?.Length
        End Function

        Protected Overrides Function getCollection() As IEnumerable(Of String)
            Return neurons
        End Function
    End Class

    Public Class HiddenLayer : Inherits ListOf(Of NeuronLayer)

        Public Property activation As ActiveFunction
        <XmlElement("layers")>
        Public Property layers As NeuronLayer()

        Protected Overrides Function getSize() As Integer
            Return layers.Length
        End Function

        Protected Overrides Function getCollection() As IEnumerable(Of NeuronLayer)
            Return layers
        End Function
    End Class
End Namespace
