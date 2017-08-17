Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Scripting.Runtime

    Public Module CastStringVector

        <Extension> Public Function AsType(Of T)(source As IEnumerable(Of String)) As IEnumerable(Of T)
            Dim type As Type = GetType(T)
            Dim [ctype] = InputHandler.CasterString(type)
            Dim result = source.Select(Function(x) DirectCast([ctype](x), T))
            Return result
        End Function

        <Extension>
        Public Function AsDouble(source As IEnumerable(Of String)) As Double()
            Return source.AsType(Of Double).ToArray
        End Function

        <Extension>
        Public Function AsSingle(source As IEnumerable(Of String)) As Single()
            Return source.AsType(Of Single).ToArray
        End Function

        <Extension>
        Public Function AsBoolean(source As IEnumerable(Of String)) As Boolean()
            Return source.AsType(Of Boolean).ToArray
        End Function

        <Extension>
        Public Function AsInteger(source As IEnumerable(Of String)) As Integer()
            Return source.AsType(Of Integer).ToArray
        End Function

        <Extension>
        Public Function AsColor(source As IEnumerable(Of String)) As Color()
            Return source.AsType(Of Color).ToArray
        End Function
    End Module
End Namespace