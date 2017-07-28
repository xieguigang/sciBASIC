Namespace Scripting

    Public Module Abstract

        ''' <summary>
        ''' Gets the variable value from runtime.
        ''' </summary>
        ''' <param name="var$"></param>
        ''' <returns></returns>
        Public Delegate Function GetValue(var$) As Object
        ''' <summary>
        ''' How to make a function calls.(这个是在已经知道了确切的函数对象的前体下才会使用这个进行调用)
        ''' </summary>
        ''' <param name="func$"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Delegate Function FunctionEvaluate(func$, args As Object()) As Object

    End Module
End Namespace