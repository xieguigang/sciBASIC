Imports System.Runtime.CompilerServices

Namespace Language

    Public Class ArgumentReference

        Public name$, value

        Public Overrides Function ToString() As String
            Return $"Dim {name} As Object = {Scripting.ToString(value, "null")}"
        End Function

        ''' <summary>
        ''' Argument variable value assign
        ''' </summary>
        ''' <param name="var">The argument name</param>
        ''' <param name="value">argument value</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(var As ArgumentReference, value As Object) As ArgumentReference
            var.value = value
            Return var
        End Operator

        Public Shared Operator <>(var As ArgumentReference, value As Object) As ArgumentReference
            Throw New NotImplementedException
        End Operator
    End Class

    ''' <summary>
    ''' ```vbnet
    ''' Imports VB = Microsoft.VisualBasic.Language.Runtime
    ''' 
    ''' With New VB
    '''     ' ...
    ''' End With
    ''' ```
    ''' </summary>
    Public Class Runtime

        ''' <summary>
        ''' Language syntax supports for argument list
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Argument(name$) As ArgumentReference
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New ArgumentReference With {
                    .name = name
                }
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return "sciBASIC for VB.NET language runtime API"
        End Function
    End Class
End Namespace