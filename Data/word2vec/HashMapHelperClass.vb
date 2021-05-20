'---------------------------------------------------------------------------------------------------------
'	Copyright ©  - 2017 Tangible Software Solutions Inc.
'	This class can be used by anyone provided that the copyright notice remains intact.
'
'	This class is used to replace calls to some Java HashMap or Hashtable methods.
'---------------------------------------------------------------------------------------------------------
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices

Friend Module HashMapHelperClass
    <Extension()>
    Friend Function SetOfKeyValuePairs(Of TKey, TValue)(dictionary As IDictionary(Of TKey, TValue)) As HashSet(Of KeyValuePair(Of TKey, TValue))
        Dim entries As HashSet(Of KeyValuePair(Of TKey, TValue)) = New HashSet(Of KeyValuePair(Of TKey, TValue))()

        For Each keyValuePair In dictionary
            entries.Add(keyValuePair)
        Next

        Return entries
    End Function

    <Extension()>
    Friend Function GetValueOrNull(Of TKey, TValue)(dictionary As IDictionary(Of TKey, TValue), key As TKey) As TValue
        Dim ret As TValue
        dictionary.TryGetValue(key, ret)
        Return ret
    End Function
End Module
