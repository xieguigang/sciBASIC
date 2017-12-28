Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Vectorization

Namespace Language

    Public Module AssertEqualsExtensions

        ''' <summary>
        ''' Assert that all of the elements in target vector match the test conditions
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <Extension>
        Public Function All(Of T)(vector As Vector(Of T)) As AssertAll(Of T)
            Return New AssertAll(Of T)(vector, Function(x, y) x = y)
        End Function

        ''' <summary>
        ''' Assert that any of the elements in target vector match the test conditions
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="vector"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Any(Of T)(vector As Vector(Of T)) As AssertAny(Of T)
            Return New AssertAny(Of T)(vector, Function(x, y) x = y)
        End Function
    End Module
End Namespace