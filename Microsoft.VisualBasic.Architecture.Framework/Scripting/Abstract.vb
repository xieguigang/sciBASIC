Namespace Scripting

    Public Module Abstract

        ''' <summary>
        ''' Gets the variable value from runtime.
        ''' </summary>
        ''' <param name="var$"></param>
        ''' <returns></returns>
        Public Delegate Function GetValue(var$) As Object
        ''' <summary>
        ''' How to make a function calls
        ''' </summary>
        ''' <param name="func$"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Delegate Function FunctionEvaluate(func$, args As Object()) As Object

    End Module
End Namespace