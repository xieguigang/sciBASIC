#Region "Microsoft.VisualBasic::9cd796ead2b6eec2ebc29c8a427c84e4, Microsoft.VisualBasic.Core\Scripting\Expressions\Selector.vb"

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

    '     Module Selector
    ' 
    '         Function: (+3 Overloads) [Select]
    '         Delegate Function
    ' 
    '             Function: [Select], ParseExpression
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Scripting.Expressions

    ''' <summary>
    ''' The property value selector
    ''' </summary>
    Public Module Selector

        ''' <summary>
        ''' Select property value from target source collection by using a specific property name
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="type"></param>
        ''' <param name="propertyName$"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function [Select](source As IEnumerable, type As Type, propertyName$) As IEnumerable(Of Object)
            Dim [property] As PropertyInfo =
                type _
                .GetProperties(BindingFlags.Public Or BindingFlags.Instance) _
                .Where(Function(prop) prop.Name.TextEquals([propertyName])) _
                .FirstOrDefault

            For Each o As Object In source
                Yield [property].GetValue(o, Nothing)
            Next
        End Function

        ''' <summary>
        ''' Select property value from target source collection by using a specific property name and with a specific type casting constraint.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="type"></param>
        ''' <param name="propertyName$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function [Select](Of T)(source As IEnumerable, type As Type, propertyName$) As IEnumerable(Of T)
            Return source.Select(type, propertyName).Select(Function(o) DirectCast(o, T))
        End Function

        ''' <summary>
        ''' Select all of the object elements their a specific property value and convert to a specific value type.
        ''' (将对象类型之中的某一个属性筛选出来，然后转换为指定的数据类型)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="propertyName$">
        ''' If the property name is ``$`` expression, that it means makes a type casting on it self.
        ''' (如果属性名称为``$``，即引用自身，则这个函数的作用只是进行强制的``CType``类型转换)
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function [Select](Of T, V)(source As IEnumerable(Of T), propertyName$) As IEnumerable(Of V)
            If propertyName = "$" Then
                Return source.Select(Function(o) CType(CObj(o), V))
            Else
                Return source.Select(GetType(T), propertyName).Select(Function(o) DirectCast(o, V))
            End If
        End Function

        ''' <summary>
        ''' The object value selector function pointer template
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="property$"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Delegate Function Selector(Of T)(property$, ByRef type As Type) As Func(Of T, Object)

        ''' <summary>
        ''' Where selector.(这个函数之中只有数字和字符串的比较)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="expression$">
        ''' ###### propertyName operator value
        ''' 
        ''' 1. ``a = b``
        ''' 2. ``a > b``
        ''' 3. ``a &lt; b``
        ''' 4. ``a => b``
        ''' 5. ``a &lt;= b``
        ''' 4. ``a IN b``
        ''' 
        ''' ``$``符号表示对象自身
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function [Select](Of T)(source As IEnumerable(Of T), expression$, Optional selector As Selector(Of T) = Nothing) As IEnumerable(Of T)
            Dim type As Type = GetType(T)
            Dim expr As NamedValue(Of String) = expression.ParseExpression
            Dim value As Object
            Dim compare As Func(Of T, Boolean)

            With expr
                Dim getValue As Func(Of T, Object)

                If selector Is Nothing Then
                    If .Name = "$" Then
                        getValue = Function(x) x
                    Else
                        Dim [property] As PropertyInfo =
                            type _
                            .GetProperties(BindingFlags.Public Or BindingFlags.Instance) _
                            .Where(Function(prop) prop.Name.TextEquals(expr.Name)) _
                            .FirstOrDefault
                        type = [property].PropertyType
                        getValue = Function(x)
                                       Return [property].GetValue(x)
                                   End Function
                    End If
                Else
                    getValue = selector(.Name, type)
                End If

                value = .Value.CTypeDynamic(type)

                If .Description = "=" Then
                    compare = Function(o) getValue(o).Equals(value)
                ElseIf .Description.TextEquals("IN") Then
                    ' 字符串查找
                    Dim s$ = CStrSafe(value)
                    compare = Function(o) InStr(s, CStrSafe(getValue(o))) > 0
                Else
                    Dim icompareValue = Val(value) ' DirectCast(value, IComparable)

                    If .Description = ">" Then
                        compare = Function(o)
                                      Return Val(getValue(o)) > (icompareValue)
                                  End Function
                    ElseIf .Description = "<" Then
                        compare = Function(o)
                                      Return Val(getValue(o)) < (icompareValue)
                                  End Function
                    ElseIf .Description = "=>" Then
                        compare = Function(o)
                                      Return Val(getValue(o)) >= (icompareValue)
                                  End Function
                    ElseIf .Description = "<=" Then
                        compare = Function(o)
                                      Return Val(getValue(o)) <= (icompareValue)
                                  End Function
                    Else
                        Throw New NotSupportedException(expression)
                    End If
                End If
            End With

            Return source.Where(predicate:=compare)
        End Function

        <Extension>
        Public Function ParseExpression(expression$) As NamedValue(Of String)
            Dim tmp As New List(Of Char)
            Dim l As New List(Of String)
            Dim source As New Pointer(Of Char)(expression)

            Do While Not source.EndRead
                Dim c As Char = +source

                If c <> " "c Then
                    tmp += c
                Else
                    l += New String(tmp)
                    tmp *= 0

                    If l.Count = 2 Then
                        l += New String(source.RawBuffer.Skip(source.Position).ToArray)
                        Exit Do
                    End If
                End If
            Loop

            If l.Count <> 3 Then
                Throw New SyntaxErrorException(expression)
            End If

            Return New NamedValue(Of String) With {
                .Name = l(Scan0),
                .Description = l(1),
                .Value = l.Last
            }
        End Function
    End Module
End Namespace
