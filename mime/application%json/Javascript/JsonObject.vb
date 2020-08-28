#Region "Microsoft.VisualBasic::b73a666b3d517071d7b59a6b761b9b9e, mime\application%json\Javascript\JsonObject.vb"

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

    '     Class JsonObject
    ' 
    '         Function: ContainsElement, ContainsKey, CreateObject, GetEnumerator, IEnumerable_GetEnumerator
    '                   Remove, ToString
    ' 
    '         Sub: (+2 Overloads) Add, WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace Javascript

    ''' <summary>
    ''' Dictionary/Array equivalent in javascript
    ''' </summary>
    Public Class JsonObject : Inherits JsonModel
        Implements IEnumerable(Of NamedValue(Of JsonElement))

        ReadOnly array As New Dictionary(Of String, JsonElement)

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

        Public Sub Add(key As String, element As JsonElement)
            Call array.Add(key, element)
        End Sub

        Public Sub Add(key$, value As Object)
            Call array.Add(key, New JsonValue(value))
        End Sub

        ''' <summary>
        ''' write bson buffer
        ''' </summary>
        ''' <param name="buffer"></param>
        Public Sub WriteBuffer(buffer As FileStream)
            Call BSON.WriteBuffer(Me, buffer)
        End Sub

        Public Function Remove(key As String) As Boolean
            Return array.Remove(key)
        End Function

        Public Function ContainsKey(key As String) As Boolean
            Return array.ContainsKey(key)
        End Function

        Public Function ContainsElement(element As JsonElement) As Boolean
            Return array.ContainsValue(element)
        End Function

        ''' <summary>
        ''' 反序列化为目标类型的对象实例
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Public Function CreateObject(Of T As Class)() As T
            Return Me.createObject(schema:=GetType(T))
        End Function

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

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
