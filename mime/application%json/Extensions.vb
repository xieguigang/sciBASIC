#Region "Microsoft.VisualBasic::a35d5949fb85f500ec92a190463a44f1, mime\application%json\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Extensions
    ' 
    '     Function: AsString, AsStringVector, ParseJson, ParseJsonFile
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.json.Parser

Public Module Extensions

    ''' <summary>
    ''' Parse json string
    ''' </summary>
    ''' <param name="JsonStr"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ParseJson(JsonStr As String) As JsonElement
        Return New JsonParser().OpenJSON(JsonStr)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ParseJsonFile(JsonFile As String) As JsonElement
        Return New JsonParser().Open(JsonFile)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsString(obj As JsonElement) As String
        Return DirectCast(obj, JsonValue).GetStripString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsStringVector(array As JsonElement) As String()
        If array Is Nothing Then
            Return Nothing
        End If
        If TypeOf array Is JsonValue Then
            Return {array.AsString}
        End If

        Return DirectCast(array, JsonArray) _
            .SafeQuery _
            .Select(AddressOf AsString) _
            .ToArray
    End Function
End Module
