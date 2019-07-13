Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
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