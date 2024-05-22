#Region "Microsoft.VisualBasic::a61c9331e7394af012d6c7c0f49acf0b, Data_science\DataMining\FeatureFrame\Encoder\EnumEncoder.vb"

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

    '   Total Lines: 33
    '    Code Lines: 25 (75.76%)
    ' Comment Lines: 3 (9.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (15.15%)
    '     File Size: 1.14 KB


    ' Class EnumEncoder
    ' 
    '     Function: Encode
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.DataFrame

''' <summary>
''' the feature type should be the string type
''' </summary>
Public Class EnumEncoder : Inherits FeatureEncoder

    Public Overrides Function Encode(feature As FeatureVector) As DataFrame
        Dim strs As String() = feature.TryCast(Of String)
        Dim str As String
        Dim extends As New Dictionary(Of String, Integer())
        Dim factors As String() = strs.Distinct.ToArray
        Dim name As String = feature.name

        For Each key As String In factors
            Call extends.Add(key, New Integer(strs.Length - 1) {})
        Next

        For i As Integer = 0 To strs.Length - 1
            str = strs(i)
            extends(str)(i) = 1
        Next

        Return New DataFrame With {
            .features = extends _
                .ToDictionary(Function(v) $"{name}.{v.Key}",
                              Function(v)
                                  Return New FeatureVector(name, v.Value)
                              End Function),
            .rownames = IndexNames(feature)
        }
    End Function
End Class
