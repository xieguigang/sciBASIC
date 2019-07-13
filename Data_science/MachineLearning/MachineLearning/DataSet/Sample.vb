#Region "Microsoft.VisualBasic::8d63659f1e975f7aa699eb1d4f456028, Data_science\MachineLearning\MachineLearning\DataSet\Sample.vb"

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

    '     Class Sample
    ' 
    '         Properties: ID, status, target
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace StoreProcedure

    ''' <summary>
    ''' The training dataset
    ''' </summary>
    Public Class Sample : Implements INamedValue

        ''' <summary>
        ''' 可选的数据集唯一标记信息
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("id")>
        Public Property ID As String Implements IKeyedEntity(Of String).Key

        ''' <summary>
        ''' Neuron network input parameters
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 属性值可能会很长,为了XML文件的美观,在这里使用element
        ''' </remarks>
        <XmlElement>
        Public Property status As NumericVector

        ''' <summary>
        ''' The network expected output values
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property target As Double()

        ''' <summary>
        ''' Create a new training dataset
        ''' </summary>
        ''' <param name="values">Neuron network input parameters</param>
        ''' <param name="targets">The network expected output values</param>
        Public Sub New(values#(), targets#(), Optional inputName$ = Nothing)
            Me.status = New NumericVector With {
                .name = inputName,
                .vector = values
            }
            Me.target = targets
        End Sub

        ''' <summary>
        ''' Create a new empty training dataset
        ''' </summary>
        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{status.vector.AsVector.ToString} => {target.AsVector.ToString}"
        End Function
    End Class
End Namespace
