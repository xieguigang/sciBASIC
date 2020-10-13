#Region "Microsoft.VisualBasic::eaca38e33d0009246042e31bc7b525e5, Microsoft.VisualBasic.Core\Extensions\Reflection\EnumHelpers.vb"

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

    ' Module EnumHelpers
    ' 
    '     Function: (+2 Overloads) Description, Enums, FlagCombinationDescription, (+2 Overloads) GetAllEnumFlags
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module EnumHelpers

    ''' <summary>
    ''' Get array value from the input flaged enum <paramref name="value"/>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="value"></param>
    ''' <returns></returns>
    Public Function GetAllEnumFlags(Of T As Structure)(value As T) As T()
        Dim type As Type = GetType(T)
        Dim enumValue As [Enum] = CType(CObj(value), [Enum])
        Dim array As T() = GetAllEnumFlags(enumValue, type) _
            .Select(Function(e)
                        Return DirectCast(CObj(e), T)
                    End Function) _
            .ToArray

        Return array
    End Function

    ''' <summary>
    ''' Get array value from the input flaged enum <paramref name="enumValue"/>.
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function GetAllEnumFlags(enumValue As [Enum], type As Type) As IEnumerable(Of [Enum])
        For Each flag As [Enum] In type _
            .GetEnumValues _
            .AsObjectEnumerator _
            .Select(Function(o) CType(CObj(o), [Enum]))

            If enumValue.HasFlag(flag) Then
                Yield flag
            End If
        Next
    End Function

    ''' <summary>
    ''' Enumerate all of the enum values in the specific <see cref="System.Enum"/> type data.
    ''' (只允许枚举类型，其他的都返回空集合)
    ''' </summary>
    ''' <typeparam name="T">泛型类型约束只允许枚举类型，其他的都返回空集合</typeparam>
    ''' <returns></returns>
    Public Function Enums(Of T As Structure)() As T()
        Dim enumType As Type = GetType(T)

        If Not enumType.IsInheritsFrom(GetType(System.Enum)) Then
            Return Nothing
        End If

        Dim enumValues As T() = enumType _
            .GetEnumValues _
            .AsObjectEnumerator _
            .Select(Function([enum]) DirectCast([enum], T)) _
            .ToArray

        Return enumValues
    End Function

#If FRAMEWORD_CORE Then
    ''' <summary>
    ''' Get the description data from a enum type value, if the target have no <see cref="DescriptionAttribute"></see> attribute data
    ''' then function will return the string value from the ToString() function.
    ''' </summary>
    ''' <param name="value"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension>
    Public Function Description(value As [Enum], Optional deli$ = "|") As String
#Else
    ''' <summary>
    ''' Get the description data from a enum type value, if the target have no <see cref="DescriptionAttribute"></see> attribute data
    ''' then function will return the string value from the ToString() function.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Description(value As [Enum]) As String
#End If
        Static descriptionCache As New Dictionary(Of Object, String)

        If descriptionCache.ContainsKey(value) Then
            Return descriptionCache(value)
        End If

        Dim type As Type = value.GetType()
        Dim s As String = value.ToString
        Dim memInfos As MemberInfo() = type.GetMember(name:=s)
        Dim result As String

        If memInfos.IsNullOrEmpty Then
            ' 当枚举类型为OR组合的时候，得到的是一个数字
            If s.IsPattern("\d+") Then
                result = FlagCombinationDescription(flag:=CLng(s), type:=type, deli:=deli)
            Else
                result = s
            End If
        Else
            result = memInfos _
                .First _
                .Description([default]:=s)
        End If

        descriptionCache.Add(value, result)

        Return result
    End Function

    Public Function FlagCombinationDescription(flag As Long, type As Type, deli$) As String
        Dim flags As New List(Of [Enum])
        Dim flagValue As Long

        For Each member In type.GetFields.Where(Function(field) field.FieldType Is type)
            flagValue = CLng(member.GetValue(Nothing))

            If flag And flagValue = flagValue Then
                flags += CType(member.GetValue(Nothing), [Enum])
            End If
        Next

        Return flags.Select(AddressOf Description).JoinBy(deli)
    End Function
End Module
