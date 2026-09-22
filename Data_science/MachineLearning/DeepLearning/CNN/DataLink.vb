#Region "Microsoft.VisualBasic::efbbd18b67f28e41a0a20dc65c52bab3, Data_science\MachineLearning\DeepLearning\CNN\DataLink.vb"

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

    '   Total Lines: 22
    '    Code Lines: 8 (36.36%)
    ' Comment Lines: 10 (45.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (18.18%)
    '     File Size: 746 B


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

    ''' <summary>
    ''' Base class for layer links that hold the input and output <see cref="DataBlock"/> of a layer.
    ''' </summary>
    Public MustInherit Class DataLink

        ''' <summary>
        ''' The input and output activations of the layer.
        ''' </summary>
        ''' <remarks>
        ''' These data objects link the current layer to the next layer and therefore do not need to be persisted into
        ''' the model file.
        ''' </remarks>
        <IgnoreDataMember> Protected in_act As DataBlock
        <IgnoreDataMember> Protected out_act As DataBlock

    End Class
End Namespace
