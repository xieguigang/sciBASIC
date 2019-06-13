Imports System.Runtime.CompilerServices

Namespace Linq

    Public Module PipelineExtensions

        ''' <summary>
        ''' Delegate pipeline function
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="Tout"></typeparam>
        ''' <param name="input"></param>
        ''' <param name="apply"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function DoCall(Of T, Tout)(input As T, apply As Func(Of T, Tout)) As Tout
            Return apply(input)
        End Function

        ''' <summary>
        ''' Delegate pipeline function
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="input"></param>
        ''' <param name="apply"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub DoCall(Of T)(input As T, apply As Action(Of T))
            Call apply(input)
        End Sub
    End Module
End Namespace