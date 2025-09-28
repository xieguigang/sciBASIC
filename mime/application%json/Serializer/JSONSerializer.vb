#Region "Microsoft.VisualBasic::572062dfcf57f17eb965667949a2bb72, mime\application%json\Serializer\JSONSerializer.vb"

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

    '   Total Lines: 89
    '    Code Lines: 64 (71.91%)
    ' Comment Lines: 13 (14.61%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 12 (13.48%)
    '     File Size: 3.38 KB


    ' Module JSONSerializer
    ' 
    '     Function: (+2 Overloads) BuildJsonString, CreateArray, CreateJSONElement, GetJson
    ' 
    '     Sub: WriteJSON
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Public Module JSONSerializer

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="maskReadonly">
    ''' 如果这个参数为真，则不会序列化只读属性
    ''' </param>
    ''' <param name="comment">
    ''' add property xml comment summary as comment into the generated json text? 
    ''' this option usually be apply to generates the config json file.
    ''' </param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetJson(Of T)(obj As T,
                                  Optional maskReadonly As Boolean = False,
                                  Optional indent As Boolean = False,
                                  Optional enumToStr As Boolean = True,
                                  Optional unixTimestamp As Boolean = True,
                                  Optional comment As Boolean = False) As String

        Return New JSONSerializerOptions With {
            .indent = indent,
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp,
            .comment = comment
        }.DoCall(Function(opts)
                     Return obj.GetType.GetJsonElement(obj, opts).BuildJsonString(opts)
                 End Function)
    End Function

    <Extension>
    Public Function CreateJSONElement(Of T)(obj As T,
                                            Optional maskReadonly As Boolean = False,
                                            Optional enumToStr As Boolean = True,
                                            Optional unixTimestamp As Boolean = True) As JsonElement

        Return New JSONSerializerOptions With {
            .maskReadonly = maskReadonly,
            .enumToString = enumToStr,
            .unixTimestamp = unixTimestamp
        }.DoCall(Function(opts)
                     Return obj.GetType.GetJsonElement(obj, opts)
                 End Function)
    End Function

    <Extension>
    Public Function BuildJsonString(json As JsonElement, Optional indent As Boolean = False) As String
        Return json.BuildJsonString(New JSONSerializerOptions With {.indent = indent})
    End Function

    <Extension>
    Public Function BuildJsonString(json As JsonElement, opts As JSONSerializerOptions) As String
        Dim json_str As New StringBuilder
        Dim writer As New JSONWriter(opts, json_str)
        Call writer.BuildJSONString(json)
        Call writer.Dispose()
        Return json_str.ToString
    End Function

    <Extension>
    Public Sub WriteJSON(json As JsonElement, file As Stream, opts As JSONSerializerOptions)
        Dim writer As New JSONWriter(opts, file)
        Call writer.BuildJSONString(json)
        Call writer.Dispose()
    End Sub

    <Extension>
    Public Function CreateArray(objs As IEnumerable(Of JsonObject)) As JsonArray
        Dim list As New JsonArray

        For Each x As JsonObject In objs
            Call list.Add(x)
        Next

        Return list
    End Function

End Module
