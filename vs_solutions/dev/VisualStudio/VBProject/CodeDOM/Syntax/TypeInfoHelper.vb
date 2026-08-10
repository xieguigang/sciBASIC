#Region "Microsoft.VisualBasic::1146855864ebcb108155cbd6f34abb94, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\Syntax\TypeInfoHelper.vb"

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

    '   Total Lines: 30
    '    Code Lines: 11 (36.67%)
    ' Comment Lines: 13 (43.33%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 6 (20.00%)
    '     File Size: 1.10 KB


    '     Module TypeInfoHelper
    ' 
    '         Function: TypeRef
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace VBProj.CodeDOM.Syntax

    ''' <summary>
    ''' helpers to build <see cref="TypeInfo"/> clr type references from the
    ''' type name text that was extracted out of the VB.NET source code.
    '''
    ''' Because the type is only known by its textual name at parse time, the
    ''' <see cref="TypeInfo.assembly"/> and <see cref="TypeInfo.reference"/>
    ''' fields are left empty; <see cref="TypeInfo.isSystemKnownType"/> can
    ''' still be used to detect framework known types.
    ''' </summary>
    Public Module TypeInfoHelper

        ''' <summary>
        ''' build a clr type reference from a type name text (e.g. "Integer",
        ''' "System.String", "List(Of T)"). returns nothing for empty input.
        ''' </summary>
        Public Function TypeRef(name As String) As TypeInfo
            If String.IsNullOrWhiteSpace(name) Then
                Return Nothing
            End If

            Return New TypeInfo With {.fullName = name.Trim()}
        End Function

    End Module

End Namespace
