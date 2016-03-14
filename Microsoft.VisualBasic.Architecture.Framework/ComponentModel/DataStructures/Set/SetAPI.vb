Imports System.Runtime.CompilerServices

Public Module SetAPI

    Public Delegate Function IEquals(Of T)(a As T, b As T) As Boolean

    '''<summary>
    ''' Performs an intersection of two sets.(求交集)
    ''' </summary>
    ''' <param name="s1">Any set.</param>
    ''' <param name="s2">Any set.</param>
    ''' <returns>A new <see cref="[Set]">Set</see> object that contains the members
    ''' that were common to both of the input sets.</returns>
    ''' 
    <Extension>
    Public Function Intersection(Of T)(s1 As IEnumerable(Of T), s2 As IEnumerable(Of T), __equals As IEquals(Of T)) As T()
        Dim result As New List(Of T)

        If s1.Count > s2.Count Then
            For Each o As T In s1
                If s2.Contains(o, __equals) Then
                    result.Add(o)
                End If
            Next
        Else
            For Each o As T In s2
                If s1.Contains(o, __equals) Then
                    result.Add(o)
                End If
            Next
        End If

        Return result
    End Function

    <Extension>
    Public Function Contains(Of T)([set] As IEnumerable(Of T), x As T, __equals As IEquals(Of T)) As Boolean
        Dim LQuery = (From obj As T In [set].AsParallel Where __equals(x, obj) Select 1).FirstOrDefault
        Return LQuery > 0
    End Function

    <Extension>
    Public Function Intersection(s1 As IEnumerable(Of String), s2 As IEnumerable(Of String), Optional strict As Boolean = True) As String()
        Return s1.Intersection(s2, AddressOf New __stringCompares(strict).Equals)
    End Function

    Private Structure __stringCompares
        Dim mode As StringComparison

        Sub New(strict As Boolean)
            mode = If(strict, StringHelpers.StrictCompares, StringHelpers.NonStrictCompares)
        End Sub

        Public Overloads Function Equals(a As String, b As String) As Boolean
            Return String.Equals(a, b, mode)
        End Function
    End Structure
End Module
