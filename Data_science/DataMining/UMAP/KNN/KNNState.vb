#Region "Microsoft.VisualBasic::3a5a00a5b95202b0f60de7bfccd73868, Data_science\DataMining\UMAP\KNN\KNNState.vb"

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

    '   Total Lines: 17
    '    Code Lines: 6
    ' Comment Lines: 7
    '   Blank Lines: 4
    '     File Size: 416 B


    '     Class KNNState
    ' 
    '         Properties: knnDistances, knnIndices
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KNN

    ''' <summary>
    ''' index and the distance matrix of the corresponding indexed entity
    ''' </summary>
    Public Class KNNState

        Public Property knnIndices As Integer()() = Nothing

        ''' <summary>
        ''' weights
        ''' </summary>
        ''' <returns></returns>
        Public Property knnDistances As Double()() = Nothing

    End Class
End Namespace
