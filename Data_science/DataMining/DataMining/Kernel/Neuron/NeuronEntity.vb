#Region "Microsoft.VisualBasic::b4df267d306b0ff286bd36b263a108ac, Data_science\DataMining\DataMining\Kernel\Neuron\NeuronEntity.vb"

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

    '     Class NeuronEntity
    ' 
    '         Properties: Y
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.DataMining.ComponentModel

Namespace Kernel.Classifier

    Public Class NeuronEntity : Inherits EntityBase(Of Double)

        <XmlAttribute> Public Property Y As Double

        Public Overrides Function ToString() As String
            Dim sBuilder As New StringBuilder(1024)
            For Each p As Integer In entityVector
                Call sBuilder.AppendFormat("{0}, ", p)
            Next
            Call sBuilder.Remove(sBuilder.Length - 1, length:=1)

            Return String.Format("<{0}> --> {1}", sBuilder.ToString, Y)
        End Function

        Public Shared Widening Operator CType(properties As Double()) As NeuronEntity
            Return New NeuronEntity With {
                .entityVector = properties
            }
        End Operator

        Public Shared Widening Operator CType(properties As Integer()) As NeuronEntity
            Return New NeuronEntity With {
                .entityVector = (From n In properties Select CType(n, Double)).ToArray
            }
        End Operator
    End Class
End Namespace
