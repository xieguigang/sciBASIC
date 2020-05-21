Public Class JSONSerializerOptions

    Public Property maskReadonly As Boolean = False
    Public Property indent As Boolean = False
    Public Property enumToString As Boolean = True
    Public Property unixTimestamp As Boolean = True
    Public Property digest As Dictionary(Of Type, Func(Of Object, Object))

End Class