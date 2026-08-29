#Region "Microsoft.VisualBasic::ef58075a396b3fa127d2980a15ecb3d2, Data_science\MachineLearning\GNN\Activations\ActivationType.vb"

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

    '   Total Lines: 11
    '    Code Lines: 8 (72.73%)
    ' Comment Lines: 3 (27.27%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 264 B


    ' Enum ActivationType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 激活函数类型枚举
''' </summary>
Public Enum ActivationType
    None       ' 不使用激活函数
    ReLU       ' ReLU
    Sigmoid    ' Sigmoid
    Tanh       ' Tanh
    LeakyReLU  ' LeakyReLU
    Softmax     ' Softmax
End Enum
