#Region "Microsoft.VisualBasic::f19781cbea8a848c8bd4ec4a1c33b4af, mime\application%json\Serializer\JSONSerializer.vb"

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

    ' Module JSONSerializer
    ' 
    '     Function: (+2 Overloads) GetJson, populateArrayJson, populateObjectJson, populateTableJson
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.ValueTypes

Public Module JSONSerializer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="maskReadonly">
    ''' 如果这个参数为真，则不会序列化只读属性
    ''' </param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetJson(Of T)(obj As T,
                                  Optional maskReadonly As Boolean = False,
                                  Optional indent As Boolean = False,
                                  Optional enumToStr As Boolean = True,
                                  Optional unixTimestamp As Boolean = True) As String

        Return New JSONSerializerOptions With {
            .indent = indent,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }.DoCall(Function(opts)
                     Return obj.GetType.GetJson(obj, opts)
                 End Function)
    End Function

    <Extension>
    Private Function populateArrayJson(schema As Type, obj As Object, opt As JSONSerializerOptions) As IEnumerable(Of String)
        Dim elementSchema As Type
        Dim populator As IEnumerable(Of String)

        If schema.IsArray Then
            elementSchema = schema.GetElementType
            populator = From element As Object
                        In DirectCast(obj, Array)
                        Select elementSchema.GetJson(element, opt)
        Else
            ' list of type
            elementSchema = schema.GenericTypeArguments(Scan0)
            populator = From element As Object
                        In DirectCast(obj, IList)
                        Select elementSchema.GetJson(element, opt)
        End If

        Return populator
    End Function

    <Extension>
    Private Function populateObjectJson(schema As Type, obj As Object, opt As JSONSerializerOptions) As String
        Dim members As New List(Of String)
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

        For Each reader As KeyValuePair(Of String, PropertyInfo) In memberReaders
            [property] = reader.Value
            valueType = [property].PropertyType
            members += $"""{reader.Key}"": {valueType.GetJson([property].GetValue(obj, Nothing), opt)}"
        Next

        If opt.indent Then
            Return $"{{
            {members.JoinBy("," & ASCII.LF)}
        }}"
        Else
            Return $"{{{members.JoinBy(",")}}}"
        End If
    End Function

    ''' <summary>
    ''' 所有的字典键都会被强制转换为字符串类型
    ''' </summary>
    ''' <param name="obj"></param>
    ''' <param name="valueSchema"></param>
    ''' <returns></returns>
    <Extension>
    Private Function populateTableJson(obj As IDictionary, valueSchema As Type, opt As JSONSerializerOptions) As String
        Dim members As New List(Of String)
        Dim key As String
        Dim value As Object

        For Each member In obj
            key = Scripting.ToString(member.Key)
            value = member.Value
            members += $"""{key}"": {valueSchema.GetJson(value, opt)}"
        Next

        If opt.indent Then
            Return $"{{
            {members.JoinBy("," & ASCII.LF)}
        }}"
        Else
            Return $"{{{members.JoinBy(",")}}}"
        End If
    End Function

    <Extension>
    Public Function GetJson(schema As Type, obj As Object, opt As JSONSerializerOptions) As String
        If obj Is Nothing Then
            Return "null"
        ElseIf schema.IsAbstract OrElse schema Is GetType(Object) AndAlso Not obj Is Nothing Then
            schema = obj.GetType
        End If

        If schema.IsArray OrElse schema.IsInheritsFrom(GetType(List(Of )), strict:=False) Then
            Dim elementJSON = schema.populateArrayJson(obj, opt).ToArray

            If opt.indent Then
                Return $"[
                {elementJSON.JoinBy(", " & ASCII.LF)}
            ]"
            Else
                Return $"[{elementJSON.JoinBy(", ")}]"
            End If
        ElseIf DataFramework.IsPrimitive(schema) Then
            If schema Is GetType(Date) Then
                If opt.unixTimestamp Then
                    Return DirectCast(obj, Date).UnixTimeStamp
                Else
                    Return JsonContract.GetObjectJson(schema, obj)
                End If
            Else
                Return JsonContract.GetObjectJson(schema, obj)
            End If
        ElseIf schema.IsEnum Then
            If opt.enumToString Then
                Return """" & obj.ToString & """"
            Else
                Return CLng(obj)
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

                Return GetJson(schema, obj, opt)
            Else
                ' isObject
                Return schema.populateObjectJson(obj, opt)
            End If
        End If
    End Function
End Module
