#Region "Microsoft.VisualBasic::745f8702351cf36b991ed4657c0bb62d, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\PrintAsTable.vb"

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

    '   Total Lines: 268
    '    Code Lines: 199 (74.25%)
    ' Comment Lines: 33 (12.31%)
    '    - Xml Docs: 96.97%
    ' 
    '   Blank Lines: 36 (13.43%)
    '     File Size: 11.24 KB


    '     Module PrintAsTable
    ' 
    '         Function: (+3 Overloads) Print
    ' 
    '         Sub: Print, PrintTable
    '         Delegate Sub
    ' 
    '             Sub: (+3 Overloads) Print, printInternal
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices.Terminal

    Public Module PrintAsTable

        ''' <summary>
        ''' Returns a text string 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="addBorder"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Print(Of T)(source As IEnumerable(Of T), Optional addBorder As Boolean = True) As String
            Dim out As New StringBuilder
            Dim dev As New StringWriter(out)
            Call source.Print(dev, addBorder)
            Return out.ToString
        End Function

        <Extension>
        Public Function Print(source As IEnumerable(Of IEnumerable(Of String)), Optional addBorder As Boolean = True) As String
            Return source.Select(Function(a) a.ToArray).Print(addBorder)
        End Function

        <Extension>
        Public Function Print(source As IEnumerable(Of String()), Optional addBorder As Boolean = True) As String
            Dim out As New StringBuilder
            Dim dev As New StringWriter(out)

            If addBorder Then
                Call source.PrintTable(dev, sep:=" "c)
            Else
                Call source.Print(dev, sep:=" "c)
            End If

            Return out.ToString
        End Function

        ''' <summary>
        ''' Print the object collection as table on the console 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="dev"></param>
        ''' <param name="addBorder"></param>
        <Extension>
        Public Sub Print(Of T)(source As IEnumerable(Of T), dev As TextWriter, Optional addBorder As Boolean = True)
            Dim schema = LinqAPI.Exec(Of BindProperty(Of DataFrameColumnAttribute)) _
 _
                () <= From x As BindProperty(Of DataFrameColumnAttribute)
                      In DataFrameColumnAttribute _
                          .LoadMapping(Of T)(mapsAll:=True) _
                          .Values
                      Where x.IsPrimitive
                      Select x

            Dim titles As String() = schema.Select(Function(x) x.Identity).ToArray
            Dim contents = LinqAPI.Exec(Of Dictionary(Of String, String)) _
 _
                () <= From x As T
                      In source
                      Select (From p As BindProperty(Of DataFrameColumnAttribute)
                              In schema
                              Select p,
                                  s = p.GetValue(x)) _
                          .ToDictionary(Function(o) o.p.Identity,
                                        Function(o)
                                            Return Scripting.ToString(o.s)
                                        End Function)

            Dim table As List(Of String()) = contents _
                .Select(Function(line)
                            Return titles.Select(Function(name) line(name)).ToArray
                        End Function) _
                .AsList

            If addBorder Then
                Call (titles + table).PrintTable(dev, sep:=" "c)
            Else
                Call (titles + table).Print(dev, sep:=" "c)
            End If
        End Sub

        ''' <summary>
        ''' 与函数<see cref="Print"/>所不同的是，这个函数还会添加边框
        ''' </summary>
        ''' <param name="source">Each element is a row in table matrix</param>
        ''' <param name="dev"></param>
        ''' <param name="sep"></param>
        <Extension>
        Public Sub PrintTable(source As IEnumerable(Of String()),
                              Optional dev As TextWriter = Nothing,
                              Optional sep As Char = " "c,
                              Optional title$() = Nothing,
                              Optional trilinearTable As Boolean = False,
                              Optional leftMargin% = 0)

            Dim printHead As Boolean = False
            Dim table$()() = source.ToArray
            Dim printOfHead As printOnDevice =
                Sub(row, width, maxLen, device)

                    If leftMargin > 0 Then
                        Call device.Write(New String(sep, leftMargin))
                    End If

                    Call device.Write("+")
                    Call device.Write(maxLen.Select(Function(l) New String("-"c, l)).JoinBy("+"))
                    Call device.Write("+")
                    Call device.WriteLine()
                End Sub

            If Not title Is Nothing Then
                table = title.Join(table).ToArray
            End If

            Call table.printInternal(
                dev, 2, Sub(row, width, maxLen, device)
                            Dim offset% = 0

                            If Not printHead Then
                                Call printOfHead(Nothing, width, maxLen, device)
                            End If
                            If leftMargin > 0 Then
                                Call device.Write(New String(sep, leftMargin))
                            End If
                            If Not trilinearTable Then
                                Call device.Write("|")
                            End If

                            For i As Integer = 0 To width - 1
                                If row(i) Is Nothing Then
                                    row(i) = ""
                                End If

                                If trilinearTable Then
                                    device.Write(" ")
                                End If

                                offset = maxLen(i) - row(i).Length - 1
                                device.Write(" " & row(i) & New String(sep, offset))

                                If Not trilinearTable Then
                                    Call device.Write("|")
                                End If
                            Next

                            Call device.WriteLine()

                            If Not printHead Then
                                Call printOfHead(Nothing, width, maxLen, device)
                                printHead = True
                            End If
                        End Sub, printOfHead)
        End Sub

        Private Delegate Sub printOnDevice(row$(), width%, maxLen%(), device As TextWriter)

        <Extension>
        Private Sub printInternal(table$()(), dev As TextWriter, distance%, printLayout As printOnDevice, Optional final As printOnDevice = Nothing)
            With dev Or Console.Out.AsDefault
                Dim width% = table.Max(Function(row) row.Length)
                Dim index%
                Dim maxLen%() = New Integer(width - 1) {}

                ' 按照列计算出layout偏移量
                For i As Integer = 0 To width - 1
                    index = i
                    maxLen(index) = table _
                        .Select(Function(row) row.ElementAtOrDefault(index)) _
                        .Select(Function(s)
                                    If String.IsNullOrEmpty(s) Then
                                        Return 0
                                    Else
                                        Return s.Length
                                    End If
                                End Function) _
                        .Max + distance
                Next

                For Each row As String() In table
                    Call printLayout(row, width, maxLen, .ByRef)
                Next

                If Not final Is Nothing Then
                    Call final(Nothing, width, maxLen, .ByRef)
                End If

                Call .Flush()
            End With
        End Sub

        ''' <summary>
        ''' Print the string matrix collection <paramref name="source"/> in table layout.
        ''' </summary>
        ''' <param name="source">The string matrix collection.</param>
        ''' <param name="dev">The output device</param>
        ''' <param name="sep"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub Print(source As IEnumerable(Of String()), Optional dev As TextWriter = Nothing, Optional sep As Char = " "c, Optional distance% = 2)
            Call source _
                .ToArray _
                .printInternal(
                    dev, distance, Sub(row, width, maxLen, device)
                                       Dim offset% = 0

                                       For i As Integer = 0 To width - 1
                                           If row(i) Is Nothing Then
                                               row(i) = ""
                                           End If

                                           device.Write(New String(sep, offset) & row(i))
                                           offset = maxLen(i) - row(i).Length
                                       Next

                                       Call device.WriteLine()
                                   End Sub)
        End Sub

        ''' <summary>
        ''' Print the string dictionary as table
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="dev"></param>
        ''' <param name="sep"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub Print(table As Dictionary(Of String, String),
                         Optional dev As TextWriter = Nothing,
                         Optional sep As Char = " "c,
                         Optional distance% = 2)
            Call {
                New String() {"Item", "Value"}
            } _
            .Join(table.Select(Function(map) {map.Key, map.Value})) _
            .Print(dev, sep, distance)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub Print(data As IEnumerable(Of NamedValue(Of String)),
                         Optional dev As TextWriter = Nothing,
                         Optional sep As Char = " "c,
                         Optional trilinearTable As Boolean = False,
                         Optional leftMargin% = 0)
            Call {
                New String() {"Name", "Value", "Description"}
            } _
            .Join(data.Select(Function(item)
                                  Return {item.Name, item.Value, item.Description}
                              End Function)) _
            .PrintTable(
                dev,
                sep,
                trilinearTable:=trilinearTable,
                leftMargin:=leftMargin)
        End Sub
    End Module
End Namespace
