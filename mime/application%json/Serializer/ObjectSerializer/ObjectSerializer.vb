#Region "Microsoft.VisualBasic::43762dcced4dff24e981eaef4b884424, mime\application%json\Serializer\ObjectSerializer\ObjectSerializer.vb"

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

    '   Total Lines: 234
    '    Code Lines: 177
    ' Comment Lines: 24
    '   Blank Lines: 33
    '     File Size: 9.04 KB


    ' Module ObjectSerializer
    ' 
    '     Function: GetJsonElement, populateArrayJson, populateObjectJson, populateTableJson
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.ValueTypes
Imports any = Microsoft.VisualBasic.Scripting

Public Module ObjectSerializer

    <Extension>
    Private Function populateArrayJson(schema As Type, obj As Object, opt As JSONSerializerOptions) As JsonElement
        Dim elementSchema As Type
        Dim populator As IEnumerable(Of JsonElement)

        If schema.IsArray Then
            elementSchema = schema.GetElementType
            populator = From element As Object
                        In DirectCast(obj, Array)
                        Select elementSchema.GetJsonElement(element, opt)
        Else
            ' list of type
            elementSchema = schema.GenericTypeArguments(Scan0)
            populator = From element As Object
                        In DirectCast(obj, IList)
                        Select elementSchema.GetJsonElement(element, opt)
        End If

        Return New JsonArray(populator)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <param name="obj"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Private Function populateObjectJson(schema As Type, obj As Object, opt As JSONSerializerOptions) As JsonElement
        ' 会需要忽略掉有<ScriptIgnore>标记的属性
        Dim memberReaders = schema _
            .Schema(PropertyAccess.Readable, nonIndex:=True) _
            .Where(Function(p)
                       If opt.maskReadonly AndAlso Not p.Value.CanWrite Then
                           Return False
                       End If

                       Dim reader = p.Value
                       Dim ignores1 = reader.GetAttribute(Of ScriptIgnoreAttribute) Is Nothing
                       Dim ignores2 = reader.GetAttribute(Of DataIgnoredAttribute) Is Nothing
                       Dim ignores3 = reader.GetAttribute(Of IgnoreDataMemberAttribute) Is Nothing

                       Return ignores1 AndAlso ignores2 AndAlso ignores3
                   End Function) _
            .ToArray
        Dim [property] As PropertyInfo
        Dim valueType As Type
        Dim json As New JsonObject
        Dim valObj As Object
        Dim jsonVal As JsonElement
        Dim isNullable As Boolean = False

        If memberReaders.Any(Function(a) a.Value.Name = "HasValue") AndAlso
            memberReaders.Any(Function(a) a.Value.Name = "Value") Then

            isNullable = True
        End If

        If isNullable Then
            Dim elementType As Type = schema.GenericTypeArguments.FirstOrDefault

            obj = memberReaders _
                .First(Function(a) a.Value.Name = "Value").Value _
                .GetValue(obj, Nothing)
            schema = obj.GetType

            If Not elementType Is Nothing Then
                If DataFramework.IsPrimitive(elementType) Then
                    Return New JsonValue(obj)
                End If
            End If

            Return populateObjectJson(schema, obj, opt)
        End If

        Dim metadata As PropertyInfo = DynamicMetadataAttribute.GetMetadata(memberReaders.Select(Function(p) p.Value))

        For Each reader As KeyValuePair(Of String, PropertyInfo) In memberReaders
            [property] = reader.Value
            valueType = [property].PropertyType
            valObj = [property].GetValue(obj)

            If metadata IsNot Nothing AndAlso [property] Is metadata Then
                Dim js As IDictionary = TryCast(valObj, IDictionary)

                ' handling of the dynamics metadata property
                If Not js Is Nothing Then
                    For Each key As Object In js.Keys
                        Dim keystr As String = key.ToString
                        Dim value As Object = js(key)

                        If json.HasObjectKey(keystr) Then
                            keystr = "metadata:" & keystr
                        End If

                        If value Is Nothing Then
                            Call json.Add(keystr, JsonValue.NULL)
                        Else
                            Call json.Add(keystr, value.GetType.GetJsonElement(value, opt))
                        End If
                    Next
                End If
            Else
                If valueType.IsInterface OrElse
                    valueType Is GetType(Object) OrElse
                    valueType.IsAbstract Then

                    If valObj Is Nothing Then
                        valueType = GetType(Object)
                    Else
                        valueType = valObj.GetType
                    End If
                End If

                If valObj Is Nothing Then
                    jsonVal = JsonValue.NULL
                ElseIf Not (valueType Is GetType(Type) OrElse
                    valueType Is GetType(TypeInfo) OrElse
                    valueType.FullName = "System.RuntimeType" OrElse
                    valueType.FullName = "System.Reflection.RuntimeAssembly") Then

                    jsonVal = valueType.GetJsonElement(valObj, opt)
                Else
                    jsonVal = Nothing
                End If

                If Not jsonVal Is Nothing Then
                    Call json.Add(reader.Key, jsonVal)
                End If
            End If
        Next

        Return json
    End Function

    ''' <summary>
    ''' 所有的字典键都会被强制转换为字符串类型
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="valueSchema"></param>
    ''' <returns></returns>
    <Extension>
    Private Function populateTableJson(obj As IDictionary, valueSchema As Type, opt As JSONSerializerOptions) As JsonElement
        Dim json As New JsonObject
        Dim key As String
        Dim value As Object

        For Each memberKey As Object In obj.Keys
            key = any.ToString(memberKey)
            value = obj.Item(memberKey)

            If value Is Nothing Then
                Call json.Add(key, New JsonValue())
            ElseIf valueSchema Is GetType(Object) Then
                Call json.Add(key, value.GetType.GetJsonElement(value, opt))
            Else
                Call json.Add(key, valueSchema.GetJsonElement(value, opt))
            End If
        Next

        Return json
    End Function

    ''' <summary>
    ''' Convert any .NET CLR object as json element model for build json string or bson data
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <param name="obj"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetJsonElement(schema As Type, obj As Object, opt As JSONSerializerOptions) As JsonElement
        If obj Is Nothing Then
            Return Nothing
        ElseIf schema.IsAbstract OrElse
            schema Is GetType(Object) AndAlso
            obj IsNot Nothing Then

            schema = obj.GetType
        End If

        If schema.IsArray OrElse schema.IsInheritsFrom(GetType(List(Of )), strict:=False) Then
            Return schema.populateArrayJson(obj, opt)
        ElseIf DataFramework.IsPrimitive(schema) Then
            If schema Is GetType(Date) Then
                If opt.unixTimestamp Then
                    Return New JsonValue(DirectCast(obj, Date).UnixTimeStamp)
                Else
                    Return New JsonValue(obj)
                End If
            Else
                Return New JsonValue(obj)
            End If
        ElseIf schema.IsEnum Then
            If opt.enumToString Then
                Return New JsonValue(obj.ToString)
            Else
                Return New JsonValue(CLng(obj))
            End If
        ElseIf schema.IsInheritsFrom(GetType(Dictionary(Of, )), strict:=False) Then
            Dim valueType As Type = schema.GenericTypeArguments _
                .ElementAtOrDefault(
                    index:=1,
                    [default]:=schema.GenericTypeArguments(Scan0)
                )

            Return DirectCast(obj, IDictionary).populateTableJson(valueType, opt)
        Else
            If Not opt.digest Is Nothing AndAlso opt.digest.ContainsKey(schema) Then
                obj = opt.digest(schema)(obj)
                schema = obj.GetType

                Return GetJsonElement(schema, obj, opt)
            Else
                ' isObject
                Return schema.populateObjectJson(obj, opt)
            End If
        End If
    End Function
End Module
