#Region "Microsoft.VisualBasic::4aafca6db6798f2f0053ea779768773b, mime\application%json\Serializer\ObjectSerializer\Deserializer.vb"

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

    '   Total Lines: 183
    '    Code Lines: 129 (70.49%)
    ' Comment Lines: 36 (19.67%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 18 (9.84%)
    '     File Size: 7.88 KB


    ' Module Deserializer
    ' 
    '     Function: createArray, createObject, (+3 Overloads) CreateObject, createVariant
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports any = Microsoft.VisualBasic.Scripting

''' <summary>
''' create .NET clr object from json
''' </summary>
Public Module Deserializer

    <Extension>
    Private Function createVariant(json As JsonObject, parent As SoapGraph, schema As Type, decodeMetachar As Boolean) As Object
        Dim jsonVar As [Variant] = Activator.CreateInstance(schema)

        schema = jsonVar.which(json)
        jsonVar.jsonValue = json.createObject(parent, schema, decodeMetachar)

        Return jsonVar
    End Function

    ''' <summary>
    ''' 进行反序列化
    ''' </summary>
    ''' <param name="json"></param>
    ''' <param name="schema">add know types for object by using the <see cref="KnownTypeAttribute"/>.</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateObject(json As JsonElement, schema As Type, decodeMetachar As Boolean) As Object
        Return json.CreateObject(Nothing, schema, decodeMetachar)
    End Function

    ''' <summary>
    ''' Cast the json element data as specific clr type object
    ''' </summary>
    ''' <typeparam name="T">add know types for object by using the <see cref="KnownTypeAttribute"/>.</typeparam>
    ''' <param name="json"></param>
    ''' <param name="decodeMetachar"></param>
    ''' <returns>
    ''' this function will returns nothing if the input json element is nothing
    ''' </returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateObject(Of T As Class)(json As JsonElement, Optional decodeMetachar As Boolean = True) As T
        Return DirectCast(json.CreateObject(Nothing, GetType(T), decodeMetachar), T)
    End Function

    <Extension>
    Private Function CreateObject(json As JsonElement,
                                  parent As SoapGraph,
                                  schema As Type,
                                  decodeMetachar As Boolean) As Object
        If json Is Nothing Then
            Return Nothing
        ElseIf TypeOf json Is JsonArray Then
            If Not schema.IsArray Then
                ' the schema require an object but gives an array
                If schema Is GetType(Object) Then
                    ' property value type is object, could be cast to any
                    Dim anyArray As JsonArray = json

                    If anyArray.All(Function(e) TypeOf e Is JsonValue) Then
                        Return anyArray _
                            .Select(Function(a) DirectCast(a, JsonValue).Literal) _
                            .ToArray
                    Else
                        Return Nothing
                    End If
                Else
                    ' type mis-matched
                    Return Nothing
                End If
            Else
                Return DirectCast(json, JsonArray).createArray(parent, schema.GetElementType, decodeMetachar)
            End If
        ElseIf TypeOf json Is JsonObject Then
            If schema.IsInheritsFrom(GetType([Variant])) Then
                Return DirectCast(json, JsonObject).createVariant(parent, schema, decodeMetachar)
            ElseIf Not schema.IsArray AndAlso Not schema.IsPrimitive AndAlso Not schema.IsEnum Then
                Return DirectCast(json, JsonObject).CreateObject(parent, schema, decodeMetachar)
            Else
                ' the schema require an array but given an object
                Return Nothing
            End If
        ElseIf TypeOf json Is JsonValue Then
            If schema Is GetType(Object) Then
                Return DirectCast(json, JsonValue).Literal
            Else
                Return DirectCast(json, JsonValue).Literal(schema, decodeMetachar)
            End If
        Else
            Throw New InvalidCastException
        End If
    End Function

    <Extension>
    Friend Function createArray(json As JsonArray,
                                parent As SoapGraph,
                                elementType As Type,
                                decodeMetachar As Boolean) As Object

        Dim array As Array = Array.CreateInstance(elementType, json.Length)
        Dim obj As Object
        Dim element As JsonElement

        For i As Integer = 0 To array.Length - 1
            element = json(i)
            obj = element.CreateObject(parent, elementType, decodeMetachar)
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
    Friend Function createObject(json As JsonObject, parent As SoapGraph, schema As Type, decodeMetachar As Boolean) As Object
        Dim graph As SoapGraph = SoapGraph.GetSchema(schema, Serializations.JSON)
        Dim obj As Object = graph.Activate(parent:=parent, docs:=json.ObjectKeys, schema:=graph)
        Dim inputs As Object()
        Dim addMethod As MethodInfo = graph.addMethod
        Dim writers As IReadOnlyDictionary(Of String, PropertyInfo) = graph.writers
        Dim writer As PropertyInfo
        Dim innerVal As Object
        Dim metaObj2 As IDictionary = Nothing
        Dim metadata As PropertyInfo = DynamicMetadataAttribute.GetMetadata(obj.GetType)
        Dim metaVal As Type = Nothing

        If metadata IsNot Nothing Then
            metaObj2 = Activator.CreateInstance(metadata.PropertyType)
            metaVal = metadata.PropertyType.GetGenericArguments()(1)
            metadata.SetValue(obj, metaObj2)
        End If

        ' write property value at here
        For Each [property] As NamedValue(Of JsonElement) In json
            If [property].Name Is Nothing Then
                Continue For
            End If

            If writers.ContainsKey([property].Name) Then
                writer = writers([property].Name)

                If writer.CanWrite Then
                    innerVal = [property].Value.CreateObject(parent:=graph, writer.PropertyType, decodeMetachar)
                    writer.SetValue(obj, innerVal)
                End If
            ElseIf graph.isTable AndAlso Not addMethod Is Nothing Then
                innerVal = [property].Value.CreateObject(parent:=graph, graph.valueType, decodeMetachar)
                inputs = {
                    any.CTypeDynamic([property].Name, graph.keyType),
                    innerVal
                }
                addMethod.Invoke(obj, inputs)
            Else
                If metadata IsNot Nothing Then
                    ' write metadata
                    innerVal = [property].Value.CreateObject(parent:=graph, metaVal, decodeMetachar)
                    metaObj2.Add([property].Name, innerVal)
                Else
                    ' 2020.2.5
                    ' property出现在了json文件之中
                    ' 但是在反序列化的目标对象类型之中却不存在
                    ' 应该是有选择性的对目标做反序列化加载还是在编写class的时候漏掉了当前的property？
                    ' 则给出警告信息
                    Call $"Missing property '{[property]}' in {graph}".Warning
                End If
            End If
        Next

        Return obj
    End Function
End Module
