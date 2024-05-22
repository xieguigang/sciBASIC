#Region "Microsoft.VisualBasic::611faf49792ea19832576432b5d3127b, Data_science\Mathematica\SignalProcessing\Signal.IO\SignalsReader.vb"

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

    '   Total Lines: 66
    '    Code Lines: 57 (86.36%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (13.64%)
    '     File Size: 3.10 KB


    ' Module SignalsReader
    ' 
    '     Function: ReadCDF
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.netCDF
Imports Microsoft.VisualBasic.DataStorage.netCDF.DataVector
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module SignalsReader

    <Extension>
    Public Function ReadCDF(file As netCDFReader) As IEnumerable(Of GeneralSignal)
        Dim signals As Integer = file.getAttribute("signals")
        Dim metaNames = file.getAttribute("metadata").ToString.LoadJSON(Of String())
        Dim description = file.getAttribute("description").ToString
        Dim x As New Vector(Of Double)(DirectCast(file.getDataVariable("measure_buffer"), doubles).Array)
        Dim y As New Vector(Of Double)(DirectCast(file.getDataVariable("signal_buffer"), doubles).Array)
        Dim chunk_size = file.getDataVariable("chunk_size")
        Dim signal_guid = DirectCast(file.getDataVariable("signal_guid"), chars).LoadJSON(Of String())
        Dim measure_unit = DirectCast(file.getDataVariable("measure_unit"), chars).LoadJSON(Of String())
        Dim index As New List(Of (start%, ends%))
        Dim buffer_size = 0
        Dim meta As Array() = metaNames _
            .Select(Function(str) "meta:" & str) _
            .Select(Function(name, idx)
                        Dim data = file.getDataVariable(name)
                        Dim info = file.getDataVariableEntry(name)
                        Dim retriveVal As Array

                        If info.FindAttribute("type").value = "json" Then
                            retriveVal = DirectCast(data, chars).LoadJSON(Of String())
                        Else
                            retriveVal = DirectCast(data.genericValue, IEnumerable).ToArray(Of Object)
                        End If

                        Return retriveVal
                    End Function) _
            .ToArray

        For Each size In DirectCast(chunk_size, integers)
            index.Add((buffer_size, buffer_size + size - 1))
            buffer_size = buffer_size + size
        Next

        Return index _
            .Select(Function(range, idx)
                        Dim measure = x(range)
                        Dim signal = y(range)
                        Dim metadata As New Dictionary(Of String, String)

                        For i As Integer = 0 To metaNames.Length - 1
                            Call metadata.Add(metaNames(i), meta(i).GetValue(idx).ToString)
                        Next

                        Return New GeneralSignal With {
                            .description = description,
                            .Measures = measure,
                            .Strength = signal,
                            .meta = metadata,
                            .measureUnit = measure_unit(idx),
                            .reference = signal_guid(idx)
                        }
                    End Function)
    End Function

End Module
