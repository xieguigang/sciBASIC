#Region "Microsoft.VisualBasic::890d65e120fdc78fcb3a131269e30aab, Data_science\MachineLearning\DeepLearning\NeuralNetwork\StoreProcedure\NamespaceDoc.vb"

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

    '   Total Lines: 15
    '    Code Lines: 6 (40.00%)
    ' Comment Lines: 7 (46.67%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 2 (13.33%)
    '     File Size: 592 B


    '     Module NamespaceDoc
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' 拥有两种类型的数据快照文件:
    ''' 
    ''' 1. 针对小网络模型的,所有的对象都被保存在同一个Xml/json文件之中
    ''' 2. 但是对于大型的网络而言,由于执行序列化的<see cref="StringBuilder"/>或者<see cref="MemoryStream"/>对象
    '''    具有自己的容量上限限制,所以大型网路是将部件分别保存在若干个文件之中完成的
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace
