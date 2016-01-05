Namespace ServicesComponents

    Public MustInherit Class InternalServicesModule : Implements System.IDisposable

        Protected _ServicesSocket As Net.TcpSynchronizationServicesSocket
        Protected _ShoalShell As Microsoft.VisualBasic.Scripting.ShoalShell.Runtime.ScriptEngine
        Protected ProtocolHandler As Net.Protocol.Reflection.ProtocolHandler

        Protected MustOverride Sub ImportsAPI()
        Protected MustOverride Function GetServicesPort() As Integer

        Protected Sub _runningServicesProtocol(Protocol As PbsProtocol)
            ProtocolHandler = New Net.Protocol.Reflection.ProtocolHandler(Protocol)
            _ServicesSocket = New Net.TcpSynchronizationServicesSocket(GetServicesPort)
            _ServicesSocket.Responsehandler = AddressOf ProtocolHandler.HandleRequest
            _ServicesSocket.Run()
        End Sub

        Protected Sub WaitForSocketStart()
            Do While _ServicesSocket Is Nothing
                Call Threading.Thread.Sleep(10)
            Loop
        End Sub

        Protected Sub _runningShoalShell()
            _ShoalShell = New Scripting.ShoalShell.Runtime.ScriptEngine()
            Call ImportsAPI()

            Do While Not _ServicesSocket Is Nothing
                Call Console.Write(">>> ")
                Dim cmdl As String = Console.ReadLine
                Call _ShoalShell.Exec(cmdl)
            Loop
        End Sub

        Public MustOverride Sub RunServices(argvs As CommandLine.CommandLine)

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call _ShoalShell.Free
                    Call _ServicesSocket.Free
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace