#Region "Microsoft.VisualBasic::f9a3b6d84c1e4b7a9d2c5e08b6f17a34, vs_solutions\JavaScript\Runtime\ConsoleTableFormatter.vb"

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

    '     Module ConsoleTableFormatter
    '
    '         Function: AllAre, BuildFromArray, BuildFromObject, Cell, EnumerableKeys
    '                   Flatten, IndexColumns, Project, Render
    '
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags

Namespace Runtime

    ''' <summary>
    ''' <c>console.table(tabularData, columns)</c> rendering on top of
    ''' <see cref="ConsoleTableBuilder"/>.
    ''' <para>
    ''' Supported inputs, matching the browser behaviour:
    ''' <list type="bullet">
    ''' <item>array of objects → one row per element, columns are the union of the
    ''' element keys in first-appearance order;</item>
    ''' <item>array of arrays → columns named by index;</item>
    ''' <item>array of scalars → <c>(index)</c> plus <c>Values</c>;</item>
    ''' <item>object of objects / arrays / scalars → <c>(index)</c> holds the outer
    ''' key, the remaining columns come from the values (Node's <c>console.table</c>
    ''' behaviour for object literals).</item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' The optional second argument is a column selection: the <c>(index)</c> column
    ''' is kept and the requested columns are emitted in the given order. Returns
    ''' <c>Nothing</c> for input that is not tabular, so the caller can fall back to
    ''' the plain <c>console.log</c> rendering.
    ''' </para>
    ''' </summary>
    Public Module ConsoleTableFormatter

        ''' <summary>Header of the leading key column of an object / scalar list.</summary>
        Public Const IndexHeader As String = "(index)"

        ''' <summary>Header used for the values of a scalar list (Node compatible).</summary>
        Public Const ValuesHeader As String = "Values"

        Public Function Render(data As Object, columns As List(Of String)) As String
            Dim header As List(Of String) = Nothing
            Dim rows As New List(Of Object())()

            If JsRuntime.IsJsArray(data) Then
                If Not BuildFromArray(DirectCast(data, List(Of Object)), header, rows) Then Return Nothing
            ElseIf JsRuntime.IsJsObject(data) Then
                If Not BuildFromObject(DirectCast(data, Dictionary(Of String, Object)), header, rows) Then Return Nothing
            Else
                Return Nothing
            End If

            If header Is Nothing OrElse header.Count = 0 OrElse rows.Count = 0 Then Return Nothing

            If columns IsNot Nothing AndAlso columns.Count > 0 Then
                Dim projectedRows As List(Of Object()) = Nothing

                header = Project(header, rows, columns, projectedRows)

                If header Is Nothing Then Return Nothing

                rows = projectedRows
            End If

            Dim builder = ConsoleTableBuilder.From(rows)
            Dim text = builder.WithColumn(header.ToArray()) _
                              .WithFormat(ConsoleTableBuilderFormat.Default) _
                              .Export() _
                              .ToString() _
                              .TrimEnd()

            If text.Length = 0 Then Return Nothing

            Return text
        End Function

        ' ===================== input normalisation =====================

        Private Function BuildFromArray(items As List(Of Object), ByRef header As List(Of String), ByRef rows As List(Of Object())) As Boolean
            If items Is Nothing OrElse items.Count = 0 Then Return False

            If AllAre(items, AddressOf JsRuntime.IsJsObject) Then
                Dim names As New List(Of String)()

                For Each item In items
                    For Each key In EnumerableKeys(DirectCast(item, Dictionary(Of String, Object)))
                        If Not names.Contains(key) Then names.Add(key)
                    Next
                Next

                If names.Count = 0 Then Return False

                For i = 0 To items.Count - 1
                    Dim source = DirectCast(items(i), Dictionary(Of String, Object))
                    Dim row(names.Count) As Object

                    ' like Node / DevTools, array rows keep their position
                    row(0) = i.ToString(CultureInfo.InvariantCulture)

                    For j = 0 To names.Count - 1
                        row(j + 1) = Cell(source, names(j))
                    Next

                    rows.Add(row)
                Next

                header = New List(Of String) From {IndexHeader}
                header.AddRange(names)
                Return True
            End If

            If AllAre(items, AddressOf JsRuntime.IsJsArray) Then
                Dim width = 0

                For Each item In items
                    width = System.Math.Max(width, DirectCast(item, List(Of Object)).Count)
                Next

                If width = 0 Then Return False

                For i = 0 To items.Count - 1
                    Dim source = DirectCast(items(i), List(Of Object))
                    Dim row(width) As Object

                    row(0) = i.ToString(CultureInfo.InvariantCulture)

                    For j = 0 To width - 1
                        row(j + 1) = If(j < source.Count, Cell(source(j)), "")
                    Next

                    rows.Add(row)
                Next

                header = New List(Of String) From {IndexHeader}
                header.AddRange(IndexColumns(width))
                Return True
            End If

            ' array of scalars
            For i = 0 To items.Count - 1
                rows.Add(New Object() {i.ToString(CultureInfo.InvariantCulture), Cell(items(i))})
            Next

            header = New List(Of String) From {IndexHeader, ValuesHeader}
            Return True
        End Function

        Private Function BuildFromObject(source As Dictionary(Of String, Object),
                                        ByRef header As List(Of String),
                                        ByRef rows As List(Of Object())) As Boolean

            Dim keys = EnumerableKeys(source)

            If keys.Count = 0 Then Return False

            ' read through accessor properties and the prototype chain, like JS would
            Dim values As New List(Of Object)()

            For Each key In keys
                values.Add(JsRuntime.JsGet(source, key))
            Next

            If AllAre(values, AddressOf JsRuntime.IsJsObject) Then
                Dim names As New List(Of String)()

                For Each value In values
                    For Each key In EnumerableKeys(DirectCast(value, Dictionary(Of String, Object)))
                        If Not names.Contains(key) Then names.Add(key)
                    Next
                Next

                If names.Count = 0 Then Return False

                For i = 0 To keys.Count - 1
                    Dim inner = DirectCast(values(i), Dictionary(Of String, Object))
                    Dim row(names.Count) As Object

                    row(0) = keys(i)

                    For j = 0 To names.Count - 1
                        row(j + 1) = Cell(inner, names(j))
                    Next

                    rows.Add(row)
                Next

                header = New List(Of String) From {IndexHeader}
                header.AddRange(names)
                Return True
            End If

            If AllAre(values, AddressOf JsRuntime.IsJsArray) Then
                Dim width = 0

                For Each value In values
                    width = System.Math.Max(width, DirectCast(value, List(Of Object)).Count)
                Next

                If width = 0 Then Return False

                For i = 0 To keys.Count - 1
                    Dim inner = DirectCast(values(i), List(Of Object))
                    Dim row(width) As Object

                    row(0) = keys(i)

                    For j = 0 To width - 1
                        row(j + 1) = If(j < inner.Count, Cell(inner(j)), "")
                    Next

                    rows.Add(row)
                Next

                header = New List(Of String) From {IndexHeader}
                header.AddRange(IndexColumns(width))
                Return True
            End If

            ' object of scalars
            For i = 0 To keys.Count - 1
                rows.Add(New Object() {keys(i), Cell(values(i))})
            Next

            header = New List(Of String) From {IndexHeader, ValuesHeader}
            Return True
        End Function

        ''' <summary>Apply the <c>columns</c> selection of <c>console.table</c>.</summary>
        Private Function Project(header As List(Of String), rows As List(Of Object()),
                                 selected As List(Of String),
                                 ByRef projectedRows As List(Of Object())) As List(Of String)

            Dim lookup As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

            For i = 0 To header.Count - 1
                If Not lookup.ContainsKey(header(i)) Then lookup(header(i)) = i
            Next

            Dim chosen As New List(Of String)()

            ' the key column always stays first
            If header.Count > 0 AndAlso header(0) = IndexHeader Then chosen.Add(IndexHeader)

            For Each name In selected
                If lookup.ContainsKey(name) AndAlso Not chosen.Contains(name) Then chosen.Add(name)
            Next

            If chosen.Count = 0 Then Return Nothing

            Dim indices(chosen.Count - 1) As Integer

            For i = 0 To chosen.Count - 1
                indices(i) = lookup(chosen(i))
            Next

            projectedRows = New List(Of Object())()

            For Each row In rows
                Dim item(chosen.Count - 1) As Object

                For i = 0 To chosen.Count - 1
                    Dim index = indices(i)
                    item(i) = If(index < row.Length, row(index), "")
                Next

                projectedRows.Add(item)
            Next

            Return chosen
        End Function

        ' ===================== helpers =====================

        Private Function EnumerableKeys(source As Dictionary(Of String, Object)) As List(Of String)
            Dim keys As New List(Of String)()

            ' internal engine keys (__proto__ / __class__) are not enumerable, see JsForInKeys
            For Each key In source.Keys
                If key.StartsWith("__", StringComparison.Ordinal) Then Continue For

                keys.Add(key)
            Next

            Return keys
        End Function

        Private Function IndexColumns(width As Integer) As List(Of String)
            Dim names As New List(Of String)(width)

            For i = 0 To width - 1
                names.Add(i.ToString(CultureInfo.InvariantCulture))
            Next

            Return names
        End Function

        Private Function AllAre(items As List(Of Object), predicate As Func(Of Object, Boolean)) As Boolean
            For Each item In items
                If Not predicate(item) Then Return False
            Next

            Return True
        End Function

        Private Function Cell(value As Object) As String
            If value Is Nothing OrElse value Is JsRuntime.Undef Then Return ""

            Return Flatten(JsRuntime.JsDisplay(value))
        End Function

        Private Function Cell(source As Dictionary(Of String, Object), key As String) As String
            Return Cell(JsRuntime.JsGet(source, key))
        End Function

        ''' <summary>A cell must stay on one line, or the table borders will not align.</summary>
        Private Function Flatten(text As String) As String
            If String.IsNullOrEmpty(text) Then Return ""

            If text.IndexOf(ChrW(10)) < 0 AndAlso text.IndexOf(ChrW(13)) < 0 Then Return text

            Return text.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbCr, " ").Trim()
        End Function

    End Module

End Namespace
