#Region "Microsoft.VisualBasic::69208aca7368006f7640b7ed4621afe0, Microsoft.VisualBasic.Core\src\My\JavaScript\JavaScriptObject\Abstract.vb"

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
    '    Code Lines: 10 (45.45%)
    ' Comment Lines: 9 (40.91%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (13.64%)
    '     File Size: 631 B


    '     Interface IJavaScriptObjectAccessor
    ' 
    '         Properties: Accessor
    ' 
    '     Enum MemberAccessorResult
    ' 
    '         ClassMemberProperty, ExtensionProperty, Undefined
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace My.JavaScript

    ''' <summary>
    ''' Interface helper for get value by name
    ''' </summary>
    Public Interface IJavaScriptObjectAccessor

        ''' <summary>
        ''' get member value by name
        ''' </summary>
        ''' <param name="name">
        ''' the member name, could be property name or field name
        ''' </param>
        ''' <returns>
        ''' the object member value
        ''' </returns>
        Default Property Accessor(name As String) As Object

    End Interface

    Public Enum MemberAccessorResult
        ''' <summary>
        ''' Member is not exists in current javascript object
        ''' </summary>
        Undefined
        ''' <summary>
        ''' IS a member property in this javascript object
        ''' </summary>
        ClassMemberProperty
        ''' <summary>
        ''' Is an extension property object this javascript object
        ''' </summary>
        ExtensionProperty
    End Enum

End Namespace
