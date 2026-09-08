Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Script

    Public Class ScriptRuntime : Implements IDisposable

        Dim disposedValue As Boolean
        Dim ctx As ScriptLoadContext
        Dim asm As Assembly

        Sub New(ctx As ScriptLoadContext, asm As Assembly)
            Me.ctx = ctx
            Me.asm = asm
        End Sub

        Public Function Run(args As String()) As Integer
            Return DynamicDll.Run(asm, CommandArgs(args))
        End Function

        Private Function CommandArgs(args As String()) As Object
            Dim type As Type = ctx.GetType(asm, GetType(CommandLine).FullName)
            Dim ctor As MethodInfo = type.GetMethod(NameOf(CommandLine.BuildFromArguments), {GetType(String()), GetType(Boolean)})
            Dim cmdl As Object = ctor.Invoke(Nothing, {args, False})

            Return cmdl
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call ctx.Unload()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace