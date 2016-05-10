Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading

Namespace Terminal

    Public Module Program

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="bar"></param>
        ''' <param name="wait">Sleep time of the thread</param>
        ''' <param name="[end]">Ends at this iteration</param>
        <Extension> Public Sub Run(bar As AbstractBar, wait As Integer, [end] As Integer)
            For cont As Integer = 0 To [end] - 1
                Call bar.[Step]()
                Call Thread.Sleep(wait)
            Next
        End Sub
    End Module
End Namespace