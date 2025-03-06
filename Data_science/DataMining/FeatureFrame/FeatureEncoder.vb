#Region "Microsoft.VisualBasic::a936e1a8623966d71c49fa121c2fd178, Data_science\DataMining\FeatureFrame\FeatureEncoder.vb"

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

    '   Total Lines: 18
    '    Code Lines: 12 (66.67%)
    ' Comment Lines: 3 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (16.67%)
    '     File Size: 590 B


    ' Class FeatureEncoder
    ' 
    '     Function: IndexNames
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' the base abstract data type of the vector encoder
''' </summary>
Public MustInherit Class FeatureEncoder

    Public MustOverride Function Encode(feature As FeatureVector) As DataFrame

    Protected Shared Function IndexNames(feature As FeatureVector) As String()
        Return feature.size _
            .Sequence _
            .Select(Function(i) (i + 1).ToString) _
            .ToArray
    End Function
End Class
