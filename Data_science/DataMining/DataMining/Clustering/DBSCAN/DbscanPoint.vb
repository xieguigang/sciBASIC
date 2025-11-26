#Region "Microsoft.VisualBasic::2055c233013d9dfa8d113b3debb3592f, Data_science\DataMining\DataMining\Clustering\DBSCAN\DbscanPoint.vb"

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

    '   Total Lines: 37
    '    Code Lines: 25 (67.57%)
    ' Comment Lines: 4 (10.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (21.62%)
    '     File Size: 1.19 KB


    '     Class DbscanPoint
    ' 
    '         Properties: ID, Index
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Enum ClusterIDs
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace DBSCAN

    Public Class DbscanPoint(Of T) : Implements INamedValue, IIndexOf(Of Integer)

        Public IsVisited As Boolean
        Public ClusterPoint As T
        Public ClusterId As Integer = ClusterIDs.Noise

        Public Property Index As Integer Implements IIndexOf(Of Integer).Address

        Public Sub New(x As T, Optional i As Integer = 0)
            ClusterPoint = x
            IsVisited = False
            Index = i
            ClusterId = ClusterIDs.Unclassified
        End Sub

        ''' <summary>
        ''' the raw data unique reference id
        ''' </summary>
        ''' <returns></returns>
        Public Property ID As String Implements IKeyedEntity(Of String).Key

        Public Overrides Function ToString() As String
            Return $"[{ClusterId}] {ClusterPoint.ToString}"
        End Function
    End Class

    Public Enum ClusterIDs As Integer
        Unclassified = 0
        Noise = -1
    End Enum
End Namespace
