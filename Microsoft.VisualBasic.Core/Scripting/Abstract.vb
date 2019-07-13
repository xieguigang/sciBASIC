#Region "Microsoft.VisualBasic::581c915f18cd405a60353097d42aa234, Microsoft.VisualBasic.Core\Scripting\Abstract.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Abstract
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting

    ''' <summary>
    ''' Delegate function pointer interface for scripting internal.
    ''' </summary>
    Public Module Abstract

        ''' <summary>
        ''' Gets the variable value from runtime.
        ''' </summary>
        ''' <param name="var$"></param>
        ''' <returns></returns>
        Public Delegate Function GetValue(var$) As Object
        ''' <summary>
        ''' Set the variable value to the variable.
        ''' </summary>
        ''' <param name="var$"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Delegate Function SetValue(var$, value As Object) As Boolean

        ''' <summary>
        ''' How to make a function calls.(这个是在已经知道了确切的函数对象的前体下才会使用这个进行调用)
        ''' </summary>
        ''' <param name="func$">The function name in the scripting runtime</param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Delegate Function FunctionEvaluate(func$, args As Object()) As Object

        ''' <summary>
        ''' 在脚本编程之中将用户输入的字符串数据转换为目标类型的方法接口
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Delegate Function LoadObject(value$) As Object
        Public Delegate Function LoadObject(Of T)(value$) As T
    End Module
End Namespace
