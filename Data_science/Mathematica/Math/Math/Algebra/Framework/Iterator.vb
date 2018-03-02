Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Framework

    Public Module Iterator

        Public MustInherit Class Kernel

            Protected Friend ReadOnly Property terminated As Boolean

            ''' <summary>
            ''' Execute the iterator step.
            ''' </summary>
            ''' <param name="itr"></param>
            Protected Friend MustOverride Sub [Step](itr As Integer)

        End Class

        <Extension>
        Public Sub Run(kernel As Kernel, Optional iterations% = 10 * 10000)
            Dim i As int = 0

            Do While ++i <= iterations AndAlso Not kernel.terminated
                Call kernel.Step(itr:=i)
            Loop
        End Sub
    End Module
End Namespace