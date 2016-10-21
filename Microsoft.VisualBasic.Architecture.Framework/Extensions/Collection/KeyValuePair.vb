#Region "Microsoft.VisualBasic::899f5ac86810c1921bf097aee139d171, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\Collection\KeyValuePair.vb"

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

Imports System.Collections.Specialized
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Public Module KeyValuePairExtensions

    ''' <summary>
    ''' 请注意，这里的类型约束只允许枚举类型
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Public Function ParserDictionary(Of T)() As Dictionary(Of String, T)
        Return Enums(Of T).ToDictionary(Function(x) DirectCast(CType(x, Object), [Enum]).Description)
    End Function

    ''' <summary>
    ''' Data exists and not nothing
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="d"></param>
    ''' <param name="key"></param>
    ''' <returns></returns>
    <Extension>
    Public Function HaveData(Of T)(d As Dictionary(Of T, String), key As T) As Boolean
        Return d.ContainsKey(key) AndAlso Not String.IsNullOrEmpty(d(key))
    End Function

    <Extension>
    Public Function ToDictionary(nc As NameValueCollection) As Dictionary(Of String, String)
        Dim hash As New Dictionary(Of String, String)

        For Each key As String In nc.AllKeys
            hash(key) = nc(key)
        Next

        Return hash
    End Function

    <Extension>
    Public Function Sort(Of T)(source As IEnumerable(Of T)) As IEnumerable(Of T)
        Return From x As T
               In source
               Select x
               Order By x Ascending
    End Function

    ''' <summary>
    ''' Creates a <see cref="System.Collections.Generic.Dictionary"/>`2 from an <see cref="System.Collections.Generic.IEnumerable"/>`1
    ''' according to a specified key selector function.
    ''' </summary>
    ''' <typeparam name="T">Unique identifier provider</typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ToDictionary(Of T As sIdEnumerable)(source As IEnumerable(Of T)) As Dictionary(Of T)
        Dim hash As Dictionary(Of T) = New Dictionary(Of T)
        Dim i As Integer = 0

        If source Is Nothing Then
            Call VBDebugger.Warning("Source is nothing, returns empty dictionary table!")
            Return hash
        End If

        Try
            For Each item As T In source
                Call hash.Add(item.Identifier, item)
                i += 1
            Next
        Catch ex As Exception
            ex = New Exception("Identifier -> [ " & source(i).Identifier & " ]", ex)
            Throw ex
        End Try

        Return hash
    End Function

    <Extension> Public Function Add(Of TKey, TValue)(ByRef list As List(Of KeyValuePair(Of TKey, TValue)), key As TKey, value As TValue) As List(Of KeyValuePair(Of TKey, TValue))
        If list Is Nothing Then
            list = New List(Of KeyValuePair(Of TKey, TValue))
        End If
        list += New KeyValuePair(Of TKey, TValue)(key, value)
        Return list
    End Function

    ''' <summary>
    ''' Adds an object to the end of the List`1.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="key"></param>
    ''' <param name="value"></param>
    ''' <returns></returns>
    <Extension> Public Function Add(Of TKey, TValue)(ByRef list As List(Of KeyValuePairObject(Of TKey, TValue)), key As TKey, value As TValue) As List(Of KeyValuePairObject(Of TKey, TValue))
        If list Is Nothing Then
            list = New List(Of KeyValuePairObject(Of TKey, TValue))
        End If
        list += New KeyValuePairObject(Of TKey, TValue)(key, value)
        Return list
    End Function
End Module
