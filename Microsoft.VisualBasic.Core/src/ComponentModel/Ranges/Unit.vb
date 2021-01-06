#Region "Microsoft.VisualBasic::8589de06db1b316eeeeb7f2b2bc47b32, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Unit.vb"

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

    '     Class Convertor
    ' 
    '         Properties: UnitType
    '         Delegate Function
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '     Module UnitConvertorExtensions
    ' 
    '         Function: Base, GetUnitConvertor, IndexOf, (+4 Overloads) Unit
    ' 
    '     Enum ByteSize
    ' 
    ' 
    ' 
    ' 
    '     Class UnitValue
    ' 
    '         Properties: Unit
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ScaleTo, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Ranges

    Public Class Convertor : Inherits Attribute

        Public ReadOnly Property UnitType As Type

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="TUnit">枚举类型？</typeparam>
        ''' <param name="value"></param>
        ''' <param name="toUnit"></param>
        ''' <returns></returns>
        Public Delegate Function Convertor(Of TUnit As Structure)(value As UnitValue(Of TUnit), toUnit As TUnit) As UnitValue(Of TUnit)

        Sub New(type As Type)
            UnitType = type
        End Sub
    End Class

    Public Module UnitConvertorExtensions

#If NET_48 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetUnitConvertor(Of T As Structure)() As (unit As T, value As Double)()
            Return Enums(Of T)() _
                .Select(Function(e)
                            Dim size As Double = CDbl(CObj(e))
                            Return (e, size)
                        End Function) _
                .OrderBy(Function(u) u.Item2) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function Base(Of T)(convertors As IEnumerable(Of (unit As T, value As Double))) As (unit As T, value As Double)
            Return convertors.Where(Function(u) u.value = 1.0#).FirstOrDefault
        End Function

        <Extension>
        Friend Function IndexOf(Of T)(convertors As (unit As T, value As Double)(), target As T) As Integer
            For i As Integer = 0 To convertors.Length - 1
                If (convertors(i).unit.Equals(target)) Then
                    Return i
                End If
            Next

            Return -1
        End Function

#End If

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

    Public Enum ByteSize As Long
        B = 1
        KB = 1024
        MB = KB * 1024
        GB = MB * 1024
        TB = GB * 1024
    End Enum

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="TUnit">枚举类型，基础类型必须是值等于1</typeparam>
    Public Class UnitValue(Of TUnit As Structure) : Inherits f64

        ''' <summary>
        ''' 分（d） ``10^-1``
        ''' </summary>
        Public Const d = 10 ^ -1
        ''' <summary>
        ''' 厘（c） ``10^-2``
        ''' </summary>
        Public Const c = 10 ^ -2
        ''' <summary>
        ''' 毫（m） ``10^-3``
        ''' </summary>
        Public Const m = 10 ^ -3
        ''' <summary>
        ''' 微（μ） ``10^-6``
        ''' </summary>
        Public Const u = 10 ^ -6
        ''' <summary>
        ''' 纳（n） ``10^-9``
        ''' </summary>
        Public Const n = 10 ^ -9
        ''' <summary>
        ''' 皮（p） ``10^-12``
        ''' </summary>
        Public Const p = 10 ^ -12
        ''' <summary>
        ''' 飞（f） ``10^-15``
        ''' </summary>
        Public Const f = 10 ^ -15
        ''' <summary>
        ''' 阿（a） ``10^-18``
        ''' </summary>
        Public Const a = 10 ^ -18

        Public Property Unit As TUnit

        Sub New(value#, unit As TUnit)
            Me.Value = value
            Me.Unit = unit
        End Sub

        Sub New()
        End Sub

#If NET_48 Then

        ''' <summary>
        ''' Unit convert
        ''' </summary>
        ''' <param name="convert"></param>
        ''' <returns></returns>
        Public Function ScaleTo(convert As TUnit) As UnitValue(Of TUnit)
            Return Me = convert
        End Function

#End If

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{Value} ({DirectCast(CObj(Unit), [Enum]).Description})"
        End Function

#If NET_48 Or netcore5 = 1 Then

        Shared ReadOnly converts As (unit As TUnit, value#)() = UnitConvertorExtensions.GetUnitConvertor(Of TUnit)

        ''' <summary>
        ''' 将当前的单位值转换为目标<paramref name="unit"/>单位制
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="unit"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(value As UnitValue(Of TUnit), unit As TUnit) As UnitValue(Of TUnit)
            ' 先计算出当前的单位值对基础单位值的结果
            Dim index% = converts.IndexOf(value.Unit)
            Dim index2 = converts.IndexOf(unit)
            ' 计算出对基底的结果值
            Dim val# = value * converts(index).value / converts(index2).value

            Return New UnitValue(Of TUnit)(val, unit)
        End Operator

        Public Overloads Shared Operator <>(value As UnitValue(Of TUnit), unit As TUnit) As UnitValue(Of TUnit)
            Throw New NotImplementedException
        End Operator

#End If
    End Class
End Namespace
