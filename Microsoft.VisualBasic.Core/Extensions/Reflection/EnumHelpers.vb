Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
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
    <ExportAPI("Get.Description")>
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
        Dim type As Type = value.GetType()
        Dim s As String = value.ToString
        Dim memInfos As MemberInfo() = type.GetMember(name:=s)

        If memInfos.IsNullOrEmpty Then
            ' 当枚举类型为OR组合的时候，得到的是一个数字
            If s.IsPattern("\d+") Then
                Return FlagCombinationDescription(flag:=CLng(s), type:=type, deli:=deli)
            Else
                Return s
            End If
        End If

        Return memInfos _
            .First _
            .Description([default]:=s)
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
