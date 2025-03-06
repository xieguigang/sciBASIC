#Region "Microsoft.VisualBasic::f19851830e0e0b7e9e64cad38eb807c7, Microsoft.VisualBasic.Core\src\Data\TypeCast\TypeCaster.vb"

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

    '   Total Lines: 97
    '    Code Lines: 66 (68.04%)
    ' Comment Lines: 12 (12.37%)
    '    - Xml Docs: 91.67%
    ' 
    '   Blank Lines: 19 (19.59%)
    '     File Size: 3.80 KB


    '     Module Extensions
    ' 
    '         Function: GetBytes, GetString, ParseObject, ParseVector, ToObject
    ' 
    '         Sub: Add
    ' 
    '     Interface ITypeCaster
    ' 
    '         Properties: type
    ' 
    '         Function: GetBytes, GetString, (+2 Overloads) ParseObject, ToObject
    ' 
    '     Class TypeCaster
    ' 
    '         Properties: sizeOf, type
    ' 
    '         Function: marshalSizeOf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace ComponentModel.DataSourceModel.TypeCast

    <HideModuleName> Public Module Extensions

        ReadOnly typeCaster As New Dictionary(Of Type, ITypeCaster) From {
            New StringCaster,
            New IntegerCaster, New DoubleCaster,
            New DateCaster,
            New BooleanCaster
        }

        <Extension>
        Private Sub Add(table As Dictionary(Of Type, ITypeCaster), caster As ITypeCaster)
            Call table.Add(caster.type, caster)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetBytes(type As Type) As Func(Of Object, Byte())
            Return AddressOf typeCaster(type).GetBytes
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetString(type As Type) As Func(Of Object, String)
            Return AddressOf typeCaster(type).GetString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToObject(type As Type) As Func(Of Byte(), Object)
            Return AddressOf typeCaster(type).ToObject
        End Function

        ''' <summary>
        ''' get helper function for parse the string to clr object
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ParseObject(type As Type) As Func(Of String, Object)
            Return AddressOf typeCaster(type).ParseObject
        End Function

        <Extension>
        Public Function ParseVector(type As Type) As Func(Of IEnumerable(Of String), Array)
            Return AddressOf typeCaster(type).ParseObject
        End Function
    End Module

    Public Interface ITypeCaster

        ReadOnly Property type As Type

        Function GetBytes(value As Object) As Byte()
        Function GetString(value As Object) As String
        Function ToObject(bytes As Byte()) As Object
        Function ParseObject(str As String) As Object
        Function ParseObject(strs As IEnumerable(Of String)) As Array

    End Interface

    Public MustInherit Class TypeCaster(Of T) : Implements ITypeCaster

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' string has no marshal size value, but data type of string always has a zero terminator
        ''' </remarks>
        Public ReadOnly Property sizeOf As Integer = marshalSizeOf()
        Public ReadOnly Property type As Type = GetType(T) Implements ITypeCaster.type

        Private Shared Function marshalSizeOf() As Integer
            Select Case GetType(T)
                Case GetType(String) : Return 1
                Case GetType(Date) : Return HeapSizeOf.datetime
                Case Else
                    Return Marshal.SizeOf(GetType(T))
            End Select
        End Function

        Public MustOverride Function GetBytes(value As Object) As Byte() Implements ITypeCaster.GetBytes
        Public MustOverride Function GetString(value As Object) As String Implements ITypeCaster.GetString
        Public MustOverride Function ToObject(bytes As Byte()) As Object Implements ITypeCaster.ToObject
        Public MustOverride Function ParseObject(str As String) As Object Implements ITypeCaster.ParseObject
        Public MustOverride Function ParseObject(strs As IEnumerable(Of String)) As Array Implements ITypeCaster.ParseObject

    End Class

End Namespace
