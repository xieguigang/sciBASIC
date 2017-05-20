Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Scripting.Expressions

    ''' <summary>
    ''' The property value selector
    ''' </summary>
    Public Module [Select]

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
    End Module
End Namespace