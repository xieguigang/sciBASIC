#Region "Microsoft.VisualBasic::bf5cbde397fefa05a86d05a92addcfee, sciBASIC#\Data_science\DataMining\BinaryTree\ComparisonProvider\AlignmentComparison.vb"

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

    '   Total Lines: 34
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.21 KB


    ' Class AlignmentComparison
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetSimilarity
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class AlignmentComparison : Inherits ComparisonProvider

    ReadOnly dataIndex As Dictionary(Of String, Double())

    Sub New(dataset As NamedValue(Of Dictionary(Of String, Double))(), equals As Double, gt As Double)
        Call MyBase.New(equals, gt)

        Dim names As String() = dataset _
            .Select(Function(a) a.Value.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray

        dataIndex = dataset _
            .ToDictionary(Function(d) d.Name,
                          Function(d)
                              Return names _
                                  .Select(Function(col) d.Value.TryGetValue(col)) _
                                  .ToArray
                          End Function)
    End Sub

    Public Overrides Function GetSimilarity(x As String, y As String) As Double
        Dim xvec As New Vector(dataIndex(x))
        Dim yvec As New Vector(dataIndex(y))

        Return SSM(xvec, yvec)
    End Function
End Class
