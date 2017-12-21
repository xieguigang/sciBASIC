Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel.Repository

    Public MustInherit Class QueryCacheFactory(Of T As IKeyedEntity(Of String))
        Implements IRepositoryRead(Of String, T)

        Dim cache As New Dictionary(Of String, T)
        Dim factory As Func(Of String, T)

        Sub New(factory As Func(Of String, T), Optional cache As IReadOnlyDictionary(Of String, T) = Nothing)
            Me.factory = factory
            Me.cache = cache.SafeQuery.ToDictionary
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{cache.Count} entity was cached. (keys={cache.Keys.Take(5).JoinBy("; ")}...)"
        End Function

        ''' <summary>
        ''' Clear the cache memory
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Clear()
            Call cache.Clear()
        End Sub

        Public Function Exists(key As String) As Boolean Implements IRepositoryRead(Of String, T).Exists
            If cache.ContainsKey(key) Then
                Return True
            Else
                Return Not GetByKey(key) Is Nothing
            End If
        End Function

        ''' <summary>
        ''' Load or read from cache
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Function GetByKey(key As String) As T Implements IRepositoryRead(Of String, T).GetByKey
            If cache.ContainsKey(key) Then
                Return cache(key)
            Else
                Dim entity As T = factory(key)

                If entity Is Nothing Then
                    Return Nothing
                Else
                    cache.Add(entity.Key, entity)
                End If

                Return entity
            End If
        End Function

        ''' <summary>
        ''' Only works on cache
        ''' </summary>
        ''' <param name="clause"></param>
        ''' <returns></returns>
        Public Function GetWhere(clause As Func(Of T, Boolean)) As IReadOnlyDictionary(Of String, T) Implements IRepositoryRead(Of String, T).GetWhere
            Return cache.Values.Where(clause).ToDictionary(Function(t) t.Key)
        End Function

        ''' <summary>
        ''' Only works on cache
        ''' </summary>
        ''' <returns></returns>
        Public Function GetAll() As IReadOnlyDictionary(Of String, T) Implements IRepositoryRead(Of String, T).GetAll
            Return New Dictionary(Of String, T)(cache)
        End Function
    End Class
End Namespace