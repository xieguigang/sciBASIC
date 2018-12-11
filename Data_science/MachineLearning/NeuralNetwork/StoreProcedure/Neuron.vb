Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace NeuralNetwork.StoreProcedure

    Public Class Synapse

        <XmlAttribute> Public Property InputNeuron As String
        <XmlAttribute> Public Property OutputNeuron As String
        ''' <summary>
        ''' 两个神经元之间的连接强度
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property weight As Double
        <XmlAttribute> Public Property weightDelta As Double

    End Class

    ''' <summary>
    ''' 一个神经元节点的数据模型
    ''' </summary>
    Public Class NeuronNode : Implements INamedValue

        ''' <summary>
        ''' 当前的这个神经元的唯一标记
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property ID As String Implements IKeyedEntity(Of String).Key
        <XmlAttribute> Public Property bias As Double
        <XmlAttribute> Public Property biasDelta As Double
        <XmlAttribute> Public Property gradient As Double
        <XmlAttribute> Public Property value As Double

    End Class
End Namespace