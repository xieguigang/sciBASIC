#Region "Microsoft.VisualBasic::4f4f3bb565370ed291733e789e13a8ee, mime\application%json\Javascript\JsonObject.vb"

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

    '   Total Lines: 273
    '    Code Lines: 172 (63.00%)
    ' Comment Lines: 57 (20.88%)
    '    - Xml Docs: 75.44%
    ' 
    '   Blank Lines: 44 (16.12%)
    '     File Size: 9.75 KB


    '     Class JsonObject
    ' 
    '         Properties: isArray, ObjectKeys, size, UnderlyingType
    ' 
    '         Function: ContainsElement, (+2 Overloads) CreateObject, GetBoolean, GetCommentText, GetDate
    '                   GetDouble, GetEnumerator, GetInteger, GetString, HasObjectKey
    '                   IEnumerable_GetEnumerator, Remove, ToJsonArray, ToString
    ' 
    '         Sub: (+2 Overloads) Add, (+2 Overloads) Dispose, WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Javascript

    ''' <summary>
    ''' Dictionary/Array equivalent in javascript
    ''' </summary>
    Public Class JsonObject : Inherits JsonModel
        Implements IDisposable
        Implements IEnumerable(Of NamedValue(Of JsonElement))

        ReadOnly array As New Dictionary(Of String, JsonElement)
        ReadOnly comments As New Dictionary(Of String, String)

        Private disposedValue As Boolean

#Region "Indexer"

        Default Public Overloads Property Item(key As String) As JsonElement
            Get
                If array.ContainsKey(key) Then
                    Return array(key)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As JsonElement)
                array(key) = value
            End Set
        End Property

        Default Public Overloads Property Item(key As Value(Of String)) As JsonElement
            Get
                Return Me(key.Value)
            End Get
            Set(value As JsonElement)
                Me(key.Value) = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' get all member names in current json object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ObjectKeys As String()
            Get
                Return array.Keys.ToArray
            End Get
        End Property

        ''' <summary>
        ''' Does all of the member names in current json object is
        ''' a number[array schema]?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isArray As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return array.Keys.All(Function(i) i.IsPattern("\d+"))
            End Get
        End Property

        Public ReadOnly Property size As Integer
            Get
                Return array.TryCount
            End Get
        End Property

        ''' <summary>
        ''' try to measure of the array base element type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property UnderlyingType As Type
            Get
                Return JsonArray.MeasureUnderlyingType(array.Values)
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(key As String, element As JsonElement, Optional comment As String = Nothing)
            Call array.Add(key, element)

            If Not comment Is Nothing Then
                comments(key) = comment
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value">
        ''' .NET clr runtime value, this parameter value should be a literal constant
        ''' </param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(key$, value As Object, Optional comment As String = Nothing)
            Call array.Add(key, New JsonValue(value))

            If Not comment Is Nothing Then
                comments(key) = comment
            End If
        End Sub

        ''' <summary>
        ''' get comment string about the associated property key, used for generates the Hjson style json output.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Function GetCommentText(key As String) As String
            Return comments.TryGetValue(key)
        End Function

        Public Function GetString(key As String) As String
            If array.ContainsKey(key) Then
                Dim val As JsonValue = TryCast(array(key), JsonValue)

                If val Is Nothing OrElse val.IsLiteralNull Then
                    Return Nothing
                Else
                    Return Scripting.ToString(val.Literal)
                End If
            End If

            Return Nothing
        End Function

        Public Function GetInteger(key As String) As Integer
            Dim str As String = GetString(key)

            If str Is Nothing Then
                Return Nothing
            End If

            Return Integer.Parse(str)
        End Function

        Public Function GetDouble(key As String) As Double
            Return Val(GetString(key))
        End Function

        Public Function GetDate(key As String) As Date
            Return GetString(key).ParseDate
        End Function

        Public Function GetBoolean(key As String) As Boolean
            Return GetString(key).ParseBoolean
        End Function

        ''' <summary>
        ''' write bson buffer
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteBuffer(buffer As FileStream)
            Call BSON.WriteBuffer(Me, buffer)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Remove(key As String) As Boolean
            Return array.Remove(key)
        End Function

        ''' <summary>
        ''' Does the current json object has the required object member? 
        ''' </summary>
        ''' <param name="key">
        ''' the object member name
        ''' </param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function HasObjectKey(key As String) As Boolean
            Return array.ContainsKey(key)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ContainsElement(element As JsonElement) As Boolean
            Return array.ContainsValue(element)
        End Function

        Public Function ToJsonArray() As JsonArray
            Dim list As New JsonArray

            For Each item As JsonElement In array.Values
                Call list.Add(item)
            Next

            Return list
        End Function

        ''' <summary>
        ''' 反序列化为目标类型的对象实例
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateObject(Of T)(decodeMetachar As Boolean) As T
            Return DirectCast(CreateObject(type:=GetType(T), decodeMetachar), T)
        End Function

        Public Function CreateObject(type As Type, decodeMetachar As Boolean) As Object
            If type.IsArray AndAlso Me.isArray Then
                Dim itemType As Type = type.GetElementType
                Dim graph As SoapGraph = SoapGraph.GetSchema(itemType, Serializations.JSON)

                Return ToJsonArray.createArray(graph, itemType, decodeMetachar)
            Else
                Return Me.createObject(parent:=Nothing, schema:=type, decodeMetachar)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return "JsonObject::[" & array.Keys.JoinBy(", ") & "]"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of NamedValue(Of JsonElement)) Implements IEnumerable(Of NamedValue(Of JsonElement)).GetEnumerator
            For Each kp As KeyValuePair(Of String, JsonElement) In array
                Yield New NamedValue(Of JsonElement) With {
                    .Name = kp.Key,
                    .Value = kp.Value
                }
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Shared Widening Operator CType(json As JsonObject) As JavaScriptObject
            Return json.CreateJsObject
        End Operator

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    For Each value As JsonElement In array.Values
                        Call JsonModel.DisposeObjects(value)
                    Next

                    Call array.Clear()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
