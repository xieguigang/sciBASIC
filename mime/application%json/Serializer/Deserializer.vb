#Region "Microsoft.VisualBasic::b9eb644d04c7766f467927a474d41db9, mime\application%json\Serializer\Deserializer.vb"

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
    '     Function: createArray, createObject, CreateObject
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
    Public Function CreateObject(json As JsonElement, schema As Type) As Object
        If TypeOf json Is JsonArray Then
            If Not schema.IsArray Then
                Throw New InvalidCastException
            Else
                Return DirectCast(json, JsonArray).createArray(schema.GetElementType)
            End If
        ElseIf TypeOf json Is JsonObject Then
            Return DirectCast(json, JsonObject).createObject(schema)
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

    <Extension>
    Private Function createObject(json As JsonObject, schema As Type) As Object
        Dim obj As Object = Activator.CreateInstance(schema)
        Dim isTable As Boolean = schema.IsInheritsFrom(GetType(DictionaryBase))
        Dim writers = schema.Schema(PropertyAccess.Writeable, PublicProperty, nonIndex:=True)
        Dim writer As PropertyInfo
        Dim addMethod As MethodInfo = schema.GetMethods _
            .Where(Function(m)
                       Dim params = m.GetParameters

                       Return Not m.IsStatic AndAlso
                           Not params.IsNullOrEmpty AndAlso
                           params.Length = 2 AndAlso
                           m.Name = "Add"
                   End Function) _
            .FirstOrDefault
        Dim valueType As Type = Nothing
        Dim inputs As Object()

        If isTable Then
            valueType = schema.GetGenericArguments(1)
        End If

        For Each [property] As NamedValue(Of JsonElement) In json
            If writers.ContainsKey([property].Name) Then
                writer = writers([property].Name)
                writer.SetValue(obj, [property].Value.CreateObject(writer.PropertyType))
            ElseIf isTable AndAlso Not addMethod Is Nothing Then
                inputs = {
                    [property].Name,
                    [property].Value.CreateObject(valueType)
                }
                addMethod.Invoke(obj, inputs)
            End If
        Next

        Return obj
    End Function
End Module
