#Region "Microsoft.VisualBasic::cfbf1f92b359151281dd0634e2e72402, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\Enumerable\IEnumerable.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace ComponentModel.Collection.Generic

    Public Interface IPairItem(Of TItem1, TItem2)
        Property Key1 As TItem1
        Property Key2 As TItem2

        ''' <summary>
        ''' Call by the method <see cref="IEnumerations.GetItem"></see>
        ''' </summary>
        ''' <param name="pairItem"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Equals(pairItem As IPairItem(Of TItem1, TItem2)) As Boolean
    End Interface

    Public Interface IHandle
        Property Handle As Long
    End Interface

    ''' <summary>
    ''' This type of object have a <see cref="sIdEnumerable.Identifier"></see> property to unique identified itself in a collection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface sIdEnumerable

        ''' <summary>
        ''' The unique identifer in the object collection. Unique-Id of the target implements object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Identifier As String
    End Interface

    Public Interface IReadOnlyId

        ''' <summary>
        ''' The unique identifer in the object collection. Unique-Id of the target implements object
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Identity As String
    End Interface
End Namespace
