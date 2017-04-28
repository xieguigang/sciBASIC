Imports System.Runtime.CompilerServices

Namespace ComponentModel

    Public Module Extensions

        <Extension>
        Public Function ToEnumsTable(Of T)(classes As IEnumerable(Of ColorClass)) As Dictionary(Of T, ColorClass)
            Return classes.ToDictionary(Function(c) DirectCast(CObj(c.int), T))
        End Function
    End Module
End Namespace