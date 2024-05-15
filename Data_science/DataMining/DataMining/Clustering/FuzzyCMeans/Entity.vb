#Region "Microsoft.VisualBasic::b667d85dd15bb1057b1ba005d05e9f66, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\Entity.vb"

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
    '    Code Lines: 20
    ' Comment Lines: 11
    '   Blank Lines: 5
    '     File Size: 1.23 KB


    '     Class FuzzyCMeansEntity
    ' 
    '         Properties: MarkClusterCenter, memberships, probablyMembership
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FuzzyCMeans

    ''' <summary>
    ''' A numeric vector object that tagged with the fuzzy cmeans cluster membership values
    ''' </summary>
    Public Class FuzzyCMeansEntity : Inherits ClusterEntity

        ''' <summary>
        ''' ``Key``键名和数组的下标一样是从0开始的
        ''' </summary>
        ''' <returns></returns>
        Public Property memberships As Dictionary(Of Integer, Double)
        Public Property MarkClusterCenter As Color

        ''' <summary>
        ''' Max probably of <see cref="memberships"/> its key value.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property probablyMembership As Integer
            Get
                Return memberships.Keys _
                    .Select(Function(i) memberships(i)) _
                    .MaxIndex
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{uid} --> {memberships.GetJson}"
        End Function
    End Class
End Namespace
