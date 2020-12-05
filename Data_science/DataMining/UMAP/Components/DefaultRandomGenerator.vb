Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math

Public NotInheritable Class DefaultRandomGenerator
    Implements IProvideRandomValues
    ''' <summary>
    ''' This is the default configuration (it supports the optimization process to be executed on multiple threads)
    ''' </summary>
    Public Shared ReadOnly Property Instance As DefaultRandomGenerator = New DefaultRandomGenerator(allowParallel:=True)

    ''' <summary>
    ''' This uses the same random number generator but forces the optimization process to run on a single thread (which may be desirable if multiple requests may be processed concurrently
    ''' or if it is otherwise not desirable to let a single request access all of the CPUs)
    ''' </summary>
    Public Shared ReadOnly Property DisableThreading As DefaultRandomGenerator = New DefaultRandomGenerator(allowParallel:=False)

    Private Sub New(allowParallel As Boolean)
        IsThreadSafe = allowParallel
    End Sub

    Public ReadOnly Property IsThreadSafe As Boolean Implements IProvideRandomValues.IsThreadSafe

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [Next](minValue As Integer, maxValue As Integer) As Integer Implements IProvideRandomValues.Next
        Return ThreadSafeFastRandom.Next(minValue, maxValue)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function NextFloat() As Single Implements IProvideRandomValues.NextFloat
        Return ThreadSafeFastRandom.NextFloat()
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub NextFloats(buffer As Single()) Implements IProvideRandomValues.NextFloats
        ThreadSafeFastRandom.NextFloats(buffer)
    End Sub
End Class
