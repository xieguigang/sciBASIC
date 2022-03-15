#Region "Microsoft.VisualBasic::fc18ec63d6f7159a0e7a591ebcb90eb7, sciBASIC#\mime\application%json\Serializer\Deserializer.vb"

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

    '   Total Lines: 150
    '    Code Lines: 111
    ' Comment Lines: 20
    '   Blank Lines: 19
    '     File Size: 6.02 KB


    ' Module Deserializer
    ' 
    '     Function: activate, createArray, createObject, (+2 Overloads) CreateObject, createVariant
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports any = Microsoft.VisualBasic.Scripting

Public Module Deserializer

    <Extension>
    Private Function createVariant(json As JsonObject, parent As ObjectSchema, schema As Type) As Object
        Dim jsonVar As [Variant] = Activator.CreateInstance(schema)

        schema = jsonVar.which(json)
        jsonVar.jsonValue = json.createObject(parent, schema)

        Return jsonVar
    End Function

    ''' <summary>
    ''' 进行反序列化
    ''' </summary>
    ''' <param name="json"></param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CreateObject(json As JsonElement, schema As Type) As Object
        Return json.CreateObject(Nothing, schema)
    End Function

    <Extension>
    Private Function CreateObject(json As JsonElement, parent As ObjectSchema, schema As Type) As Object
        If json Is Nothing Then
            Return Nothing
        ElseIf TypeOf json Is JsonArray Then
            If Not schema.IsArray Then
                ' the schema require an object but gives an array
                Return Nothing
            Else
                Return DirectCast(json, JsonArray).createArray(parent, schema.GetElementType)
            End If
        ElseIf TypeOf json Is JsonObject Then
            If schema.IsInheritsFrom(GetType([Variant])) Then
                Return DirectCast(json, JsonObject).createVariant(parent, schema)
            ElseIf Not schema.IsArray AndAlso Not schema.IsPrimitive AndAlso Not schema.IsEnum Then
                Return DirectCast(json, JsonObject).createObject(parent, schema)
            Else
                ' the schema require an array but given an object
                Return Nothing
            End If
        ElseIf TypeOf json Is JsonValue Then
            Return DirectCast(json, JsonValue).Literal(schema)
        Else
            Throw New InvalidCastException
        End If
    End Function

    <Extension>
    Friend Function createArray(json As JsonArray, parent As ObjectSchema, elementType As Type) As Object
        Dim array As Array = Array.CreateInstance(elementType, json.Length)
        Dim obj As Object
        Dim element As JsonElement

        For i As Integer = 0 To array.Length - 1
            element = json(i)
            obj = element.CreateObject(parent, elementType)
            array.SetValue(obj, i)
        Next

        Return array
    End Function

    <Extension>
    Private Function activate(ByRef schema As ObjectSchema, parent As ObjectSchema, score As JsonObject) As Object
        Dim knownType As ObjectSchema

        If Not schema.raw.IsInterface AndAlso Not schema.raw Is GetType(Object) Then
            Return Activator.CreateInstance(schema.raw)
        ElseIf schema.raw.IsInterface Then
            knownType = parent _
                .FindInterfaceImpementations(schema.raw) _
                .OrderByDescending(Function(a) a.Score(score)) _
                .FirstOrDefault

            If knownType Is Nothing Then
                Throw New InvalidProgramException($"can not create object from an interface type: {schema.raw.FullName}!")
            End If
        Else ' is object
            knownType = parent _
                .knownTypes _
                .Select(AddressOf ObjectSchema.GetSchema) _
                .OrderByDescending(Function(a) a.Score(score)) _
                .FirstOrDefault

            If knownType Is Nothing Then
                Throw New InvalidProgramException($"can not create object...")
            End If
        End If

        schema = knownType

        Return Activator.CreateInstance(knownType.raw)
    End Function

    ''' <summary>
    ''' 反序列化为目标类型的对象实例
    ''' </summary>
    ''' <param name="json"></param>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    <Extension>
    Friend Function createObject(json As JsonObject, parent As ObjectSchema, schema As Type) As Object
        Dim graph As ObjectSchema = ObjectSchema.GetSchema(schema)
        Dim obj As Object = graph.activate(parent:=parent, score:=json)
        Dim inputs As Object()
        Dim addMethod As MethodInfo = graph.addMethod
        Dim writers As IReadOnlyDictionary(Of String, PropertyInfo) = graph.writers
        Dim writer As PropertyInfo
        Dim innerVal As Object

        For Each [property] As NamedValue(Of JsonElement) In json
            If writers.ContainsKey([property].Name) Then
                writer = writers([property].Name)

                If writer.CanWrite Then
                    innerVal = [property].Value.CreateObject(parent:=graph, writer.PropertyType)
                    writer.SetValue(obj, innerVal)
                End If
            ElseIf graph.isTable AndAlso Not addMethod Is Nothing Then
                innerVal = [property].Value.CreateObject(parent:=graph, graph.valueType)
                inputs = {
                    any.CTypeDynamic([property].Name, graph.keyType),
                    innerVal
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
