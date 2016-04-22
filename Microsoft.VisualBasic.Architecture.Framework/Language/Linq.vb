Imports Microsoft.VisualBasic.Linq

Namespace Language

    ''' <summary>
    ''' Language syntax extension for the Linq expression in VisualBasic language
    ''' </summary>
    Public Module LinqAPI

        ''' <summary>
        ''' Initializes a new instance of the <see cref="List"/>`1 class that
        ''' contains elements copied from the specified collection and has sufficient capacity
        ''' to accommodate the number of elements copied.
        ''' </summary>
        Public Function MakeList(Of T)() As ListHelper(Of T)
            Return New ListHelper(Of T)
        End Function

        ''' <summary>
        ''' Execute a linq expression. Creates an array from a <see cref="System.Collections.Generic.IEnumerable(Of T)"/>.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns>An array that contains the elements from the input sequence.</returns>
        Public Function Exec(Of T)() As ExecHelper(Of T)
            Return New ExecHelper(Of T)
        End Function

        Public Function DefaultFirst(Of T)(Optional [default] As T = Nothing) As FirstOrDefaultHelper(Of T)
            Return New FirstOrDefaultHelper(Of T)([default])
        End Function

        Public Function Exec(Of T, V)(source As IEnumerable(Of T)) As ToArrayHelper(Of T, V)
            Return New ToArrayHelper(Of T, V)(source)
        End Function
    End Module

    Public Structure ListHelper(Of T)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="List"/>`1 class that
        ''' contains elements copied from the specified collection and has sufficient capacity
        ''' to accommodate the number of elements copied.
        ''' </summary>
        ''' <param name="linq">The collection whose elements are copied to the new list.</param>
        ''' <returns></returns>
        Public Shared Operator <=(cls As ListHelper(Of T), linq As IEnumerable(Of T)) As List(Of T)
            Return linq.ToList
        End Operator

        Public Shared Operator >=(cls As ListHelper(Of T), linq As IEnumerable(Of T)) As List(Of T)
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' Initializes a new instance of the <see cref="List"/>`1 class that
        ''' contains elements copied from the specified collection and has sufficient capacity
        ''' to accommodate the number of elements copied.
        ''' </summary>
        ''' <param name="linq">The collection whose elements are copied to the new list.</param>
        ''' <returns></returns>
        Public Shared Operator <=(cls As ListHelper(Of T), linq As IEnumerable(Of IEnumerable(Of T))) As List(Of T)
            Return linq.MatrixToList
        End Operator

        Public Shared Operator >=(cls As ListHelper(Of T), linq As IEnumerable(Of IEnumerable(Of T))) As List(Of T)
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' Initializes a new instance of the <see cref="List"/>`1 class that
        ''' contains elements copied from the specified collection and has sufficient capacity
        ''' to accommodate the number of elements copied.
        ''' </summary>
        ''' <param name="linq">The collection whose elements are copied to the new list.</param>
        ''' <returns></returns>
        Public Shared Operator <=(cls As ListHelper(Of T), linq As IEnumerable(Of IEnumerable(Of IEnumerable(Of T)))) As List(Of T)
            Return linq.MatrixAsIterator.MatrixToList
        End Operator

        Public Shared Operator >=(cls As ListHelper(Of T), linq As IEnumerable(Of IEnumerable(Of IEnumerable(Of T)))) As List(Of T)
            Throw New NotSupportedException
        End Operator
    End Structure

    ''' <summary>
    ''' Execute a linq expression. Creates an array from a <see cref="System.Collections.Generic.IEnumerable(Of T)"/>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure ExecHelper(Of T)

        ''' <summary>
        ''' Creates an array from a <see cref="System.Collections.Generic.IEnumerable(Of T)"/>.
        ''' </summary>
        ''' <param name="cls"></param>
        ''' <param name="linq">
        ''' An <see cref="System.Collections.Generic.IEnumerable(Of T)"/> to create an array from.
        ''' </param>
        ''' <returns>An array that contains the elements from the input sequence.</returns>
        Public Shared Operator <=(cls As ExecHelper(Of T), linq As IEnumerable(Of T)) As T()
            Return linq.ToArray
        End Operator

        Public Shared Operator >=(cls As ExecHelper(Of T), linq As IEnumerable(Of T)) As T()
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' Creates an array from a <see cref="System.Collections.Generic.IEnumerable(Of T)"/>.
        ''' </summary>
        ''' <param name="cls"></param>
        ''' <param name="linq">
        ''' An <see cref="System.Collections.Generic.IEnumerable(Of T)"/> to create an array from.
        ''' </param>
        ''' <returns>An array that contains the elements from the input sequence.</returns>
        Public Shared Operator <=(cls As ExecHelper(Of T), linq As IEnumerable(Of IEnumerable(Of T))) As T()
            Return linq.MatrixToVector
        End Operator

        Public Shared Operator >=(cls As ExecHelper(Of T), linq As IEnumerable(Of IEnumerable(Of T))) As T()
            Throw New NotSupportedException
        End Operator
    End Structure

    Public Structure FirstOrDefaultHelper(Of T)

        ReadOnly __default As T

        Sub New(__default As T)
            Me.__default = __default
        End Sub

        ''' <summary>
        ''' Exec ToArray
        ''' </summary>
        ''' <param name="cls"></param>
        ''' <param name="linq"></param>
        ''' <returns></returns>
        Public Shared Operator <=(cls As FirstOrDefaultHelper(Of T), linq As IEnumerable(Of T)) As T
            Return linq.DefaultFirst(cls.__default)
        End Operator

        Public Shared Operator >=(cls As FirstOrDefaultHelper(Of T), linq As IEnumerable(Of T)) As T
            Throw New NotSupportedException
        End Operator
    End Structure

    ''' <summary>
    ''' Execute a linq expression. Creates an array from a <see cref="System.Collections.Generic.IEnumerable(Of T)"/>.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Structure ToArrayHelper(Of T, V)

        Private __source As IEnumerable(Of T)

        Sub New(source As IEnumerable(Of T))
            __source = source
        End Sub

        ''' <summary>
        ''' Exec ToArray
        ''' </summary>
        ''' <param name="cls"></param>
        ''' <param name="linq"></param>
        ''' <returns></returns>
        Public Shared Operator <=(cls As ToArrayHelper(Of T, V), linq As Func(Of T, V)) As V()
            Return (From x As T In cls.__source Select linq(x)).ToArray
        End Operator

        Public Shared Operator >=(cls As ToArrayHelper(Of T, V), linq As Func(Of T, V)) As V()
            Throw New NotSupportedException
        End Operator
    End Structure
End Namespace