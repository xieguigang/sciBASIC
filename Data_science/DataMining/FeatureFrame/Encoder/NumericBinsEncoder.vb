#Region "Microsoft.VisualBasic::086220f969fdd10c6274126670277338, Data_science\DataMining\FeatureFrame\Encoder\NumericBinsEncoder.vb"

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

'   Total Lines: 49
'    Code Lines: 40 (81.63%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 9 (18.37%)
'     File Size: 1.86 KB


' Class NumericBinsEncoder
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: Encode, NumericBinsEncoder
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Discretion

Public Class NumericBinsEncoder : Inherits FeatureEncoder

    ReadOnly nbins As Integer
    ReadOnly format As String

    Sub New(nbins As Integer, Optional format As String = "G3")
        Me.nbins = nbins
        Me.format = format
    End Sub

    Public Overrides Function Encode(feature As FeatureVector) As DataFrame
        Return NumericBinsEncoder(feature, nbins, format)
    End Function

    Public Shared Function NumericBinsEncoder(feature As FeatureVector, nbins As Integer, Optional format As String = "G3") As DataFrame
        Dim raw As Double() = feature.TryCast(Of Double)
        Dim encoder As New Discretizer(raw, levels:=nbins)
        Dim extends As New Dictionary(Of String, Integer())
        Dim key As String
        Dim name As String = feature.name
        Dim binNames As String() = encoder.binList _
            .Select(Function(r)
                        Return $"{name} [{r.Min.ToString(format)}~{r.Max.ToString(format)}]"
                    End Function) _
            .ToArray

        For i As Integer = 1 To encoder.binSize
            Call extends.Add(i, New Integer(raw.Length - 1) {})
        Next

        For i As Integer = 0 To raw.Length - 1
            key = encoder.GetLevel(raw(i)) + 1
            extends(key)(i) = 1
        Next

        Return New DataFrame With {
            .features = extends _
                .ToDictionary(Function(v) binNames(Integer.Parse(v.Key) - 1),
                              Function(v)
                                  Return New FeatureVector(binNames(Integer.Parse(v.Key) - 1), v.Value)
                              End Function),
            .rownames = IndexNames(feature)
        }
    End Function

End Class
