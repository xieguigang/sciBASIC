Imports System.Runtime.CompilerServices

Namespace Scripting.SymbolBuilder.VBLanguage

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' 这个拓展函数将字典之中的字符串主键处理为符合VB的对象命名规则的字符串
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsVBIdentifier(Of T)(table As Dictionary(Of String, T)) As Dictionary(Of String, T)
            Return table.ToDictionary(Function(map) AsVBIdentifier(map.Key), Function(map) map.Value)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsVBIdentifier(key As String) As String
            Return key.NormalizePathString(alphabetOnly:=True).Replace(" ", "_")
        End Function
    End Module
End Namespace