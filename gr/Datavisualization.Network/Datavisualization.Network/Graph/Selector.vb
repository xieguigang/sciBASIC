Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework

Namespace Graph

    ''' <summary>
    ''' Graph value selector by property name
    ''' </summary>
    Public Module Selector

        Public Interface IGraphValueContainer(Of T As GraphData)
            Property Data As T
        End Interface

        ''' <summary>
        ''' Create a node value selector from a property name
        ''' </summary>
        ''' <param name="property$"></param>
        ''' <returns></returns>
        <Extension> Public Function SelectNodeValue(property$) As Func(Of Node, Object)
            Return [property].GenericSelector(Of NodeData, Node)
        End Function

        <Extension>
        Public Function GenericSelector(Of T As GraphData, Graph As IGraphValueContainer(Of T))(property$) As Func(Of Graph, Object)
            Dim graphValue = GetType(Graph).Schema(PropertyAccess.Readable,, True)
            Dim reader As PropertyInfo
            Dim dataValues = GetType(T).Schema(PropertyAccess.Readable,, True)

            If graphValue.ContainsKey([property]) Then
                reader = graphValue([property])
                Return Function(model) reader.GetValue(model)
            ElseIf dataValues.ContainsKey([property]) Then
                reader = dataValues([property])
                Return Function(model) reader.GetValue(model.Data)
            Else
                Return Function(model) model.Data([property])
            End If
        End Function

        <Extension> Public Function SelectEdgeValue(property$) As Func(Of Edge, Object)
            Return [property].GenericSelector(Of EdgeData, Edge)
        End Function
    End Module
End Namespace