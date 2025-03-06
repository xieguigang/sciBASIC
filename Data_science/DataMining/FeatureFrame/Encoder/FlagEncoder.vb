#Region "Microsoft.VisualBasic::1b87a840f8e60c5dc06cb6b3cf12bdb5, Data_science\DataMining\FeatureFrame\Encoder\FlagEncoder.vb"

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

    '   Total Lines: 24
    '    Code Lines: 16 (66.67%)
    ' Comment Lines: 3 (12.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 785 B


    ' Class FlagEncoder
    ' 
    '     Function: Encode
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Framework

''' <summary>
''' the feature type should be the boolean type
''' </summary>
Public Class FlagEncoder : Inherits FeatureEncoder

    Public Overrides Function Encode(feature As FeatureVector) As DataFrame
        Dim ints As Double() = New Double(feature.size - 1) {}
        Dim bools As Boolean() = feature.vector
        Dim name As String = feature.name

        For i As Integer = 0 To ints.Length - 1
            ints(i) = If(bools(i), 1, 0)
        Next

        feature = New FeatureVector(name, ints)

        Return New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector) From {{name, feature}},
            .rownames = IndexNames(feature)
        }
    End Function
End Class
