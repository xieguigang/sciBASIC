Imports System.Runtime.CompilerServices

Namespace Data.Repository

    Public Module FNV1a

        ReadOnly hashHandler As New Dictionary(Of Type, Func(Of Object, Integer))

        Sub New()
            hashHandler(GetType(String)) = AddressOf GetDeterministicHashCode
        End Sub

        ''' <summary>
        ''' What if you need GetHashCode() to be deterministic across program executions?
        ''' 
        ''' https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetDeterministicHashCode(str As String) As Integer
            Dim hash1 As Integer = (5381 << 16) + 5381
            Dim hash2 As Integer = hash1

            For i As Integer = 0 To str.Length - 1 Step 2
                hash1 = (hash1 << 5) + hash1 Xor AscW(str(i))

                If i = str.Length - 1 Then
                    Exit For
                Else
                    hash2 = (hash2 << 5) + hash2 Xor AscW(str(i + 1))
                End If
            Next

            Return hash1 + hash2 * 1566083941
        End Function

        <Extension()>
        Public Function GetHashCode(Of T)(x As T, getFields As Func(Of T, IEnumerable(Of Object))) As Integer
            Return GetHashCode(getFields(x))
        End Function

        Public Function GetHashCode(getValues As Func(Of IEnumerable(Of Object))) As Integer
            Return GetHashCode(getValues())
        End Function

        Public Sub RegisterHashFunction(target As Type, hash As Func(Of Object, Integer))
            hashHandler(target) = hash
        End Sub

        Private Function getHashValue(obj As Object) As Integer
            If TypeOf obj Is Integer Then
                Return DirectCast(obj, Integer)
            ElseIf hashHandler.ContainsKey(obj.GetType) Then
                Return hashHandler(obj.GetType)(obj)
            Else
                Return obj.GetHashCode
            End If
        End Function

        Public Function GetHashCode(targets As IEnumerable(Of String)) As Integer
            Const offset As Integer = 2166136261
            Const prime As Integer = 16777619

            Return targets.Aggregate(
                seed:=offset,
                func:=Function(hashCode As Integer, value As String)
                          If value Is Nothing OrElse value = "" Then
                              Return (hashCode Xor 0) * prime
                          Else
                              Return (hashCode Xor GetDeterministicHashCode(value)) * prime
                          End If
                      End Function)
        End Function

        ''' <summary>
        ''' get FNV-1a hascode
        ''' 
        ''' ```
        ''' hash = offset_basis
        ''' 
        ''' for each octet_of_data to be hashed
        '''   hash = hash Xor octet_of_data
        '''   hash = hash * FNV_prime
        '''   
        ''' return hash
        ''' ```
        ''' </summary>
        ''' <param name="targets"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://www.isthe.com/chongo/tech/comp/fnv/index.html
        ''' </remarks>
        Public Function GetHashCode(targets As IEnumerable(Of Object)) As Integer
            Const offset As Integer = 2166136261
            Const prime As Integer = 16777619

            Return targets.Aggregate(
                seed:=offset,
                func:=Function(hashCode As Integer, value As Object)
                          If value Is Nothing Then
                              Return (hashCode Xor 0) * prime
                          Else
                              Return (hashCode Xor getHashValue(value)) * prime
                          End If
                      End Function)
        End Function
    End Module
End Namespace
