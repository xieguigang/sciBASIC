#Region "Microsoft.VisualBasic::dfdb7ba540465fcdc9ec28dac86c6b99, mime\application%json\Javascript\JsonObject.vb"

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
    '                   Remove, Score, ToString
    ' 
    '         Sub: (+2 Overloads) Add, (+2 Overloads) Dispose, WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace Javascript

    ''' <summary>
    ''' Dictionary/Array equivalent in javascript
    ''' </summary>
    Public Class JsonObject : Inherits JsonModel
        Implements IDisposable
        Implements IEnumerable(Of NamedValue(Of JsonElement))

        ReadOnly array As New Dictionary(Of String, JsonElement)

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

        Public Function Score(schema As Type) As Integer
            Dim hits As Integer

            For Each [property] As PropertyInfo In schema.GetProperties(PublicProperty)
                If array.ContainsKey([property].Name) Then
                    hits += 1
                End If
            Next

            Return hits
        End Function

        ''' <summary>
        ''' 反序列化为目标类型的对象实例
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Public Function CreateObject(Of T As Class)() As T
            Return Me.createObject(parent:=Nothing, schema:=GetType(T))
        End Function

        Public Function CreateObject(type As Type) As Object
            Return Me.createObject(parent:=Nothing, schema:=type)
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
