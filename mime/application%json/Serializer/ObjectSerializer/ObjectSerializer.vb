#Region "Microsoft.VisualBasic::7cce882909080160e24a03f21516df67, sciBASIC#\mime\application%json\Serializer\ObjectSerializer\ObjectSerializer.vb"

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

'   Total Lines: 154
'    Code Lines: 119
' Comment Lines: 16
'   Blank Lines: 19
'     File Size: 5.89 KB


' Module ObjectSerializer
' 
'     Function: GetJsonElement, populateArrayJson, populateObjectJson, populateTableJson
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
#If netcore5 = 0 Then
Imports System.Web.Script.Serialization
#End If
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.ValueTypes

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

    <Extension>
    Private Function populateObjectJson(schema As Type, obj As Object, opt As JSONSerializerOptions) As JsonElement
        ' 会需要忽略掉有<ScriptIgnore>标记的属性
        Dim memberReaders = schema _
            .Schema(PropertyAccess.Readable, nonIndex:=True) _
            .Where(Function(p)
                       If opt.maskReadonly AndAlso Not p.Value.CanWrite Then
                           Return False
                       End If

                       Return p.Value.GetAttribute(Of ScriptIgnoreAttribute) Is Nothing
                   End Function)
        Dim [property] As PropertyInfo
        Dim valueType As Type
        Dim json As New JsonObject
        Dim valObj As Object
        Dim graph As ObjectSchema = ObjectSchema.GetSchema(schema)
        Dim jsonVal As JsonElement

        For Each reader As KeyValuePair(Of String, PropertyInfo) In memberReaders
            [property] = reader.Value
            valueType = [property].PropertyType
            valObj = [property].GetValue(obj)

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
                valueType.FullName = "System.RuntimeType") Then

                jsonVal = valueType.GetJsonElement(valObj, opt)
            Else
                jsonVal = Nothing
            End If

            If Not jsonVal Is Nothing Then
                Call json.Add(reader.Key, jsonVal)
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
            key = Scripting.ToString(memberKey)
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
    ''' Convert any .NET object as json element model for build json string or bson data
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <param name="obj"></param>
    ''' <param name="opt"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetJsonElement(schema As Type, obj As Object, opt As JSONSerializerOptions) As JsonElement
        If obj Is Nothing Then
            Return Nothing
        ElseIf schema.IsAbstract OrElse schema Is GetType(Object) AndAlso Not obj Is Nothing Then
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
            Dim valueType As Type = schema _
                .GenericTypeArguments _
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
