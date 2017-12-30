Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.Vectorization

Namespace Language

    Public Class VectorAssert(Of T)

        Protected ReadOnly vector As Vector(Of T)
        Protected ReadOnly assert As BinaryAssert(Of Object)

        Default Public ReadOnly Property RunAssert(i%, obj As T) As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return assert(vector(index:=i), obj)
            End Get
        End Property

        Sub New(vector As Vector(Of T), assert As BinaryAssert(Of Object))
            Me.vector = vector
            Me.assert = assert
        End Sub
    End Class

    Public Class AssertAll(Of T) : Inherits VectorAssert(Of T)

        Sub New(vector As Vector(Of T), assert As BinaryAssert(Of Object))
            Call MyBase.New(vector, assert)
        End Sub

        ''' <summary>
        ''' Does the elements in this vector all equals to a specific value <paramref name="x"/>?
        ''' </summary>
        ''' <param name="assert"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(assert As AssertAll(Of T), x As T) As Boolean
            Return assert.vector.Sequence.All(Function(i) assert(i, x))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(assert As AssertAll(Of T), x As T) As Boolean
            Return Not assert = x
        End Operator
    End Class

    Public Class AssertAny(Of T) : Inherits VectorAssert(Of T)

        Sub New(vector As Vector(Of T), assert As BinaryAssert(Of Object))
            Call MyBase.New(vector, assert)
        End Sub

        ''' <summary>
        ''' Does the elements in this vector all equals to a specific value <paramref name="x"/>?
        ''' </summary>
        ''' <param name="assert"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(assert As AssertAny(Of T), x As T) As Boolean
            Return assert.vector.Sequence.Any(Function(i) assert(i, x))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(assert As AssertAny(Of T), x As T) As Boolean
            Return Not assert = x
        End Operator
    End Class
End Namespace