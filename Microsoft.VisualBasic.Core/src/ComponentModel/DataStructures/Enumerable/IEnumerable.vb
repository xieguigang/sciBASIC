﻿#Region "Microsoft.VisualBasic::3eb69c5ec28321fbdb068dfd45528a54, G:/GCModeller/src/runtime/sciBASIC#/Microsoft.VisualBasic.Core/src//ComponentModel/DataStructures/Enumerable/IEnumerable.vb"

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

    '   Total Lines: 25
    '    Code Lines: 8
    ' Comment Lines: 13
    '   Blank Lines: 4
    '     File Size: 982 B


    '     Interface INamedValue
    ' 
    ' 
    ' 
    '     Interface IReadOnlyId
    ' 
    '         Properties: Identity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' This type of object have a <see cref="INamedValue.Key"></see> property to unique identified itself in a collection.
    ''' This interface was inherits from type <see cref="IKeyedEntity(Of String)"/>.
    ''' (一个具有自己的名称的变量值的抽象)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface INamedValue : Inherits IKeyedEntity(Of String)
    End Interface

    ''' <summary>
    ''' 与<see cref="iNamedValue"/>所不同的是，这个对象的标识属性是只读的.
    ''' </summary>
    Public Interface IReadOnlyId

        ''' <summary>
        ''' The unique identifer in the object collection. Unique-Id of the target implements object
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Identity As String
    End Interface
End Namespace
