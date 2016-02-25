Imports System.Runtime.InteropServices.Marshal

Namespace Marshal

    Public Delegate Sub UnsafeCopy(Of T)(source As System.IntPtr, destination As T(), startIndex As Integer, length As Integer)

    ''' <summary>
    ''' 内存指针
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class IntPtr(Of T)

        ''' <summary>
        ''' 第一个位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Scan0 As System.IntPtr

        Protected __innerRaw As T()
        Dim _current As Integer

        Public ReadOnly Property Current As T
            Get
                Return Value(_current)
            End Get
        End Property

        Default Public Property Value(p As Integer) As T
            Get
                If p < 0 OrElse p >= __innerRaw.Length Then
                    Return Nothing
                Else
                    Return __innerRaw(p)
                End If
            End Get
            Set(value As T)
                __innerRaw(p) = value
            End Set
        End Property

        Sub New(p As System.IntPtr, chunkSize As Integer, unsafe As UnsafeCopy(Of T))
            Scan0 = p
            __innerRaw = New T(chunkSize - 1) {}
            Call unsafe(Scan0, __innerRaw, 0, __innerRaw.Length)
        End Sub

        Public Overrides Function ToString() As String
            Return $"* {GetType(T).Name} + {_current} --> {Current}  // {Scan0.ToString}"
        End Function

        Public Shared Operator +(ptr As IntPtr(Of T), d As Integer) As T
            ptr._current += d
            Return ptr.Current
        End Operator

        Public Shared Operator -(ptr As IntPtr(Of T), d As Integer) As T
            ptr._current -= d
            Return ptr.Current
        End Operator
    End Class
End Namespace