Imports System.Runtime.CompilerServices

Namespace Text.Xml.Models

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToDictionary(Of T)(properties As IEnumerable(Of [Property]), parser As Func(Of String, T)) As Dictionary(Of String, T)
            Return properties.ToDictionary(Function(p) p.name, Function(p) parser(p.value))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToProperties(Of T)(table As Dictionary(Of String, T), toString As Func(Of T, String)) As IEnumerable(Of [Property])
            Return table.Select(Function(p) New [Property] With {.name = p.Key, .value = toString(p.Value)})
        End Function
    End Module
End Namespace