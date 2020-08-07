Imports System.Threading
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace CommandLine

    Public Structure ProcessEx : Implements IIORedirectAbstract

        Public Property Bin As String Implements IIORedirectAbstract.Bin
        Public Property CLIArguments As String Implements IIORedirectAbstract.CLIArguments

        Public ReadOnly Property StandardOutput As String Implements IIORedirectAbstract.StandardOutput
            Get
                Throw New NotSupportedException
            End Get
        End Property

        Public Event ProcessExit(exitCode As Integer, exitTime As String) Implements IIORedirectAbstract.ProcessExit

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function Run() As Integer Implements IIORedirectAbstract.Run
            Return Start(True)
        End Function

        Public Function Start(Optional waitForExit As Boolean = False) As Integer Implements IIORedirectAbstract.Start
            Dim proc As New Process

            Try
                proc.StartInfo = New ProcessStartInfo(Bin, CLIArguments)
                proc.Start()
            Catch ex As Exception
                ex = New Exception(Me.GetJson, ex)
                Throw ex
            End Try

            If waitForExit Then
                Call wait(proc)
                Return proc.ExitCode
            Else
                Dim h As Action(Of Process) = AddressOf wait
                Call New Thread(Sub() Call h(proc)).Start()
                Return 0
            End If
        End Function

        Private Sub wait(proc As Process)
            Call proc.WaitForExit()
            RaiseEvent ProcessExit(proc.ExitCode, Now.ToString)
        End Sub
    End Structure
End Namespace