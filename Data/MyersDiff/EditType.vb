#Region "Microsoft.VisualBasic::95bb1a017f55c96482fdd1914a806c02, Data\MyersDiff\EditType.vb"

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

    '   Total Lines: 14
    '    Code Lines: 5 (35.71%)
    ' Comment Lines: 9 (64.29%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 0 (0.00%)
    '     File Size: 603 B


    ' Enum EditType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' -----------------------------------------------------------------------
' 编辑操作类型枚举
' -----------------------------------------------------------------------
''' <summary>
''' 表示差异项的编辑操作类型。
''' </summary>
Public Enum EditType As Integer
    ''' <summary>两序列中相同的元素，无需编辑。</summary>
    Equal = 0
    ''' <summary>元素在旧序列中存在但在新序列中被删除。</summary>
    Delete = 1
    ''' <summary>元素在新序列中存在但旧序列中没有，属于插入操作。</summary>
    Insert = 2
End Enum
