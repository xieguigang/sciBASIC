#Region "Microsoft.VisualBasic::d78439964c9dee886517ca87740b7d40, Data_science\MachineLearning\MLDebugger\DataSetExtensions.vb"

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

    ' Module DataSetExtensions
    ' 
    '     Function: SampleSetCreator, ToTable
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure
Imports Table = Microsoft.VisualBasic.Data.csv.IO.DataSet

<HideModuleName>
Public Module DataSetExtensions

    ''' <summary>
    ''' We usually use this extension method for generates the demo test dataset.
    ''' </summary>
    ''' <param name="samples"></param>
    ''' <param name="inputNames"></param>
    ''' <param name="outputNames"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function SampleSetCreator(samples As IEnumerable(Of Sample),
                                     Optional inputNames As IEnumerable(Of String) = Nothing,
                                     Optional outputNames As IEnumerable(Of String) = Nothing) As DataSet

        Return New SampleList With {
            .items = samples _
                .SafeQuery _
                .ToArray
        }.CreateDataSet(inputNames, outputNames)
    End Function

    ''' <summary>
    ''' Convert XML dataset to csv table
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="markOutput">All of the column name of the data output will be marked in format like ``[name]``.</param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function ToTable(raw As DataSet, Optional markOutput As Boolean = False) As IEnumerable(Of Table)
        Dim inputNames As String() = raw.NormalizeMatrix.names
        Dim outputs As String() = raw.output
        Dim data As Dictionary(Of String, Double)
        Dim size As Integer = raw.DataSamples.size
        Dim vec As Double()

        For Each sample As Sample In raw.DataSamples.AsEnumerable
            vec = sample.vector
            data = inputNames _
                .SeqIterator _
                .ToDictionary(Function(name) name.value,
                              Function(name)
                                  Return vec(name)
                              End Function)

            ' append output result to input data
            Call outputs _
                .SeqIterator _
                .DoEach(Sub(name)
                            Dim target As Double() = sample.target
                            Dim val As Double = target(name)

                            ' output element name can not be duplicated with
                            ' the input name
                            If markOutput Then
                                Call data.Add($"[{name.value}]", val)
                            Else
                                Call data.Add(name.value, val)
                            End If
                        End Sub)

            size -= 1

            Yield New Table With {
                .ID = sample.ID,
                .Properties = data
            }
        Next

        If size > 0 Then
            Call $"Inconsistent sample size! {size} sample is missing!".Warning
        End If
    End Function
End Module
