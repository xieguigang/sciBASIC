#Region "Microsoft.VisualBasic::67c92bbcb37fdfceb876fe78a811a6bc, Data_science\MachineLearning\MachineLearning\NeuralNetwork\StoreProcedure\Models\Neuron.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Synapse
    ' 
    '         Properties: [in], [out], delta, w
    ' 
    '         Function: ToString
    ' 
    '     Class NeuronNode
    ' 
    '         Properties: bias, delta, gradient, id
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace NeuralNetwork.StoreProcedure

    <XmlType("synapse")> Public Class Synapse

        <XmlAttribute> Public Property [in] As String
        <XmlAttribute> Public Property [out] As String
        ''' <summary>
        ''' 两个神经元之间的连接强度
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property w As Double
        <XmlAttribute> Public Property delta As Double

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"|{[in]} => {out}| = {w}"
        End Function
    End Class

    ''' <summary>
    ''' 一个神经元节点的数据模型
    ''' </summary>
    <XmlType("neuron")> Public Class NeuronNode : Implements INamedValue

        ''' <summary>
        ''' 当前的这个神经元的唯一标记
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property id As String Implements IKeyedEntity(Of String).Key
        <XmlAttribute> Public Property bias As Double
        <XmlAttribute> Public Property delta As Double
        <XmlAttribute> Public Property gradient As Double

        Public Overrides Function ToString() As String
            Return id
        End Function

    End Class
End Namespace
