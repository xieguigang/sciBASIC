#Region "Microsoft.VisualBasic::279d5f8932e56eb5963414a847240e4a, Microsoft.VisualBasic.Core\src\Scripting\Actives.vb"

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

    '   Total Lines: 181
    '    Code Lines: 131
    ' Comment Lines: 31
    '   Blank Lines: 19
    '     File Size: 7.19 KB


    '     Module Activity
    ' 
    '         Function: __active, __activeArray, __activeDictionary, __activeList, Active
    '                   ActiveObject, DisplayType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Language
Imports v = System.Array

Namespace Scripting

    ''' <summary>
    ''' Display the object instance object json from the type definition by using <see cref="Activator"/>.
    ''' </summary>
    Public Module Activity

        ''' <summary>
        ''' 请注意，所需要进行显示的类型必须要Public类型的，假若该目标类型在Module之中，请保证Module也是Public访问的
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension> Public Function DisplayType(type As Type) As String
            Dim sb As New StringBuilder
            Dim view As ActiveViewsAttribute = type.GetCustomAttribute(Of ActiveViewsAttribute)
            Dim fullName$ = type.FullName

            If fullName.Split("."c).First = "System" Then
                Call sb.AppendLine($"**Decalre**:  _<a href=""https://msdn.microsoft.com/en-us/library/{fullName.ToLower}(v=vs.110).aspx?cs-save-lang=1&cs-lang=vb#code-snippet-1"">{fullName}</a>_")
            Else
                Call sb.AppendLine($"**Decalre**:  _{fullName}_")
            End If

            Call sb.AppendLine()
            Call sb.AppendLine("Example: ")

            If view Is Nothing Then
                Call sb.AppendLine("```json")
                Call sb.AppendLine(Active(type))
                Call sb.AppendLine("```")
            Else
                Call sb.AppendLine("```" & view.type)
                Call sb.AppendLine(view.Views)
                Call sb.AppendLine("```")
            End If

            Return sb.ToString
        End Function

        ''' <summary>
        ''' Display the json of the target type its instance object.
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Active(type As Type) As String
            Return type.GetObjectJson(obj:=type.__active)
        End Function

        ''' <summary>
        ''' 与<see cref="Activator.CreateInstance(Type)"/>所不同的是，这个函数还会对属性进行一些Demo值得赋值操作
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension> Public Function ActiveObject(type As Type) As Object
            Return type.__active
        End Function

        ''' <summary>
        ''' Creates the example instance object for the example
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension> Private Function __active(type As Type) As Object
            If type.Equals(GetType(String)) Then
                Return type.FullName
            End If
            If type.Equals(GetType(Char)) Then
                Return "c"c
            End If
            If type.Equals(GetType(Date)) OrElse
                type.Equals(GetType(DateTime)) Then
                Return Now
            End If
            If __examples.ContainsKey(type) Then
                Return __examples(type)
            End If

            If type.IsInheritsFrom(GetType(Array)) Then
                Return type.__activeArray
            ElseIf type.IsInheritsFrom(GetType(List(Of )), strict:=False) Then
                Return type.__activeList
            ElseIf type.IsInheritsFrom(GetType(Dictionary(Of ,)), strict:=False) Then
                Return type.__activeDictionary
            End If

            Try
                Dim obj As Object = Activator.CreateInstance(type)
                Dim source As IEnumerable(Of PropertyInfo) =
                    type _
                    .GetProperties _
                    .Where(Function(x)
                               Return x.CanWrite AndAlso
                                      x.GetIndexParameters _
                                       .IsNullOrEmpty
                           End Function)

                For Each prop As PropertyInfo In source
                    Dim value As Object = prop _
                        .PropertyType _
                        .__active

                    ' 对于复杂的自定义类型，进行递归分解构造
                    Call prop.SetValue(obj, value)
                Next

                Return obj
            Catch ex As Exception
                ex = New Exception(type.FullName, ex)
                Call App.LogException(ex)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' + <see cref="System.Collections.Generic.List(Of T)"/>
        ''' + <see cref="Microsoft.VisualBasic.Language.List(Of T)"/>(会统一返回这种类型)
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Private Function __activeList(type As Type) As Object
            Dim base As Type = type.GenericTypeArguments(Scan0)
            Dim value As Object = base.__active
            Dim list As Type = GetType(List(Of )).MakeGenericType(base)
            Dim IList As IList = DirectCast(Activator.CreateInstance(list), IList)
            Call IList.Add(value)
            Return IList
        End Function

        <Extension>
        Private Function __activeArray(type As Type) As Object
            Dim base As Type = type.GetElementType
            Dim value As Object = base.__active
            Dim array As Array = v.CreateInstance(base, 1)
            Call array.SetValue(value, Scan0)
            Return array
        End Function

        <Extension>
        Private Function __activeDictionary(type As Type) As Object
            With type.GenericTypeArguments.AsList
                Dim baseKey As Type = .Item(0)
                Dim baseValue As Type = .Item(1)
                Dim k As Object = baseKey.__active
                Dim v As Object = baseValue.__active
                Dim IDictionary As IDictionary = DirectCast(Activator.CreateInstance(type), IDictionary)
                Call IDictionary.Add(k, v)
                Return IDictionary
            End With
        End Function

        ReadOnly __examples As IReadOnlyDictionary(Of Type, Object) =
            New Dictionary(Of Type, Object) From {
 _
                {GetType(Double), 0R},
                {GetType(Double?), 0R},
                {GetType(Single), 0!},
                {GetType(Single?), 0!},
                {GetType(Integer), 0},
                {GetType(Integer?), 0},
                {GetType(Long), 0L},
                {GetType(Long?), 0L},
                {GetType(Short), 0S},
                {GetType(Short?), 0S},
                {GetType(Byte), 0},
                {GetType(SByte), 0},
                {GetType(Boolean), True},
                {GetType(Decimal), 0@}
        }
    End Module
End Namespace
