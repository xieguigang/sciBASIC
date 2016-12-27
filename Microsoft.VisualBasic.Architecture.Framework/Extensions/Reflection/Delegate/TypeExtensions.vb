#Region "Microsoft.VisualBasic::5d52aeb1717c3f150a5c31a726f4632f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Reflection\Delegate\TypeExtensions.vb"

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

Imports System.Linq
Imports System.Runtime.CompilerServices

Namespace Emit.Delegates

    Public Module TypeExtensions

        <Extension>
        Public Function CanBeAssignedFrom(destination As Type, source As Type) As Boolean
            If source Is Nothing OrElse destination Is Nothing Then
                Return False
            End If

            If destination Is source OrElse source.IsSubclassOf(destination) Then
                Return True
            End If

            If destination.IsInterface Then
                Return source.ImplementsInterface(destination)
            End If

            If Not destination.IsGenericParameter Then
                Return False
            End If

            Dim constraints = destination.GetGenericParameterConstraints()
            Return constraints.All(Function(t1) t1.CanBeAssignedFrom(source))
        End Function

        <Extension> Public Function ImplementsInterface(source As Type, interfaceType As Type) As Boolean
            While source IsNot Nothing
                Dim interfaces = source.GetInterfaces()
                If interfaces.Any(Function(i) i Is interfaceType OrElse i.ImplementsInterface(interfaceType)) Then
                    Return True
                End If

                source = source.BaseType
            End While
            Return False
        End Function
    End Module
End Namespace
