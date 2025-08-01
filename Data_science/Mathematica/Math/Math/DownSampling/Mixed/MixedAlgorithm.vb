#Region "Microsoft.VisualBasic::31f16e51f76ff650a7b1b00d9d273395, Data_science\Mathematica\Math\Math\DownSampling\Mixed\MixedAlgorithm.vb"

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

    '   Total Lines: 48
    '    Code Lines: 34 (70.83%)
    ' Comment Lines: 3 (6.25%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (22.92%)
    '     File Size: 1.65 KB


    '     Class MixedAlgorithm
    ' 
    '         Function: process, ToString
    ' 
    '         Sub: add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports std = System.Math

Namespace DownSampling.Mixed


    ''' <summary>
    ''' Merge other algorithms' results into a new result, then deduplicate and sort it.
    ''' </summary>
    Public Class MixedAlgorithm
        Implements DownSamplingAlgorithm

        Private map As New Dictionary(Of DownSamplingAlgorithm, Double)()

        Public Overridable Sub add(da As DownSamplingAlgorithm, rate As Double)
            map.Add(da, rate)
        End Sub

        Public Overridable Function process(data As IList(Of ITimeSignal), threshold As Integer) As IList(Of ITimeSignal) Implements DownSamplingAlgorithm.process
            If map.Empty Then
                Return data
            End If
            Dim [set] As New HashSet(Of ITimeSignal)()
            For Each da As DownSamplingAlgorithm In map.Keys
                Dim subList As IList(Of ITimeSignal) = da.process(data, CInt(std.Truncate(threshold * map(da))))

                For Each item In subList
                    [set].Add(item)
                Next

            Next da
            Dim result As New List(Of ITimeSignal)([set].Count)
            CType(result, List(Of ITimeSignal)).AddRange([set])
            result.Sort(AddressOf EventOrder.BY_TIME_ASC)
            Return result
        End Function

        Public Overrides Function ToString() As String
            Dim name As String = "MIXED"
            If Not map.Empty Then
                name &= map.ToString()
            End If
            Return name
        End Function

    End Class

End Namespace
