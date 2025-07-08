#Region "Microsoft.VisualBasic::f2e1e5034f596a4622965721814163d5, mime\application%json\Javascript\JsonArray.vb"

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

    '   Total Lines: 164
    '    Code Lines: 118 (71.95%)
    ' Comment Lines: 21 (12.80%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 25 (15.24%)
    '     File Size: 5.70 KB


    '     Class JsonArray
    ' 
    '         Properties: FirstAndLast, Length, UnderlyingType
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: AsObjects, ContainsElement, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Add, Insert, Remove
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports i32 = Microsoft.VisualBasic.Language.i32

Namespace Javascript

    Public Class JsonArray : Inherits JsonModel
        Implements IEnumerable(Of JsonElement)

        Friend ReadOnly list As New List(Of JsonElement)

        ''' <summary>
        ''' the element count in current json array 
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            Get
                Return list.Count
            End Get
        End Property

        ''' <summary>
        ''' get a tuple of array data which is the first element and last element value in this array seperatelly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FirstAndLast As (first As JsonElement, last As JsonElement)
            Get
                If list.IsNullOrEmpty Then
                    Return Nothing
                Else
                    Return (list.First, list.Last)
                End If
            End Get
        End Property

        ''' <summary>
        ''' try to measure of the array base element type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property UnderlyingType As Type
            Get
                If list.IsNullOrEmpty Then
                    Return GetType(Object)
                ElseIf list.All(Function(o) TypeOf o Is JsonValue) Then
                    Return list _
                        .Select(Function(o) DirectCast(o, JsonValue)) _
                        .Where(Function(o) Not o.IsLiteralNull) _
                        .Select(Function(o) o.UnderlyingType) _
                        .GroupBy(Function(a) a) _
                        .OrderByDescending(Function(a) a.Count) _
                        .First _
                        .Key
                Else
                    Return GetType(Object)
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets/Set elements by index
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Overloads Property Item(index As Integer) As JsonElement
            Get
                Return list(index)
            End Get
            Set(value As JsonElement)
                list(index) = value
            End Set
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(objs As IEnumerable(Of JsonElement))
            list = objs.SafeQuery.ToList
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(values As IEnumerable(Of String))
            Call Me.New(values.SafeQuery.Select(Function(str) New JsonValue(str)))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(values As IEnumerable(Of Double))
            Call Me.New(values.Select(Function(d) New JsonValue(d)))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(values As IEnumerable(Of Integer))
            Call Me.New(values.Select(Function(d) New JsonValue(d)))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(element As JsonElement)
            Call list.Add(element)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Insert(index As Integer, element As JsonElement)
            Call list.Insert(index, element)
        End Sub

        Public Sub Remove(index As Integer)
            list.RemoveAt(index)
        End Sub

        Public Function ContainsElement(element As JsonElement) As Boolean
            Return list.Contains(element)
        End Function

        ''' <summary>
        ''' directcast this json array as a collection of the json object
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function AsObjects() As IEnumerable(Of JsonObject)
            If list.IsNullOrEmpty Then
                Return
            End If

            For Each eli As JsonElement In list
                Yield DirectCast(eli, JsonObject)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return "JsonArray: {count: " & list.Count & "}"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of JsonElement) Implements IEnumerable(Of JsonElement).GetEnumerator
            For Each eli As JsonElement In list
                Yield eli
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Overloads Shared Narrowing Operator CType(array As JsonArray) As String()
            If array Is Nothing Then
                Return {}
            End If

            Dim list As String() = New String(array.Length - 1) {}
            Dim i As i32 = 0

            For Each el As JsonElement In array.list
                If TypeOf el Is JsonValue Then
                    list(++i) = DirectCast(el, JsonValue)
                ElseIf el Is Nothing Then
                    list(++i) = Nothing
                Else
                    list(++i) = el.ToString
                End If
            Next

            Return list
        End Operator
    End Class
End Namespace
