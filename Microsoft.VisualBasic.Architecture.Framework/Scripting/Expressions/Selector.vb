Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language

Namespace Scripting.Expressions

    ''' <summary>
    ''' The property value selector
    ''' </summary>
    Public Module Selector

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

        <Extension>
        Public Function [Select](Of T)(source As IEnumerable, type As Type, propertyName$) As IEnumerable(Of T)
            Return source.Select(type, propertyName).Select(Function(o) DirectCast(o, T))
        End Function

        <Extension>
        Public Function [Select](Of T, V)(source As IEnumerable(Of T), propertyName$) As IEnumerable(Of V)
            Return source.Select(GetType(T), propertyName).Select(Function(o) DirectCast(o, V))
        End Function

        ''' <summary>
        ''' Where selector
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
        Public Function [Select](Of T)(source As IEnumerable(Of T), expression$) As IEnumerable(Of T)
            Dim type As Type = GetType(T)
            Dim expr As NamedValue(Of String) = expression.ParseExpression
            Dim [property] As PropertyInfo =
                type _
                .GetProperties(BindingFlags.Public Or BindingFlags.Instance) _
                .Where(Function(prop) prop.Name.TextEquals(expr.Name)) _
                .FirstOrDefault
            Dim value As Object
            Dim compare As Func(Of T, Boolean)

            With expr
                Dim getValue As Func(Of T, Object)

                If .Name = "$" Then
                    getValue = Function(x) x
                    value = .Value.CTypeDynamic(type)
                Else
                    getValue = Function(x)
                                   Return [property].GetValue(x)
                               End Function
                    value = .Value.CTypeDynamic([property].PropertyType)
                End If

                If .Description = "=" Then
                    compare = Function(o) getValue(o).Equals(value)
                ElseIf .Description.TextEquals("IN") Then
                    ' 字符串查找
                    Dim s$ = CStrSafe(value)
                    compare = Function(o) InStr(s, CStrSafe(getValue(o))) > 0
                Else
                    Dim icompareValue = DirectCast(value, IComparable)

                    If .Description = ">" Then
                        compare = Function(o)
                                      Return DirectCast(getValue(o), IComparable).GreaterThan(icompareValue)
                                  End Function
                    ElseIf .Description = "<" Then
                        compare = Function(o)
                                      Return DirectCast(getValue(o), IComparable).LessThan(icompareValue)
                                  End Function
                    ElseIf .Description = "=>" Then
                        compare = Function(o)
                                      Return DirectCast(getValue(o), IComparable).GreaterThanOrEquals(icompareValue)
                                  End Function
                    ElseIf .Description = "<=" Then
                        compare = Function(o)
                                      Return DirectCast(getValue(o), IComparable).LessThanOrEquals(icompareValue)
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
                        l += New String(source.Raw.Skip(source.Pointer).ToArray)
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