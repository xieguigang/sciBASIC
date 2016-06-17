Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Terminal.STDIO__

Namespace Language

    Public Module FormatHelpers

        ''' <summary>
        ''' <see cref="sprintf"/>
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension>
        Public Function xFormat(s As String) As FormatHelper
            Return New FormatHelper With {.source = s}
        End Function
    End Module

    Public Structure FormatHelper

        Dim source As String

        Public Overrides Function ToString() As String
            Return source
        End Function

        Public Shared Operator <=(format As FormatHelper, args As String()) As String
            Return sprintf(format.source, args)
        End Operator

        Public Shared Operator >=(format As FormatHelper, args As String()) As String
            Throw New NotSupportedException
        End Operator
    End Structure
End Namespace