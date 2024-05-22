#Region "Microsoft.VisualBasic::b23054bc1558daef393b5315f932249a, Data_science\DataMining\DataMining\DecisionTree\Attributes.vb"

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

    '   Total Lines: 36
    '    Code Lines: 26 (72.22%)
    ' Comment Lines: 3 (8.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (19.44%)
    '     File Size: 1.20 KB


    '     Class Attributes
    ' 
    '         Properties: differentAttributeNames, informationGain, name
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetDifferentAttributeNamesOfColumn, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DecisionTree.Data
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace DecisionTree

    ''' <summary>
    ''' Node attribute value
    ''' </summary>
    Public Class Attributes

        Public Property name As String
        Public Property differentAttributeNames As String()
        Public Property informationGain As Double

        Public Sub New(name As String, differentAttributenames$())
            Me.name = name
            Me.differentAttributeNames = differentAttributenames
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {differentAttributeNames.GetJson} = {informationGain}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetDifferentAttributeNamesOfColumn(data As DataTable, columnIndex As Integer) As String()
            Return data.rows _
                .Select(Function(d) d(columnIndex)) _
                .Distinct _
                .ToArray
        End Function
    End Class
End Namespace
