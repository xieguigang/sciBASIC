#Region "Microsoft.VisualBasic::d9a7dbda05f063fff48836bf78d906ba, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Unit\UnitConvertorExtensions.vb"

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

    '   Total Lines: 61
    '    Code Lines: 50
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 2.30 KB


    '     Module UnitConvertorExtensions
    ' 
    '         Function: Base, GetUnitConvertor, IndexOf, (+4 Overloads) Unit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Ranges.Unit

    <HideModuleName>
    Public Module UnitConvertorExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetUnitConvertor(Of T As Structure)() As UnitTag(Of T)()
            Return Enums(Of T)() _
                .Select(Function(e)
                            Dim size As Double = CDbl(CObj(e))
                            Return New UnitTag(Of T)(e, size)
                        End Function) _
                .OrderBy(Function(u) u.value) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function Base(Of T)(convertors As IEnumerable(Of UnitTag(Of T))) As UnitTag(Of T)
            Return convertors.Where(Function(u) u.value = 1.0#).FirstOrDefault
        End Function

        <Extension>
        Friend Function IndexOf(Of T)(convertors As UnitTag(Of T)(), target As T) As Integer
            For i As Integer = 0 To convertors.Length - 1
                If (convertors(i).unit.Equals(target)) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Unit(Of T As Structure)(value#, unitVal As T) As UnitValue(Of T)
            Return New UnitValue(Of T)(value, unitVal)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Unit(Of T As Structure)(value&, unitVal As T) As UnitValue(Of T)
            Return New UnitValue(Of T)(value, unitVal)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Unit(Of T As Structure)(value%, unitVal As T) As UnitValue(Of T)
            Return New UnitValue(Of T)(value, unitVal)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Unit(Of T As Structure)(value!, unitVal As T) As UnitValue(Of T)
            Return New UnitValue(Of T)(value, unitVal)
        End Function
    End Module

End Namespace
