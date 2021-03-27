#Region "Microsoft.VisualBasic::2f151b3ffd00dcc8fdb8a017dd0a15ba, Data\BinaryData\BinaryData\XDR\TypeExtensions.vb"

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

    '     Module TypeExtensions
    ' 
    '         Function: ArraySubType, GetAttr, GetAttrs, ListSubType, NullableSubType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Xdr
    Public Module TypeExtensions
        <Extension()>
        Public Function GetAttr(Of T As Attribute)(mi As MemberInfo) As T
            Return TryCast(mi.GetCustomAttributes(GetType(T), True).FirstOrDefault(), T)
        End Function

        <Extension()>
        Public Function GetAttrs(Of T As Attribute)(mi As MemberInfo) As IEnumerable(Of T)
            Return mi.GetCustomAttributes(GetType(T), True).Cast(Of T)()
        End Function

        <Extension()>
        Public Function NullableSubType(type As Type) As Type
            If Not type.IsGenericType Then Return Nothing
            If type.GetGenericTypeDefinition() IsNot GetType(Nullable(Of)) Then Return Nothing
            Return type.GetGenericArguments()(0)
        End Function

        <Extension()>
        Public Function ArraySubType(type As Type) As Type
            If Not type.HasElementType Then Return Nothing
            Dim itemType As Type = type.GetElementType()
            If itemType Is Nothing OrElse itemType.MakeArrayType() IsNot type Then Return Nothing
            Return itemType
        End Function

        <Extension()>
        Public Function ListSubType(type As Type) As Type
            If Not type.IsGenericType Then Return Nothing
            Dim genericType As Type = type.GetGenericTypeDefinition()
            If genericType IsNot GetType(List(Of)) Then Return Nothing
            Return type.GetGenericArguments()(0)
        End Function
    End Module
End Namespace

