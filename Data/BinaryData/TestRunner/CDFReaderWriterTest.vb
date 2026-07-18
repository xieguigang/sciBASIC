#Region "Microsoft.VisualBasic::f6c63e04faa5fa22f46f6c45475f576f, Data\BinaryData\TestRunner\CDFReaderWriterTest.vb"

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

    '   Total Lines: 104
    '    Code Lines: 79 (75.96%)
    ' Comment Lines: 11 (10.58%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 14 (13.46%)
    '     File Size: 4.77 KB


    '     Module CDFReaderWriterTest
    ' 
    '         Function: RoundTripStandard, RunAll
    ' 
    '         Sub: Check
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector

    ''' <summary>
    ''' Round-trip tests for the netCDF reader/writer. They exercise the
    ''' standard-compliant writer (version selection, 4-byte padding, record
    ''' variable interleaving, 64-bit offsets) and the reader (dimension-product
    ''' based element counts, record interleaving, extended types).
    ''' </summary>
    Module CDFReaderWriterTest

        ''' <summary>
        ''' Run every test and return the list of failure messages (empty = all pass).
        ''' </summary>
        Public Function RunAll() As String()
            Dim failures As New List(Of String)
            Call failures.AddRange(RoundTripStandard)
            Return failures.ToArray
        End Function

        Private Function RoundTripStandard() As IEnumerable(Of String)
            Dim filePath = Path.GetTempFileName()
            Dim failures As New List(Of String)

            Try
                Dim lat = New Dimension("lat", 3)
                Dim lon = New Dimension("lon", 4)
                Dim time = New Dimension("time", 0) ' record (unlimited) dimension

                Dim grid = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}
                Dim dbl = {1.5, 2.5, 3.5}
                Dim sh = {CShort(10), CShort(20), CShort(30)}
                Dim lg = {100L, 200L, 300L}
                Dim by = {CByte(1), CByte(2), CByte(3)}
                Dim ch = {"a"c, "b"c, "c"c}
                Dim fl = {CSng(1.25), CSng(2.25), CSng(3.25)}
                Dim bo = {True, False, True}
                ' BOOLEAN is stored as NC_BYTE, so it round-trips as bytes 0/1.
                Dim boBytes = {CByte(1), CByte(0), CByte(1)}
                ' record variable temp(time, lat): 2 records x 3 lat = 6 elements
                Dim temp = {1, 2, 3, 4, 5, 6}

                Using w As New CDFWriter(filePath)
                    Call w.Dimensions(lat, lon, time)
                    Call w.AddVariable("grid", CType(grid, integers), {lat, lon})
                    Call w.AddVariable("dbl", CType(dbl, doubles), lat)
                    Call w.AddVariable("sh", CType(sh, shorts), lat)
                    Call w.AddVariable("lg", CType(lg, longs), lat)
                    Call w.AddVariable("by", CType(by, bytes), lat)
                    Call w.AddVariable("ch", CType(ch, chars), lat)
                    Call w.AddVariable("fl", CType(fl, floats), lat)
                    Call w.AddVariable("bo", CType(bo, flags), lat)
                    Call w.AddVariable("temp", CType(temp, integers), {time, lat})
                End Using

                Using r As New netCDFReader(filePath)
                    Call Check(r, "grid", grid, failures)
                    Call Check(r, "dbl", dbl, failures)
                    Call Check(r, "sh", sh, failures)
                    Call Check(r, "lg", lg, failures)
                    Call Check(r, "by", by, failures)
                    Call Check(r, "ch", ch, failures)
                    Call Check(r, "fl", fl, failures)
                    Call Check(r, "bo", bo, failures)
                    Call Check(r, "temp", temp, failures)

                    If r.recordDimension.length <> 2 Then
                        failures.Add($"recordDimension.length = {r.recordDimension.length} <> 2")
                    End If

                    If r.version <> "64-bit data format (CDF-5)" Then
                        failures.Add("expected CDF-5 file version, got: " & r.version)
                    End If
                End Using
            Catch ex As Exception
                Call failures.Add("exception: " & ex.ToString)
            Finally
                If File.Exists(filePath) Then
                    Call File.Delete(filePath)
                End If
            End Try

            Return failures
        End Function

        Private Sub Check(Of T)(r As netCDFReader, name As String, expected As T(), failures As List(Of String))
            Dim v = r.getDataVariable(name)
            Dim actual = DirectCast(v.genericValue, T())

            If actual.Length <> expected.Length Then
                Call failures.Add($"{name}: length mismatch {actual.Length} <> {expected.Length}")
                Return
            End If

            For i As Integer = 0 To expected.Length - 1
                If Not actual(i).Equals(expected(i)) Then
                    Call failures.Add($"{name}[{i}] = {actual(i)} <> {expected(i)}")
                End If
            Next
        End Sub
    End Module


