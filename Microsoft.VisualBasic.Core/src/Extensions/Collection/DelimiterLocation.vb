#Region "Microsoft.VisualBasic::7b0438763c52e7f7abe927100ce7a96f, Microsoft.VisualBasic.Core\src\Extensions\Collection\DelimiterLocation.vb"

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

    '   Total Lines: 21
    '    Code Lines: 6
    ' Comment Lines: 15
    '   Blank Lines: 0
    '     File Size: 556 B


    ' Enum DelimiterLocation
    ' 
    '     Individual, NextFirst, NotIncludes, PreviousLast
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 分隔符对象在分块之中的位置
''' </summary>
Public Enum DelimiterLocation As Integer
    ''' <summary>
    ''' 上一个分块的最末尾
    ''' </summary>
    PreviousLast
    ''' <summary>
    ''' 不会再任何分块之中包含有分隔符
    ''' </summary>
    NotIncludes
    ''' <summary>
    ''' 包含在下一个分块之中的最开始的位置
    ''' </summary>
    NextFirst
    ''' <summary>
    ''' 分隔符在单独的一个切割分块之中
    ''' </summary>
    Individual
End Enum
