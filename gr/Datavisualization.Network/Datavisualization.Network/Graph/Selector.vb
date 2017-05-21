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
        <Extension> Public Function SelectNodeValue(property$, Optional ByRef type As Type = Nothing) As Func(Of Node, Object)
            Return [property].GenericSelector(Of NodeData, Node)(type)
        End Function

        ''' <summary>
        ''' 所映射的属性的类型
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="Graph"></typeparam>
        ''' <param name="property$"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GenericSelector(Of T As GraphData, Graph As IGraphValueContainer(Of T))(property$, ByRef type As Type) As Func(Of Graph, Object)
            Dim graphValue = GetType(Graph).Schema(PropertyAccess.Readable,, True)
            Dim reader As PropertyInfo
            Dim dataValues = GetType(T).Schema(PropertyAccess.Readable,, True)

            If graphValue.ContainsKey([property]) Then
                reader = graphValue([property])
                type = reader.PropertyType
                Return Function(model) reader.GetValue(model)
            ElseIf dataValues.ContainsKey([property]) Then
                reader = dataValues([property])
                type = reader.PropertyType
                Return Function(model) reader.GetValue(model.Data)
            Else
                type = GetType(String)
                Return Function(model) model.Data([property])
            End If
        End Function

        <Extension> Public Function SelectEdgeValue(property$, Optional ByRef type As Type = Nothing) As Func(Of Edge, Object)
            Return [property].GenericSelector(Of EdgeData, Edge)(type)
        End Function
    End Module
End Namespace