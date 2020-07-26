#Region "Microsoft.VisualBasic::849645e066598619360f3258def818c8, mime\application%json\Serializer\Deserializer.vb"

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

    ' Module Deserializer
    ' 
    '     Function: createArray, createObject, CreateObject, createVariant
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module Deserializer

    <Extension>
    Private Function createVariant(json As JsonObject, schema As Type) As Object
        Dim jsonVar As [Variant] = Activator.CreateInstance(schema)

        schema = jsonVar.which(json)
        jsonVar.jsonValue = json.createObject(schema)

        Return jsonVar
    End Function

    ''' <summary>
    ''' 进行反序列化
    ''' </summary>
    ''' <param name="json"></param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CreateObject(json As JsonElement, schema As Type) As Object
        If json Is Nothing Then
            Return Nothing
        ElseIf TypeOf json Is JsonArray Then
            If Not schema.IsArray Then
                Throw New InvalidCastException
            Else
                Return DirectCast(json, JsonArray).createArray(schema.GetElementType)
            End If
        ElseIf TypeOf json Is JsonObject Then
            If schema.IsInheritsFrom(GetType([Variant])) Then
                Return DirectCast(json, JsonObject).createVariant(schema)
            Else
                Return DirectCast(json, JsonObject).createObject(schema)
            End If
        ElseIf TypeOf json Is JsonValue Then
            Return DirectCast(json, JsonValue).Literal(schema)
        Else
            Throw New InvalidCastException
        End If
    End Function

    <Extension>
    Private Function createArray(json As JsonArray, elementType As Type) As Object
        Dim array As Array = Array.CreateInstance(elementType, json.Length)
        Dim obj As Object
        Dim element As JsonElement

        For i As Integer = 0 To array.Length - 1
            element = json(i)
            obj = element.CreateObject(elementType)
            array.SetValue(obj, i)
        Next

        Return array
    End Function

    ''' <summary>
    ''' 反序列化为目标类型的对象实例
    ''' </summary>
    ''' <param name="json"></param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    <Extension>
    Friend Function createObject(json As JsonObject, schema As Type) As Object
        Dim obj As Object = Activator.CreateInstance(schema)
        Dim inputs As Object()
        Dim graph As ObjectSchema = ObjectSchema.GetSchema(schema)
        Dim addMethod As MethodInfo = graph.addMethod
        Dim writers = graph.writers
        Dim writer As PropertyInfo

        For Each [property] As NamedValue(Of JsonElement) In json
            If writers.ContainsKey([property].Name) Then
                writer = writers([property].Name)
                writer.SetValue(obj, [property].Value.CreateObject(writer.PropertyType))
            ElseIf graph.isTable AndAlso Not addMethod Is Nothing Then
                inputs = {
                    [property].Name,
                    [property].Value.CreateObject(graph.valueType)
                }
                addMethod.Invoke(obj, inputs)
            Else
                ' 2020.2.5
                ' property出现在了json文件之中
                ' 但是在反序列化的目标对象类型之中却不存在
                ' 应该是有选择性的对目标做反序列化加载还是在编写class的时候漏掉了当前的property？
                ' 则给出警告信息
                Call $"Missing property '{[property]}' in {graph}".Warning
            End If
        Next

        Return obj
    End Function
End Module
