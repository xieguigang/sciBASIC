#Region "Microsoft.VisualBasic::aa9f4a3d714a142c84a8d60a775c67b2, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Delegate\TypeExtensions.vb"

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

    '   Total Lines: 104
    '    Code Lines: 64 (61.54%)
    ' Comment Lines: 22 (21.15%)
    '    - Xml Docs: 95.45%
    ' 
    '   Blank Lines: 18 (17.31%)
    '     File Size: 3.92 KB


    '     Module TypeExtensions
    ' 
    '         Function: CanBeAssignedFrom, ImplementAnyInterfaces, (+2 Overloads) ImplementInterface, ImplementInterfaceAssertInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Emit.Delegates

    Public Module TypeExtensions

        ''' <summary>
        ''' <paramref name="source"/>能否转换至<paramref name="destination"/>类型？
        ''' </summary>
        ''' <param name="destination"></param>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CanBeAssignedFrom(destination As Type, source As Type) As Boolean
            If source Is Nothing OrElse destination Is Nothing Then
                Return False
            End If

            If destination Is source OrElse source.IsSubclassOf(destination) Then
                Return True
            End If

            If destination.IsInterface Then
                Return source.ImplementInterface(destination)
            End If

            If Not destination.IsGenericParameter Then
                Return False
            End If

            Dim constraints = destination.GetGenericParameterConstraints()
            Return constraints.All(Function(t1) t1.CanBeAssignedFrom(source))
        End Function

        ''' <summary>
        ''' 目标类型<paramref name="source"/>是否实现了制定的接口类型？
        ''' </summary>
        ''' <param name="source"></param>
        ''' <typeparam name="interfaceType">接口类型信息</typeparam>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ImplementInterface(Of interfaceType)(source As Type) As Boolean
            Return source.ImplementInterface(GetType(interfaceType))
        End Function

        ''' <summary>
        ''' 目标类型<paramref name="source"/>是否实现了制定的接口类型？
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="interfaceType">接口类型信息</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function has a synlock cache of the type schema test result.
        ''' </remarks>
        <Extension>
        Public Function ImplementInterface(source As Type, interfaceType As Type) As Boolean
            Static cache As New Dictionary(Of Type, Dictionary(Of Type, Boolean))

            SyncLock cache
                If Not cache.ContainsKey(source) Then
                    Call cache.Add(source, New Dictionary(Of Type, Boolean))
                End If
            End SyncLock

            SyncLock cache(source)
                If Not cache(source).ContainsKey(interfaceType) Then
                    Call cache(source).Add(interfaceType, ImplementInterfaceAssertInternal(source, interfaceType))
                End If
            End SyncLock

            Return cache(source)(interfaceType)
        End Function

        <Extension>
        Public Function ImplementAnyInterfaces(source As Type, anyInterfaceTypes As Type()) As Boolean
            For Each type As Type In anyInterfaceTypes
                If source.ImplementInterface(type) Then
                    Return True
                End If
            Next

            Return False
        End Function

        Private Function ImplementInterfaceAssertInternal(source As Type, interfaceType As Type) As Boolean
            While source IsNot Nothing
                Dim interfaces = source.GetInterfaces()

                If interfaces.Any(Function(i)
                                      Return i Is interfaceType OrElse
                                             i.ImplementInterface(interfaceType)
                                  End Function) Then
                    Return True
                End If

                source = source.BaseType
            End While

            Return False
        End Function
    End Module
End Namespace
