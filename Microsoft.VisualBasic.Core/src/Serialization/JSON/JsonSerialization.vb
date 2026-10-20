#Region "Microsoft.VisualBasic::8ce9445010f043059a84106ef51fdda8, Microsoft.VisualBasic.Core\src\Serialization\JSON\JsonSerialization.vb"

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

    '   Total Lines: 375
    '    Code Lines: 234 (62.40%)
    ' Comment Lines: 100 (26.67%)
    '    - Xml Docs: 97.00%
    ' 
    '   Blank Lines: 41 (10.93%)
    '     File Size: 16.36 KB


    '     Module JsonContract
    ' 
    '         Function: EnsureDate, GetJson, (+2 Overloads) GetObjectJson, LoadJSON, LoadJsonFile
    '                   (+2 Overloads) LoadJSONL, LoadJSONObject, (+2 Overloads) LoadObject, MatrixJson, RemoveJsonNullItems
    '                   WriteLargeJson
    ' 
    '         Sub: writeJson
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Text.Json.Serialization.Metadata
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
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
                    knownTypes:=knownTypes Or emptyTypeList,
                    indent:=indent
                )

                Return Encoding.UTF8.GetString(ms.ToArray())
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
        Private Sub writeJson(output As Stream, obj As Object, type As Type, simpleDict As Boolean, indent As Boolean, knownTypes As IEnumerable(Of Type))
            ' simpleDict is kept for API compatibility only: System.Text.Json always
            ' serializes a string-keyed dictionary as a JSON object (i.e. the simple
            ' dictionary format that DataContractJsonSerializer produced with
            ' UseSimpleDictionaryFormat = True).
            Call JsonSerializer.Serialize(output, obj, type, BuildOptions(type, knownTypes))
        End Sub

        ''' <summary>
        ''' 将目标对象保存为json文件
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteLargeJson(Of T)(obj As T, path$, Optional simpleDict As Boolean = True, Optional indent As Boolean = False) As Boolean
            Using ms As FileStream = path.Open(, doClear:=True)
                Call ms.writeJson(obj, GetType(T), simpleDict, indent:=indent, knownTypes:=Nothing)
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
        ''' <param name="json">parse this json string and then decoded as the required .NET clr object. some special json text mapping: null will be decoded as Nothing</param>
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

            Dim normalized = ConvertMicrosoftDates(json)
            Dim de As Func(Of Object) = Function() JsonSerializer.Deserialize(normalized, type, BuildOptions(type, knownTypes))
            Dim obj = TryCatch(de, $"Incorrect JSON string format => >>>{json}<<<", throwEx, exception)
            Return obj
        End Function

        <Extension>
        Public Function LoadJSONObject(jsonStream As Stream, type As Type, Optional simpleDict As Boolean = True) As Object
            If jsonStream Is Nothing Then
                Return Nothing
            Else
                Using reader As New StreamReader(jsonStream, Encoding.UTF8)
                    Dim json = reader.ReadToEnd()
                    Return JsonSerializer.Deserialize(ConvertMicrosoftDates(json), type, BuildOptions(type, Nothing))
                End Using
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
        ''' load json list file
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="file"></param>
        ''' <param name="encoding"></param>
        ''' <param name="simpleDict"></param>
        ''' <param name="knownTypes"></param>
        ''' <param name="throwEx"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function LoadJSONL(Of T)(file$,
                                                 Optional encoding As Encoding = Nothing,
                                                 Optional simpleDict As Boolean = True,
                                                 Optional knownTypes As IEnumerable(Of Type) = Nothing,
                                                 Optional throwEx As Boolean = True) As IEnumerable(Of T)

            Using jsonl As New StreamReader(file.OpenReadonly(), encoding)
                Dim json_str As Value(Of String) = ""

                Do While (json_str = jsonl.ReadLine) IsNot Nothing
                    Yield json_str.LoadJSON(Of T)(simpleDict:=simpleDict, throwEx:=throwEx, knownTypes:=knownTypes)
                Loop
            End Using
        End Function

        ''' <summary>
        ''' load json list file
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="jsonl">the file content of the jsonl file</param>
        ''' <param name="encoding"></param>
        ''' <param name="simpleDict"></param>
        ''' <param name="knownTypes"></param>
        ''' <param name="throwEx"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function LoadJSONL(Of T)(jsonl As IEnumerable(Of String),
                                                 Optional encoding As Encoding = Nothing,
                                                 Optional simpleDict As Boolean = True,
                                                 Optional knownTypes As IEnumerable(Of Type) = Nothing,
                                                 Optional throwEx As Boolean = True) As IEnumerable(Of T)
            For Each json_str As String In jsonl
                Yield json_str.LoadJSON(Of T)(
                    simpleDict:=simpleDict,
                    throwEx:=throwEx,
                    knownTypes:=knownTypes
                )
            Next
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
                                           Optional knownTypes As IEnumerable(Of Type) = Nothing,
                                           Optional throwEx As Boolean = True) As T

            Return (file.ReadAllText(encoding Or UTF8, throwEx:=throwEx, suppress:=True) Or "null".AsDefault) _
                .LoadJSON(Of T)(simpleDict, knownTypes:=knownTypes, throwEx:=throwEx)
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

        ' Matches the /Date(ticks)/ and /Date(ticks+hhmm)/ forms previously emitted by
        ' DataContractJsonSerializer, keeping the surrounding double quotes via look-around.
        Private ReadOnly microsoftDatePattern As New Regex("(?<= "")/Date\((-?\d+)([+\-]\d{4})?\)/(?= "")", RegexOptions.None)

        ''' <summary>
        ''' Rewrite legacy ``/Date(...)/`` tokens into ISO 8601 strings so that JSON
        ''' persisted by the old DataContractJsonSerializer based code keeps loading.
        ''' </summary>
        Private Function ConvertMicrosoftDates(json As String) As String
            If json Is Nothing Then
                Return json
            End If

            Return microsoftDatePattern.Replace(json, AddressOf NormalizeMicrosoftDate)
        End Function

        Private Function NormalizeMicrosoftDate(m As Match) As String
            Dim ticks As Long = Long.Parse(m.Groups(1).Value, CultureInfo.InvariantCulture)
            Dim utc = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ticks)
            Dim offset = m.Groups(2).Value

            If offset.Length = 5 Then
                Dim sign = If(offset(0) = "-"c, -1, 1)
                Dim hh = Integer.Parse(offset.Substring(1, 2), CultureInfo.InvariantCulture)
                Dim mm = Integer.Parse(offset.Substring(3, 2), CultureInfo.InvariantCulture)
                utc = utc.AddMinutes(-sign * (hh * 60 + mm))
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Local)
            End If

            Return utc.ToString("o", CultureInfo.InvariantCulture)
        End Function

        ''' <summary>
        ''' Cached options for the common case (no polymorphic known types).
        ''' </summary>
        Private ReadOnly defaultOptions As JsonSerializerOptions = CreateDefaultOptions()

        ''' <summary>
        ''' Build the <see cref="JsonSerializerOptions"/> used for (de)serialization. When
        ''' <paramref name="knownTypes"/> is supplied, polymorphic (de)serialization is enabled
        ''' using the DataContract convention of a ``__type`` discriminator so that JSON
        ''' previously emitted by the legacy DataContractJsonSerializer keeps working.
        ''' </summary>
        Private Function BuildOptions(type As Type, knownTypes As IEnumerable(Of Type)) As JsonSerializerOptions
            Dim types As Type()

            If knownTypes Is Nothing Then
                types = New Type() {}
            Else
                types = knownTypes.Where(Function(t) t IsNot Nothing).ToArray()
            End If

            If types.Length = 0 Then
                Return defaultOptions
            End If

            Dim options As New JsonSerializerOptions() With {
                .IncludeFields = True,
                .DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                .PropertyNamingPolicy = Nothing,
                .DictionaryKeyPolicy = Nothing,
                .WriteIndented = False
            }

            Dim resolver As New DefaultJsonTypeInfoResolver()
            resolver.Modifiers.Add(AddressOf IgnoreUnwantedMembers)
            resolver.Modifiers.Add(Sub(info) RegisterKnownTypes(info, types))
            options.TypeInfoResolver = resolver

            Return options
        End Function

        Private Function CreateDefaultOptions() As JsonSerializerOptions
            Dim options As New JsonSerializerOptions() With {
                .IncludeFields = True,
                .DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                .PropertyNamingPolicy = Nothing,
                .DictionaryKeyPolicy = Nothing,
                .WriteIndented = False
            }

            Dim resolver As New DefaultJsonTypeInfoResolver()
            resolver.Modifiers.Add(AddressOf IgnoreUnwantedMembers)
            options.TypeInfoResolver = resolver

            Return options
        End Function

        ''' <summary>
        ''' Drop members that DataContractJsonSerializer would have skipped: those marked with
        ''' <see cref="IgnoreDataMemberAttribute"/> or the framework's custom
        ''' <see cref="ScriptIgnoreAttribute"/>.
        ''' </summary>
        Private Sub IgnoreUnwantedMembers(info As JsonTypeInfo)
            For i As Integer = info.Properties.Count - 1 To 0 Step -1
                Dim provider = info.Properties(i).AttributeProvider

                If provider Is Nothing Then
                    Continue For
                End If

                Dim ignored = provider _
                    .GetCustomAttributes(GetType(IgnoreDataMemberAttribute), True) _
                    .Length > 0 OrElse
                    provider _
                    .GetCustomAttributes(GetType(ScriptIgnoreAttribute), True) _
                    .Length > 0

                If ignored Then
                    info.Properties.RemoveAt(i)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Enable polymorphic (de)serialization for the declared base type and any of its
        ''' ancestors that appear in <paramref name="knownTypes"/>, mirroring the
        ''' KnownTypes behaviour of the legacy DataContractJsonSerializer.
        ''' </summary>
        Private Sub RegisterKnownTypes(info As JsonTypeInfo, knownTypes As Type())
            Dim derived = knownTypes _
                .Where(Function(t) t IsNot info.Type AndAlso info.Type.IsAssignableFrom(t)) _
                .ToArray()

            If derived.Length = 0 Then
                Return
            End If

            If info.PolymorphismOptions Is Nothing Then
                info.PolymorphismOptions = New JsonPolymorphismOptions()
            End If

            info.PolymorphismOptions.TypeDiscriminatorPropertyName = "__type"

            For Each d In derived
                If Not info.PolymorphismOptions.DerivedTypes.Any(Function(dt) dt.DerivedType = d) Then
                    info.PolymorphismOptions.DerivedTypes.Add(New JsonDerivedType(d))
                End If
            Next
        End Sub
    End Module
End Namespace
