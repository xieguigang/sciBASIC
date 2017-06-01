
Imports System.Reflection

Namespace Emit.Delegates

    ''' <summary>
    ''' 将属性的<see cref="PropertyInfo.SetValue(Object, Object)"/>编译为方法调用
    ''' </summary>
    Public Class SetValueHelper(Of T)

        ReadOnly type As Type = GetType(T)

        ' Public Property Evaluate (Of V)(name$) As V()
        


    End Class
End Namespace