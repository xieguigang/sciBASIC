#Region "Microsoft.VisualBasic::b8db10a83d8a54b66fe1216750ceff24, Data_science\Mathematica\SignalProcessing\Signal.IO\SignalsWriter.vb"

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

    '   Total Lines: 160
    '    Code Lines: 119
    ' Comment Lines: 14
    '   Blank Lines: 27
    '     File Size: 8.02 KB


    ' Module SignalsWriter
    ' 
    '     Function: createAttributes, WriteCDF
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.DataStorage.netCDF.Data
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' cdf writer of the signals
''' </summary>
Public Module SignalsWriter

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="signals"></param>
    ''' <param name="file"></param>
    ''' <param name="description"></param>
    ''' <param name="enableCDFExtension">enable netCDF extension type?</param>
    ''' <returns></returns>
    <Extension>
    Public Function WriteCDF(signals As IEnumerable(Of GeneralSignal), file As String,
                             Optional description$ = Nothing,
                             Optional enableCDFExtension As Boolean = False) As Boolean

        ' 20200704 redesign of the general signal cdf file storage layout:
        ' the previous version of the signal data file is too slow that reading 
        ' in R script code when the signal data count is large

        Using cdffile As New CDFWriter(file)
            Call cdffile.GlobalAttributes(New attribute With {.name = "time", .type = CDFDataTypes.NC_CHAR, .value = Now.ToString})
            Call cdffile.GlobalAttributes(New attribute With {.name = "filename", .type = CDFDataTypes.NC_CHAR, .value = file.FileName})
            Call cdffile.GlobalAttributes(New attribute With {.name = "github", .type = CDFDataTypes.NC_CHAR, .value = LICENSE.githubURL})

            If Not description.StringEmpty Then
                Call cdffile.GlobalAttributes(New attribute With {.name = NameOf(description), .type = CDFDataTypes.NC_CHAR, .value = description})
            End If

            Dim package As GeneralSignal() = signals.ToArray
            Dim chunksize As New List(Of Integer)
            Dim offsets As New List(Of Long)
            Dim dataDimension As Dimension
            Dim attrNames As New List(Of String)
            Dim measures As New List(Of Double)
            Dim signalDatas As New List(Of Double)
            Dim annotation As attribute

            Call cdffile.GlobalAttributes(New attribute With {.name = "signals", .type = CDFDataTypes.NC_INT, .value = package.Length})

            For Each attr As NamedValue(Of ICDFDataVector) In package.createAttributes(enableCDFExtension)
                dataDimension = New Dimension With {
                    .name = "attribute_data: " & attr.Name,
                    .size = attr.Value.length
                }
                annotation = New attribute With {
                    .name = "type",
                    .type = CDFDataTypes.NC_CHAR,
                    .value = attr.Description
                }

                Call cdffile.AddVariable("meta:" & attr.Name, attr.Value, dataDimension, {annotation})
                Call attrNames.Add(attr.Name)
            Next

            Call cdffile.GlobalAttributes(New attribute With {.name = "metadata", .type = CDFDataTypes.NC_CHAR, .value = attrNames.AsEnumerable.GetJson})

            Dim bufferOffset As Long = Scan0
            Dim signal_guid As ICDFDataVector = CType(signals.Select(Function(sig) sig.reference).GetJson, chars)
            Dim units As ICDFDataVector = CType(signals.Select(Function(sig) sig.measureUnit).GetJson, chars)

            For Each signal As GeneralSignal In signals
                measures.AddRange(signal.Measures)
                signalDatas.AddRange(signal.Strength)
                offsets.Add(bufferOffset)
                chunksize.Add(signal.Measures.Length)

                bufferOffset += signal.Measures.Length
            Next

            dataDimension = New Dimension With {.name = "data_chunks", .size = measures.Count}

            Call cdffile.AddVariable("measure_buffer", CType(measures.ToArray, doubles), dataDimension)
            Call cdffile.AddVariable("signal_buffer", CType(signalDatas.ToArray, doubles), dataDimension)

            dataDimension = New Dimension With {.name = "signals", .size = chunksize.Count}
            cdffile.AddVariable("chunk_size", CType(chunksize.ToArray, integers), dataDimension)

            If enableCDFExtension Then
                Call cdffile.AddVariable("buffer_offset", CType(offsets.ToArray, longs), dataDimension)
            End If

            dataDimension = New Dimension With {.name = "guid_json_chars", .size = signal_guid.length}
            cdffile.AddVariable("signal_guid", signal_guid, dataDimension)

            dataDimension = New Dimension With {.name = "measure_unit_json_chars", .size = units.length}
            cdffile.AddVariable("measure_unit", units, dataDimension)
        End Using

        Return True
    End Function

    <Extension>
    Private Iterator Function createAttributes(package As GeneralSignal(), enableCDFExtension As Boolean) As IEnumerable(Of NamedValue(Of ICDFDataVector))
        Dim allNames As String() = package.Select(Function(sig) sig.meta.Keys).IteratesALL.Distinct.ToArray

        For Each name As String In allNames
            Dim values As New List(Of String)
            Dim data As ICDFDataVector
            Dim type As String

            For Each signal As GeneralSignal In package
                values.Add(signal.meta.TryGetValue(name, [default]:=""))
            Next

            If values.AsParallel.Select(Function(s) s Is Nothing OrElse s = "" OrElse s.IsPattern("\d+")).All(Function(t) t = True) Then
                Dim longs = values.Select(AddressOf Long.Parse).ToArray

                If longs.All(Function(b) b <= 255 AndAlso b >= -255) Then
                    data = CType(longs.Select(Function(l) CByte(l)).ToArray, bytes)
                    type = "base64"
                ElseIf longs.All(Function(s) s <= Short.MaxValue AndAlso s >= Short.MinValue) Then
                    data = CType(longs.Select(Function(l) CShort(l)).ToArray, shorts)
                    type = "int16"
                ElseIf longs.All(Function(i) i <= Integer.MaxValue AndAlso i >= Integer.MinValue) Then
                    data = CType(longs.Select(Function(l) CInt(l)).ToArray, integers)
                    type = "i32"
                ElseIf enableCDFExtension Then
                    data = CType(longs, longs)
                    type = "i64"
                Else
                    Throw New InvalidProgramException($"{GetType(Long).FullName} is not supports when option '{NameOf(enableCDFExtension)}' is not enable!")
                End If

            ElseIf values.AsParallel.Select(Function(s) s Is Nothing OrElse s = "" OrElse s.IsNumeric).All(Function(t) t = True) Then
                data = CType(values.Select(AddressOf ParseDouble).ToArray, doubles)
                type = "f64"
            ElseIf values.AsParallel.Select(Function(s) s Is Nothing OrElse s = "" OrElse s.IsPattern("((true)|(false))", RegexICSng)).All(Function(t) t = True) Then
                If enableCDFExtension Then
                    data = CType(values.Select(AddressOf ParseBoolean).ToArray, flags)
                    type = "flags"
                Else
                    data = CType(values.Select(AddressOf ParseBoolean).Select(Function(f) CShort(If(f, 1, 0))).ToArray, shorts)
                    type = "i16"
                End If
            Else
                data = CType(values.AsEnumerable.GetJson, chars)
                type = "json"
            End If

            Yield New NamedValue(Of ICDFDataVector) With {
                .Name = name,
                .Value = data,
                .Description = type
            }
        Next
    End Function
End Module
