#Region "Microsoft.VisualBasic::3156107ff56e7a160196e5a79d4048c8, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Terminal\PrintAsTable.vb"

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

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Terminal

    Public Module PrintAsTable

        <Extension>
        Public Function Print(Of T)(source As IEnumerable(Of T), Optional addFrame As Boolean = True) As String
            Dim out As New StringBuilder
            Dim dev As New StringWriter(out)
            Call source.Print(dev, addFrame)
            Return out.ToString
        End Function

        <Extension>
        Public Sub Print(Of T)(source As IEnumerable(Of T), Optional dev As TextWriter = Nothing, Optional addFrame As Boolean = True)
            Dim schema = LinqAPI.Exec(Of BindProperty(Of DataFrameColumnAttribute)) _
 _
                () <= From x As BindProperty(Of DataFrameColumnAttribute)
                      In DataFrameColumnAttribute _
                          .LoadMapping(Of T)(mapsAll:=True) _
                          .Values
                      Where x.IsPrimitive
                      Select x
            Dim titles As String() = schema.ToArray(Function(x) x.Identity)
            Dim contents = LinqAPI.Exec(Of Dictionary(Of String, String)) _
 _
                () <= From x As T
                      In source
                      Select (From p As BindProperty(Of DataFrameColumnAttribute)
                              In schema
                              Select p,
                                  s = p.GetValue(x)) _
                          .ToDictionary(Function(o) o.p.Identity,
                                        Function(o) Scripting.ToString(o.s))
            Dim table$()() = contents _
                .Select(Function(line)
                            Return titles.Select(Function(name) line(name)).ToArray
                        End Function) _
                .ToArray

            If addFrame Then
                Call table.PrintTable(dev, sep:=" "c)
            Else
                Call table.Print(dev, sep:=" "c)
            End If
        End Sub

        ''' <summary>
        ''' 与函数<see cref="Print"/>所不同的是，这个函数还会添加边框
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="dev"></param>
        ''' <param name="sep"></param>
        <Extension>
        Public Sub PrintTable(source As IEnumerable(Of String()), Optional dev As TextWriter = Nothing, Optional sep As Char = " "c)

        End Sub

        <Extension>
        Public Sub Print(source As IEnumerable(Of String()), Optional dev As TextWriter = Nothing, Optional sep As Char = " "c)
            With dev Or Console.Out.AsDefault

                Dim table$()() = source.ToArray
                Dim maxLen As New List(Of Integer)
                Dim width% = table.Max(Function(row) row.Length)
                Dim index%
                Dim offset%

                ' 按照列计算出layout偏移量
                For i As Integer = 0 To width - 1
                    index = i
                    maxLen += table _
                        .Select(Function(row) row.ElementAtOrDefault(index)) _
                        .Select(Function(s)
                                    If String.IsNullOrEmpty(s) Then
                                        Return 0
                                    Else
                                        Return s.Length
                                    End If
                                End Function) _
                        .Max
                Next

                For Each row As String() In table
                    offset = 0

                    For i As Integer = 0 To width - 1
                        Call .Write(New String(sep, offset) & row(i))
                        offset = maxLen(i) - row(i).Length
                    Next

                    Call .WriteLine()
                Next

                Call .Flush()
            End With
        End Sub
    End Module
End Namespace
