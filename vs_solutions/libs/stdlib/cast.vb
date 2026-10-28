Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports any = Microsoft.VisualBasic.Scripting

Public Module cast

    <Extension>
    Public Function as_string(x As Array) As String()
        Return x.AsObjectEnumerator.Select(Function(o) any.ToString(o)).ToArray
    End Function

    <Extension>
    Public Function json_str(Of T)(x As T, Optional indent As Boolean = False) As String
        Return x.GetJson(indent:=indent)
    End Function

    Public Function parse_json(Of T)(json_str As String) As T
        Return json_str.LoadJSON(Of T)
    End Function

End Module
