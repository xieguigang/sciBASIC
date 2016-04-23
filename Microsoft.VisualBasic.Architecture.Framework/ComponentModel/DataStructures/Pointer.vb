Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' Type of <see cref="Int32"/> pointer class to the <see cref="Array"/> class.
    ''' (<see cref="Int32"/>类型，一般用来进行数组操作的)
    ''' </summary>
    Public Class Pointer

        Protected __index As Integer
        ReadOnly __step As Integer

        ''' <summary>
        ''' Construct a pointer class and then assign a initial <see cref="Int32"/> value.(构造一个指针对象，并且赋值其初始值)
        ''' </summary>
        ''' <param name="n">The initial value.</param>
        Sub New(n As Integer)
            __index = n
        End Sub

        Private Sub New(n As Integer, [step] As Integer)
            __index = n
            __step = [step]
        End Sub

        ''' <summary>
        ''' 构造一个初始值为零的整形数指针对象
        ''' </summary>
        Sub New()
            Call Me.New(Scan0)
        End Sub

        Public Overrides Function ToString() As String
            Return __index
        End Function

        Public Shared Widening Operator CType(n As Integer) As Pointer
            Return New Pointer(n)
        End Operator

        Public Shared Narrowing Operator CType(n As Pointer) As Integer
            Return n.__index
        End Operator

        Public Overloads Shared Operator +(n As Pointer, x As Integer) As Pointer
            n.__index += x
            Return n
        End Operator

        Public Overloads Shared Operator +(x As Integer, n As Pointer) As Pointer
            n.__index += x
            Return n
        End Operator

        Public Overloads Shared Operator +(x As Pointer, n As Pointer) As Pointer
            Return New Pointer(n.__index + x.__index)
        End Operator

        Public Overloads Shared Operator <(x As Pointer, n As Integer) As Boolean
            Return x.__index < n
        End Operator

        Public Overloads Shared Operator >(x As Pointer, n As Integer) As Boolean
            Return x.__index > n
        End Operator

        ''' <summary>
        ''' 移动n，然后返回之前的数值
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Shared Operator <<(x As Pointer, n As Integer) As Integer
            Dim value As Integer = x.__index
            x.__index += n
            Return value
        End Operator

        ''' <summary>
        ''' 自增1，然后返回之前的数值
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(x As Pointer) As Integer
            Dim p As Integer = x.__index
            x.__index += 1
            Return p
        End Operator

        ''' <summary>
        ''' 自减1，然后返回之前的数值
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator -(x As Pointer) As Integer
            Dim p As Integer = x.__index
            x.__index -= 1
            Return p
        End Operator

        ''' <summary>
        ''' Setup the offsets
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="steps"></param>
        ''' <returns></returns>
        Public Shared Operator <=(x As Pointer, steps As Integer) As Pointer
            Return New Pointer(x.__index, steps)
        End Operator

        Public Shared Operator >=(x As Pointer, steps As Integer) As Pointer
            Throw New NotSupportedException
        End Operator
    End Class

    Public Class Pointer(Of T) : Inherits Pointer

        ''' <summary>
        ''' Returns current line in the array and then pointer moves to next.
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="i"></param>
        ''' <returns></returns>

        Public Overloads Shared Operator +(array As T(), i As Pointer(Of T)) As T
            Return array(+i)
        End Operator

        Public Overloads Shared Operator -(array As T(), i As Pointer(Of T)) As T
            Return array(-i)
        End Operator

        Public Overloads Shared Operator +(list As List(Of T), i As Pointer(Of T)) As T
            Return list(+i)
        End Operator

        Public Overloads Shared Operator -(list As List(Of T), i As Pointer(Of T)) As T
            Return list(-i)
        End Operator
    End Class
End Namespace