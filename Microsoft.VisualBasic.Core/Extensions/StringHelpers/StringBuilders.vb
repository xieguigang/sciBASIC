Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Public Module StringBuilders

    <Extension>
    Public Function Replace(sb As StringBuilder, ParamArray replacements As NamedValue(Of String)()) As StringBuilder
        For Each tuple As NamedValue(Of String) In replacements.SafeQuery
            Call sb.Replace(tuple.Name, tuple.Value)
        Next

        Return sb
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Replace(sb As StringBuilder, find$, value As Func(Of String)) As StringBuilder
        Return sb.Replace(find, value())
    End Function
End Module
