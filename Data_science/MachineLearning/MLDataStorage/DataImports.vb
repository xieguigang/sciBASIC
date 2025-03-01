#Region "Microsoft.VisualBasic::b64241959852cfd06f9f58db63e2a7c9, Data_science\MachineLearning\MLDataStorage\DataImports.vb"

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

'   Total Lines: 62
'    Code Lines: 48 (77.42%)
' Comment Lines: 6 (9.68%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 8 (12.90%)
'     File Size: 2.27 KB


' Module DataImports
' 
'     Function: [Imports], Numeric
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Text.Xml.Models

Public Module DataImports

    ''' <summary>
    ''' Convert dataframe object as the machine learning dataset
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="labels">The column field names for read vector as output labels</param>
    ''' <returns></returns>
    <Extension>
    Public Function [Imports](df As DataFrame, labels As String()) As DataSet
        Dim samples As New List(Of Sample)
        Dim features As New List(Of SampleDistribution)
        Dim featureNames As String() = df.featureNames _
            .Where(Function(lb) labels.IndexOf(lb) < 0) _
            .ToArray
        Dim feature As SampleDistribution
        Dim sample As Sample

        For Each field As String In featureNames
            feature = New SampleDistribution(
                data:=df.features(field).Numeric,
                estimateQuantile:=False
            )
            features.Add(feature)
        Next

        For i As Integer = 0 To df.nsamples - 1
            Dim idx As Integer = i
            Dim v As Double() = featureNames.Select(Function(name) CDbl(df.features(name).vector.GetValue(idx))).ToArray
            Dim label As Double() = labels.Select(Function(name) CDbl(df.features(name).vector.GetValue(idx))).ToArray

            sample = New Sample(v, label, df.rownames(i))
            samples.Add(sample)
        Next

        Return New DataSet With {
            .output = labels,
            .DataSamples = New SampleList With {
                .items = samples.ToArray
            },
            .NormalizeMatrix = New NormalizeMatrix With {
                .matrix = New XmlList(Of SampleDistribution) With {
                    .items = features.ToArray
                },
                .names = featureNames
            }
        }
    End Function

    <Extension>
    Private Iterator Function Numeric(v As FeatureVector) As IEnumerable(Of Double)
        For i As Integer = 0 To v.size - 1
            Yield CDbl(v.vector.GetValue(i))
        Next
    End Function

End Module
