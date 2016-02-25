Imports System.Runtime.InteropServices.Marshal

Namespace Marshal

    Public Delegate Sub UnsafeCopys(Of T)(source As System.IntPtr, destination As T(), startIndex As Integer, length As Integer)
    Public Delegate Sub UnsafeWrite(Of T)(destination As T(), startIndex As Integer, source As System.IntPtr, length As Integer)

    ''' <summary>
    ''' 内存指针
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class IntPtr(Of T) : Implements IDisposable

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
                If p < 0 OrElse p >= __innerRaw.Length Then
                    Throw New MemberAccessException(p & " reference to invalid memory region!")
                Else
                    __innerRaw(p) = value
                End If
            End Set
        End Property

        Sub New(p As System.IntPtr, chunkSize As Integer, unsafeCopys As UnsafeCopys(Of T), unsafeWrite As UnsafeWrite(Of T))
            Scan0 = p
            __innerRaw = New T(chunkSize - 1) {}
            Call unsafeCopys(Scan0, __innerRaw, 0, __innerRaw.Length)
        End Sub

        ''' <summary>
        ''' 方便进行数组操作的
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="p"></param>
        Sub New(ByRef raw As T(), Optional p As System.IntPtr = Nothing)
            __innerRaw = raw
            Scan0 = p
        End Sub

        Dim __writeMemory As UnsafeWrite(Of T)

        Private Sub __unsafeWrite(p As System.IntPtr)
            Call __writeMemory(__innerRaw, 0, p, __innerRaw.Length)
        End Sub

        ''' <summary>
        ''' Unsafe write memory
        ''' </summary>
        Public Sub Write()
            Call __unsafeWrite(Scan0)
        End Sub

        Public Sub Write(des As System.IntPtr)
            Call __unsafeWrite(des)
        End Sub

        Public Overrides Function ToString() As String
            Return $"* {GetType(T).Name} + {_current} --> {Current}  // {Scan0.ToString}"
        End Function

        Public Shared Operator +(ptr As IntPtr(Of T), d As Integer) As IntPtr(Of T)
            ptr._current += d
            Return ptr
        End Operator

        Public Shared Operator -(ptr As IntPtr(Of T), d As Integer) As IntPtr(Of T)
            ptr._current -= d
            Return ptr
        End Operator

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Write()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace