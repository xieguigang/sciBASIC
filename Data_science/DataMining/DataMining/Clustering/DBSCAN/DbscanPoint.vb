#Region "Microsoft.VisualBasic::877132dbf5150fb08a32fb406394e504, Data_science\DataMining\DataMining\Clustering\DBSCAN\DbscanPoint.vb"

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

    '     Class DbscanPoint
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

Namespace DBSCAN

    Public Class DbscanPoint(Of T)

        Public IsVisited As Boolean
        Public ClusterPoint As T
        Public ClusterId As Integer

        Public Sub New(x As T)
            ClusterPoint = x
            IsVisited = False
            ClusterId = ClusterIDs.Unclassified
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{ClusterId}] {ClusterPoint.ToString}"
        End Function
    End Class

    Public Enum ClusterIDs As Integer
        Unclassified = 0
        Noise = -1
    End Enum
End Namespace
