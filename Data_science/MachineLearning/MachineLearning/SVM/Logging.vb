Imports System.IO

Namespace SVM

    Public Class Logging

        ''' <summary>
        ''' Whether the system will output information to the console during the training process.
        ''' </summary>
        Public Shared Property IsVerbose As Boolean

        Shared svm_print_stdout As TextWriter = Console.Out

        Public Shared Sub flush()
            SyncLock svm_print_stdout
                Call svm_print_stdout.Flush()
            End SyncLock
        End Sub

        Public Shared Sub info(s As String)
            If _IsVerbose Then
                SyncLock svm_print_stdout
                    Call svm_print_stdout.Write(s)
                End SyncLock
            End If
        End Sub
    End Class
End Namespace