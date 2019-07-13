#Region "Microsoft.VisualBasic::0720c8e5af3efa52ad43264a8f773142, Data\DataFrame\StorageProvider\Reflection\StorageProviders\DataFlowDirections.vb"

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

    '     Enum DataFlowDirections
    ' 
    '         ReadDataFromObject, WriteDataToObject
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace StorageProvider.Reflection

    Public Enum DataFlowDirections
        ''' <summary>
        ''' 需要从对象之中读取数据，需要将数据写入文件的时候使用
        ''' </summary>
        ''' <remarks></remarks>
        ReadDataFromObject
        ''' <summary>
        ''' 需要相对象写入数据，从文件之中加载数据的时候使用
        ''' </summary>
        ''' <remarks></remarks>
        WriteDataToObject
    End Enum
End Namespace
