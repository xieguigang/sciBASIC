Imports Microsoft.VisualBasic.ComponentModel

Namespace Marshal

    Public Class Pointer(Of T) : Inherits DataStructures.Pointer(Of T)

        Protected __innerRaw As T()

        Public ReadOnly Property Current As T
            Get
                Return Value(__index)
            End Get
        End Property

        ''' <summary>
        ''' 相对于当前的指针的位置而言的
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        Default Public Property Value(p As Integer) As T
            Get
                p += __index

                If p < 0 OrElse p >= __innerRaw.Length Then
                    Return Nothing
                Else
                    Return __innerRaw(p)
                End If
            End Get
            Set(value As T)
                p += __index

                If p < 0 OrElse p >= __innerRaw.Length Then
                    Throw New MemberAccessException(p & " reference to invalid memory region!")
                Else
                    __innerRaw(p) = value
                End If
            End Set
        End Property

        Public ReadOnly Property EndRead As Boolean
            Get
                Return __index = __innerRaw.Length
            End Get
        End Property

        Sub New(array As T())
            __innerRaw = array
        End Sub

        Sub New(array As List(Of T))
            __innerRaw = array.ToArray
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"* {GetType(T).Name} + {__index} --> {Current}  // {Scan0.ToString}"
        End Function

        Public Overloads Shared Narrowing Operator CType(p As Pointer(Of T)) As T
            Return p.Current
        End Operator

        Public Overloads Shared Widening Operator CType(raw As T()) As Pointer(Of T)
            Return New Pointer(Of T)(raw)
        End Operator

        Public Overloads Shared Operator +(ptr As Pointer(Of T), d As Integer) As Pointer(Of T)
            ptr.__index += d
            Return ptr
        End Operator

        Public Overloads Shared Operator -(ptr As Pointer(Of T), d As Integer) As Pointer(Of T)
            ptr.__index -= d
            Return ptr
        End Operator

        ''' <summary>
        ''' Pointer move to next and then returns the previous value
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(ptr As Pointer(Of T)) As T
            Dim i As Integer = ptr.__index
            ptr.__index += 1
            Return ptr.__innerRaw(i)
        End Operator

        Public Overloads Shared Operator -(ptr As Pointer(Of T)) As T
            Dim i As Integer = ptr.__index
            ptr.__index -= 1
            Return ptr.__innerRaw(i)
        End Operator
    End Class
End Namespace