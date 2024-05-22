#Region "Microsoft.VisualBasic::58d0c4456f566c0361dca7e54948959e, Data_science\DataMining\FeatureFrame\Encoder\NumericEncoder.vb"

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

    '   Total Lines: 16
    '    Code Lines: 12 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (25.00%)
    '     File Size: 551 B


    ' Class NumericEncoder
    ' 
    '     Function: Encode
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.DataFrame

Public Class NumericEncoder : Inherits FeatureEncoder

    Public Overrides Function Encode(feature As FeatureVector) As DataFrame
        Dim name As String = feature.name
        Dim values As Double() = feature.TryCast(Of Double)

        feature = New FeatureVector(name, values)

        Return New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector) From {{name, feature}},
            .rownames = IndexNames(feature)
        }
    End Function
End Class
