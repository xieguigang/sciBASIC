#Region "Microsoft.VisualBasic::4dd18ffb656304271cedd442d237c4c2, Data_science\MachineLearning\xgboost\tree\RTreeNodeStat.vb"

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

    '   Total Lines: 26
    '    Code Lines: 16
    ' Comment Lines: 7
    '   Blank Lines: 3
    '     File Size: 915 B


    '     Class RTreeNodeStat
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

Namespace tree

    ''' <summary>
    ''' Statistics each node in tree.
    ''' </summary>
    <Serializable>
    Friend Class RTreeNodeStat
        ' ! \brief loss chg caused by current split 
        Friend ReadOnly loss_chg As Single
        ' ! \brief sum of hessian values, used to measure coverage of data 
        Friend ReadOnly sum_hess As Single
        ' ! \brief weight of current node 
        Friend ReadOnly base_weight As Single
        ' ! \brief number of child that is leaf node known up to now 
        Friend ReadOnly leaf_child_cnt As Integer

        Friend Sub New(reader As ModelReader)
            loss_chg = reader.readFloat()
            sum_hess = reader.readFloat()
            base_weight = reader.readFloat()
            leaf_child_cnt = reader.readInt()
        End Sub
    End Class
End Namespace
