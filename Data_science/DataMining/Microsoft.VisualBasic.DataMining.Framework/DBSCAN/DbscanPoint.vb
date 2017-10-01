#Region "Microsoft.VisualBasic::163fd3064429b06f220f250861271eae, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\DBSCAN\DbscanPoint.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region


Namespace DBSCAN

    Public Class DbscanPoint(Of T)

        Public IsVisited As Boolean
        Public ClusterPoint As T
        Public ClusterId As Integer

        Public Sub New(x As T)
            ClusterPoint = x
            IsVisited = False
            ClusterId = CInt(ClusterIds.Unclassified)
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{ClusterId}] {ClusterPoint.ToString}"
        End Function
    End Class
End Namespace
