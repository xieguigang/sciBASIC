Imports System
Imports System.Runtime.CompilerServices

Namespace UMAP
    Public NotInheritable Class DefaultRandomGenerator
        Implements UMAP.IProvideRandomValues
        ''' <summary>
        ''' This is the default configuration (it supports the optimization process to be executed on multiple threads)
        ''' </summary>
        Public Shared ReadOnly Property Instance As UMAP.DefaultRandomGenerator = New UMAP.DefaultRandomGenerator(allowParallel:=True)

        ''' <summary>
        ''' This uses the same random number generator but forces the optimization process to run on a single thread (which may be desirable if multiple requests may be processed concurrently
        ''' or if it is otherwise not desirable to let a single request access all of the CPUs)
        ''' </summary>
        Public Shared ReadOnly Property DisableThreading As UMAP.DefaultRandomGenerator = New UMAP.DefaultRandomGenerator(allowParallel:=False)

        Private Sub New(ByVal allowParallel As Boolean)
            CSharpImpl.__Assign(IsThreadSafe, allowParallel)
        End Sub

        Public ReadOnly Property IsThreadSafe As Boolean Implements IProvideRandomValues.IsThreadSafe

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Next](ByVal minValue As Integer, ByVal maxValue As Integer) As Integer Implements IProvideRandomValues.Next
            Return UMAP.ThreadSafeFastRandom.Next(minValue, maxValue)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function NextFloat() As Single Implements IProvideRandomValues.NextFloat
            Return UMAP.ThreadSafeFastRandom.NextFloat()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub NextFloats(ByVal buffer As Span(Of Single)) Implements IProvideRandomValues.NextFloats
            UMAP.ThreadSafeFastRandom.NextFloats(buffer)
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class
End Namespace
