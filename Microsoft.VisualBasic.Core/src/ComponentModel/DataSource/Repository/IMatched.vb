#Region "Microsoft.VisualBasic::d68023fd62338db0f12e0a895c6091c6, Microsoft.VisualBasic.Core\ComponentModel\DataSource\Repository\IMatched.vb"

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

    '     Interface IMatched
    ' 
    '         Properties: IsMatched
    ' 
    '     Interface IKeyIndex
    ' 
    '         Properties: Entity, Index
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace ComponentModel.DataSourceModel.Repository

    ''' <summary>
    ''' The object implements on this interface can be matched with some rules.
    ''' </summary>
    Public Interface IMatched

        ''' <summary>
        ''' Is this object matched the condition?
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsMatched As Boolean
    End Interface

    Public Interface IKeyIndex(Of T)

        Property Entity As T
        ''' <summary>
        ''' 一般是一些短的字符串所构成的能够唯一标记该<see cref="Entity"/>对象的术语列表
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Index As Index(Of String)

    End Interface
End Namespace
