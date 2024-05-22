#Region "Microsoft.VisualBasic::a511bd96efe8430f71c8824b2dddf131, Data_science\MachineLearning\DeepLearning\CNN\DataLink.vb"

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

    '   Total Lines: 19
    '    Code Lines: 8 (42.11%)
    ' Comment Lines: 7 (36.84%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (21.05%)
    '     File Size: 559 B


    '     Class DataLink
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN

    Public MustInherit Class DataLink

        ''' <summary>
        ''' the input and output
        ''' </summary>
        ''' <remarks>
        ''' data object at here for link the current layer and the next layer
        ''' no needs for save into the model file
        ''' </remarks>
        <IgnoreDataMember> Protected in_act As DataBlock
        <IgnoreDataMember> Protected out_act As DataBlock

    End Class
End Namespace
