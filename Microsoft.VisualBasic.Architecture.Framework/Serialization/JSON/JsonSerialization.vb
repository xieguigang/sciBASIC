#Region "Microsoft.VisualBasic::47ce25e01b5add6a244f7dee1000e83b, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Serialization\JSON\JsonSerialization.vb"

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

Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Web
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Serialization.JSON

    ''' <summary>
    ''' Only works on the Public visible type.
    ''' (使用.NET系统环境之中自带的框架进行JSON序列化和反序列化)
    ''' </summary>
    <PackageNamespace("Json.Contract")>
    Public Module JsonContract

        ''' <summary>
        ''' 使用<see cref="ScriptIgnoreAttribute"/>来屏蔽掉不想序列化的属性
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <ExportAPI("Get.Json")>
        Public Function GetJson(obj As Object, type As Type, Optional indent As Boolean = True, Optional simpleDict As Boolean = True) As String
            Using ms As New MemoryStream()
                Dim settings As New DataContractJsonSerializerSettings With {
                    .UseSimpleDictionaryFormat = True
                }
                Dim jsonSer As New DataContractJsonSerializer(type, settings)
                Call jsonSer.WriteObject(ms, obj)
                Dim json As String = Encoding.UTF8.GetString(ms.ToArray())
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
                    .UseSimpleDictionaryFormat = simpleDict
                }
                Dim jsonSer As New DataContractJsonSerializer(GetType(T), settings)
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
        <Extension> Public Function GetJson(Of T)(obj As T, Optional indent As Boolean = False, Optional simpleDict As Boolean = True) As String
            Return GetJson(obj, GetType(T), indent, simpleDict)
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
                    .UseSimpleDictionaryFormat = simpleDict
                }
                Dim ser As New DataContractJsonSerializer(type, settings)
                Dim obj As Object = ser.ReadObject(MS)
                Return obj
            End Using
        End Function

        <Extension>
        Public Function LoadJSONObject(jsonStream As Stream, type As Type, Optional simpleDict As Boolean = True)
            If jsonStream Is Nothing Then
                Return Nothing
            Else
                Dim settings As New DataContractJsonSerializerSettings With {
                    .UseSimpleDictionaryFormat = simpleDict
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

        <Extension>
        Public Function NamedProperty(Of T)(name As String, value As T) As String
            Dim json As String = value.GetJson
            Return $"""{name}"": " & json
        End Function

        ''' <summary>
        ''' 生成Json之中的动态属性
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NamedProperty(Of T)(x As NamedValue(Of T)) As String
            Return x.Name.NamedProperty(Of T)(x.x)
        End Function
    End Module
End Namespace
