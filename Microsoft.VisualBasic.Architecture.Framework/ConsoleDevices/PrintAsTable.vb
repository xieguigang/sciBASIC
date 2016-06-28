#Region "c652bc0eb7945add88233f87c7fe9385, ..\Microsoft.VisualBasic.Architecture.Framework\ConsoleDevices\PrintAsTable.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Terminal

    Public Module PrintAsTable

        <Extension>
        Public Function Print(Of T)(source As IEnumerable(Of T)) As String
            If source Is Nothing Then
                Return ""
            End If

            Dim totalWidth As Integer = Console.WindowWidth
            Dim schema As BindProperty(Of DataFrameColumnAttribute)() =
                LinqAPI.Exec(Of BindProperty(Of DataFrameColumnAttribute)) <=
                    From x As BindProperty(Of DataFrameColumnAttribute)
                    In DataFrameColumnAttribute.LoadMapping(Of T)(mapsAll:=True).Values
                    Where x.IsPrimitive
                    Select x
            Dim titles As String() = schema.ToArray(Function(x) x.Identity)
            Dim table =
                LinqAPI.Exec(Of Dictionary(Of String, String)) <=
                    From x As T
                    In source
                    Select (From p As BindProperty(Of DataFrameColumnAttribute)
                            In schema
                            Select p,
                                s = p.GetValue(x)) _
                               .ToDictionary(Function(o) o.p.Identity,
                                             Function(o) Scripting.ToString(o.s))
            Dim maxLens As Integer() =
                LinqAPI.Exec(Of Integer) <= From i As SeqValue(Of String)
                                            In titles.SeqIterator
                                            Select Math.Max(i.obj.Length,
                                                (From x As Dictionary(Of String, String)
                                                 In table
                                                 Select Len(x.Values(i.i))).Max)

            Dim hr As String = New String("="c, totalWidth)
            Dim sb As New StringBuilder
            Dim d As Integer = (totalWidth - maxLens.Sum) / titles.Length - 1

            If d < 0 Then
                d = 0
            End If

            Call sb.AppendLine(hr)
            Call sb.__appendLine(maxLens, titles, d)
            Call sb.AppendLine(hr)

            For Each line As Dictionary(Of String, String) In table
                Call sb.__appendLine(
                maxLens,
                line.Values.ToArray,
                d)
            Next

            Call Console.WriteLine(sb.ToString)

            Return sb.ToString
        End Function

        <Extension>
        Private Sub __appendLine(ByRef sb As StringBuilder,
                                 maxlens As Integer(),
                                 values As String(),
                                 d As Integer)

            For i As Integer = 0 To values.Length - 2
                Dim s As String = values(i)
                Call sb.Append(s)
                Call sb.Append(New String(" "c, maxlens(i) - Len(s) + d))
            Next

            ' 最后一列是右对齐的
            Dim right As Integer = maxlens(values.Length - 1) - Len(values.Last) + d

            Call sb.Append(New String(" "c, right))
            Call sb.Append(values.Last)
        End Sub
    End Module
End Namespace
