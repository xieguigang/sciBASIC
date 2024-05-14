#Region "Microsoft.VisualBasic::742c8bfa81660fc6ae66f5eae36b658d, Microsoft.VisualBasic.Core\src\Serialization\JSON\JsonSerialization.vb"

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

    '   Total Lines: 327
    '    Code Lines: 209
    ' Comment Lines: 80
    '   Blank Lines: 38
    '     File Size: 13.72 KB


    '     Module JsonContract
    ' 
    '         Function: EnsureDate, GetJson, (+2 Overloads) GetObjectJson, LoadJSON, LoadJsonFile
    '                   LoadJSONObject, (+2 Overloads) LoadObject, MatrixJson, RemoveJsonNullItems, WriteLargeJson
    ' 
    '         Sub: writeJson
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NETCOREAPP Then
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
#Else
Imports System.Web.Script.Serialization
#End If

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports Factory = System.Runtime.Serialization.Json.DataContractJsonSerializer
Imports r = System.Text.RegularExpressions.Regex

Namespace Serialization.JSON

    ''' <summary>
    ''' Only works on the Public visible type.
    ''' (使用.NET系统环境之中自带的框架进行JSON序列化和反序列化)
    ''' </summary>
    <Package("Json.Contract")> Public Module JsonContract

        ''' <summary>
        ''' Create json text for array matrix.
        ''' </summary>
        ''' <param name="matrix"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MatrixJson(matrix As Double()()) As String
            Dim rows = matrix.Select(Function(row) $"[ {row.JoinBy(", ")} ]")
            Dim json = $"[ {rows.JoinBy("," & ASCII.LF)} ]"
            Return json
        End Function

        Public Function GetObjectJson(obj As Object,
                                      Optional indent As Boolean = True,
                                      Optional simpleDict As Boolean = True,
                                      Optional knownTypes As IEnumerable(Of Type) = Nothing) As String
            If obj Is Nothing Then
                Return "null"
            Else
                Return obj.GetType.GetObjectJson(
                    obj:=obj,
                    indent:=indent,
                    simpleDict:=simpleDict,
                    knownTypes:=knownTypes
                )
            End If
        End Function

        ''' <summary>
        ''' 使用<see cref="ScriptIgnoreAttribute"/>来屏蔽掉不想序列化的属性
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetObjectJson(type As Type, obj As Object,
                                      Optional indent As Boolean = True,
                                      Optional simpleDict As Boolean = True,
                                      Optional knownTypes As IEnumerable(Of Type) = Nothing) As String

            Static emptyTypeList As [Default](Of IEnumerable(Of Type)) = New Type() {}

            Using ms As New MemoryStream()
                Call ms.writeJson(
                    obj:=obj,
                    type:=type,
                    simpleDict:=simpleDict,
                    knownTypes:=knownTypes Or emptyTypeList
                )

                If indent Then
                    Return Formatter.Format(Encoding.UTF8.GetString(ms.ToArray()))
                Else
                    Return Encoding.UTF8.GetString(ms.ToArray())
                End If
            End Using
        End Function

        ''' <summary>
        ''' Write json with setting configuration
        ''' </summary>
        ''' <param name="output"></param>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        ''' <param name="simpleDict"></param>
        ''' <param name="knownTypes"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Sub writeJson(output As Stream, obj As Object, type As Type, simpleDict As Boolean, knownTypes As IEnumerable(Of Type))
            If simpleDict Then
                Dim settings As New DataContractJsonSerializerSettings With {
                    .UseSimpleDictionaryFormat = True,
                    .SerializeReadOnlyTypes = True,
                    .KnownTypes = knownTypes _
                        .SafeQuery _
                        .ToArray
                }
                Call New Factory(type, settings).WriteObject(output, obj)
            Else
                Call New Factory(type).WriteObject(output, obj)
            End If
        End Sub

        ''' <summary>
        ''' 将目标对象保存为json文件
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteLargeJson(Of T)(obj As T, path$, Optional simpleDict As Boolean = True) As Boolean
            Using ms As FileStream = path.Open(, doClear:=True)
                Call ms.writeJson(obj, GetType(T), simpleDict, Nothing)
            End Using

            Return True
        End Function

        ''' <summary>
        ''' 有些javascript程序(例如highcharts.js)要求json里面不可以出现null的属性，可以使用这个方法进行移除
        ''' </summary>
        ''' <param name="json"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function RemoveJsonNullItems(json As String) As String
            Return r.Replace(json, """[^""]+""[:]\s*null\s*,?", "", RegexICSng)
        End Function

        ''' <summary>
        ''' Gets the json text value of the target object, the attribute <see cref="ScriptIgnoreAttribute"/> 
        ''' can be used for block the property which is will not serialize to the text.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (使用<see cref="ScriptIgnoreAttribute"/>来屏蔽掉不想序列化的属性)
        ''' 
        ''' 2016-11-9 对字典进行序列化的时候，假若对象类型是从字典类型继承而来的，则新的附加属性并不会被序列化，只会序列化字典本身
        ''' 2018-10-5 不可以序列化匿名类型
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetJson(Of T)(obj As T,
                                      Optional indent As Boolean = False,
                                      Optional simpleDict As Boolean = True,
                                      Optional knownTypes As IEnumerable(Of Type) = Nothing) As String
            Dim schema As Type

            If obj Is Nothing Then
                Return "null"
            End If

            If GetType(T) Is GetType(Array) AndAlso Not obj Is Nothing Then
                schema = obj.GetType
            Else
                schema = GetType(T)
            End If

            Return schema.GetObjectJson(obj, indent, simpleDict, knownTypes)
        End Function

        ''' <summary>
        ''' Create object instance from a given json text base on the template <see cref="Type"/>
        ''' information.
        ''' </summary>
        ''' <param name="json">null -> Nothing</param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <ExportAPI("LoadObject")>
        <Extension>
        Public Function LoadObject(json$,
                                   type As Type,
                                   Optional simpleDict As Boolean = True,
                                   Optional throwEx As Boolean = True,
                                   Optional ByRef exception As Exception = Nothing,
                                   Optional knownTypes As IEnumerable(Of Type) = Nothing) As Object

            If String.Equals(json, "null", StringComparison.OrdinalIgnoreCase) Then
                Return Nothing
            ElseIf json.StringEmpty Then
                If throwEx Then
                    Throw New NullReferenceException("Empty json text!")
                Else
                    Return Nothing
                End If
            End If

            Using MS As New MemoryStream(Encoding.UTF8.GetBytes(json))
                Dim settings As New DataContractJsonSerializerSettings With {
                    .UseSimpleDictionaryFormat = simpleDict,
                    .SerializeReadOnlyTypes = True,
                    .KnownTypes = If(knownTypes, DirectCast(New Type() {}, IEnumerable(Of Type))).ToArray
                }
                Dim ser As New DataContractJsonSerializer(type, settings)
                Dim de As Func(Of Object) = Function() ser.ReadObject(MS)
                Dim obj = TryCatch(de, $"Incorrect JSON string format => >>>{json}<<<", throwEx, exception)
                Return obj
            End Using
        End Function

        <Extension>
        Public Function LoadJSONObject(jsonStream As Stream, type As Type, Optional simpleDict As Boolean = True) As Object
            If jsonStream Is Nothing Then
                Return Nothing
            Else
                Dim settings As New DataContractJsonSerializerSettings With {
                    .UseSimpleDictionaryFormat = simpleDict,
                    .SerializeReadOnlyTypes = True
                }
                Return New DataContractJsonSerializer(type, settings) _
                    .ReadObject(jsonStream)
            End If
        End Function

        ''' <summary>
        ''' 从文本文件或者文本内容之中进行JSON反序列化
        ''' </summary>
        ''' <param name="json">This string value can be json text or json file path.</param>
        ''' <remarks>
        ''' null or empty string will be parsed as nothing if the optional
        ''' parameter <paramref name="throwEx"/> option value is set to false
        ''' </remarks>
        <Extension>
        Public Function LoadJSON(Of T)(json$,
                                       Optional simpleDict As Boolean = True,
                                       Optional throwEx As Boolean = True,
                                       Optional ByRef exception As Exception = Nothing,
                                       Optional knownTypes As IEnumerable(Of Type) = Nothing) As T

            Dim text$ = json.SolveStream(Encodings.UTF8)

            If text.StringEmpty Then
                If throwEx Then
                    Throw New NullReferenceException("empty json text!")
                Else
                    Return Nothing
                End If
            ElseIf text.TextEquals("null") Then
                Return Nothing
            End If

            Dim value As Object = text.LoadObject(GetType(T), simpleDict, throwEx, exception, knownTypes)
            Dim obj As T = DirectCast(value, T)
            Return obj
        End Function

        ''' <summary>
        ''' XML CDATA to json
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="json"></param>
        ''' <param name="simpleDict"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function LoadObject(Of T As New)(json As XElement, Optional simpleDict As Boolean = True) As T
            Return json.Value.LoadJSON(Of T)(simpleDict:=simpleDict)
        End Function

        ''' <summary>
        ''' Create object instance from a json text of a given text file. 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="file$"></param>
        ''' <param name="encoding"></param>
        ''' <param name="simpleDict"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function LoadJsonFile(Of T)(file$,
                                           Optional encoding As Encoding = Nothing,
                                           Optional simpleDict As Boolean = True,
                                           Optional knownTypes As IEnumerable(Of Type) = Nothing) As T

            Return (file.ReadAllText(encoding Or UTF8, throwEx:=False, suppress:=True) Or "null".AsDefault) _
                .LoadJSON(Of T)(simpleDict, knownTypes:=knownTypes)
        End Function

        Const JsonLongTime$ = "\d+-\d+-\d+T\d+:\d+:\d+\.\d+"

        Public Function EnsureDate(json$, Optional propertyName$ = Nothing) As String
            Dim pattern$ = $"""{JsonLongTime}"""

            If Not propertyName.StringEmpty Then
                pattern = $"""{propertyName}""\s*:\s*" & pattern
            End If

            Dim dates = r.Matches(json, pattern, RegexICSng)
            Dim sb As New StringBuilder(json)
            Dim [date] As Date

            For Each m As Match In dates
                Dim s$ = m.Value

                If Not propertyName.StringEmpty Then
                    With r.Replace(s, $"""{propertyName}""\s*:", "", RegexICSng) _
                        .Trim _
                        .Trim(ASCII.Quot)

                        [date] = Date.Parse(.ByRef)
                    End With

                    sb.Replace(s, $"""{propertyName}"":" & [date].GetJson)
                Else
                    [date] = Date.Parse(s.Trim(ASCII.Quot))

                    sb.Replace(s, [date].GetJson)
                End If
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace
