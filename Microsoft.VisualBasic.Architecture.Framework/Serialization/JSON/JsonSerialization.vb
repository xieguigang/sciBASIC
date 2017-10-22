#Region "Microsoft.VisualBasic::29d7d662e9f4d6df218eb314c17418ab, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Serialization\JSON\JsonSerialization.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports r = System.Text.RegularExpressions.Regex

Namespace Serialization.JSON

    ''' <summary>
    ''' Only works on the Public visible type.
    ''' (使用.NET系统环境之中自带的框架进行JSON序列化和反序列化)
    ''' </summary>
    <Package("Json.Contract")>
    Public Module JsonContract

        ''' <summary>
        ''' 使用<see cref="ScriptIgnoreAttribute"/>来屏蔽掉不想序列化的属性
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <ExportAPI("Get.Json")>
        Public Function GetObjectJson(obj As Object, type As Type, Optional indent As Boolean = True, Optional simpleDict As Boolean = True) As String
            Using ms As New MemoryStream()
                Dim settings As New DataContractJsonSerializerSettings With {
                    .UseSimpleDictionaryFormat = True,
                    .SerializeReadOnlyTypes = True
                }
                Dim jsonSer As DataContractJsonSerializer = If(
                    simpleDict,
                    New DataContractJsonSerializer(type, settings),
                    New DataContractJsonSerializer(type))

                Call jsonSer.WriteObject(ms, obj)

                Dim json$ = Encoding.UTF8.GetString(ms.ToArray())
                If indent Then
                    json = Formatter.Format(json)
                End If
                Return json
            End Using
        End Function

        ''' <summary>
        ''' 将目标对象保存为json文件
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteLargeJson(Of T)(obj As T, path As String, Optional simpleDict As Boolean = True) As Boolean
            Call "".SaveTo(path)

            Using ms As FileStream = path.Open
                Dim settings As New DataContractJsonSerializerSettings With {
                    .UseSimpleDictionaryFormat = simpleDict,
                    .SerializeReadOnlyTypes = True
                }
                Dim jsonSer As DataContractJsonSerializer = If(
                    simpleDict,
                    New DataContractJsonSerializer(GetType(T), settings),
                    New DataContractJsonSerializer(GetType(T)))
                Call jsonSer.WriteObject(ms, obj)
                Return True
            End Using
        End Function

        ''' <summary>
        ''' Gets the json text value of the target object, the attribute <see cref="ScriptIgnoreAttribute"/> 
        ''' can be used for block the property which is will not serialize to the text.
        ''' (使用<see cref="ScriptIgnoreAttribute"/>来屏蔽掉不想序列化的属性)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 2016-11-9 对字典进行序列化的时候，假若对象类型是从字典类型继承而来的，则新的附加属性并不会被序列化，只会序列化字典本身
        ''' </remarks>
        <Extension> Public Function GetJson(Of T)(obj As T, Optional indent As Boolean = False, Optional simpleDict As Boolean = True) As String
            Return GetObjectJson(obj, GetType(T), indent, simpleDict)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="json">null -> Nothing</param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <ExportAPI("LoadObject")>
        <Extension>
        Public Function LoadObject(json As String, type As Type, Optional simpleDict As Boolean = True) As Object
            If String.Equals(json, "null", StringComparison.OrdinalIgnoreCase) Then
                Return Nothing
            End If

            Using MS As New MemoryStream(Encoding.UTF8.GetBytes(json))
                Dim settings As New DataContractJsonSerializerSettings With {
                    .UseSimpleDictionaryFormat = simpleDict,
                    .SerializeReadOnlyTypes = True
                }
                Dim ser As New DataContractJsonSerializer(type, settings)
                Dim obj As Object = ser.ReadObject(MS)
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
        ''' JSON反序列化
        ''' </summary>
        <Extension> Public Function LoadObject(Of T)(json As String, Optional simpleDict As Boolean = True) As T
            Dim value As Object = LoadObject(json, GetType(T), simpleDict)
            Dim obj As T = DirectCast(value, T)
            Return obj
        End Function

        Public Function LoadJsonFile(Of T)(file As String, Optional encoding As Encoding = Nothing, Optional simpleDict As Boolean = True) As T
            Dim json As String = IO.File.ReadAllText(file, If(encoding Is Nothing, Encoding.Default, encoding))
            Return json.LoadObject(Of T)(simpleDict)
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

                        [date] = Date.Parse(.ref)
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
