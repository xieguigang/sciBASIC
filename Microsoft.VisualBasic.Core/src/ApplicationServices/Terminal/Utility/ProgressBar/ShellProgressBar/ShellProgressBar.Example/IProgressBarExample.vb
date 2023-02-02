Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example
    Public Interface IProgressBarExample
        Function Start(token As CancellationToken) As Task
    End Interface
End Namespace
