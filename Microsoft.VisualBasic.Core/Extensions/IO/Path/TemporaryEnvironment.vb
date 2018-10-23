Namespace FileIO

    ''' <summary>
    ''' 在<see cref="TemporaryEnvironment.Dispose()"/>的时候，文件夹会切换回来
    ''' </summary>
    Public Class TemporaryEnvironment : Inherits Directory
        Implements IDisposable

        ReadOnly previous$

        ''' <summary>
        ''' 如果不存在会创建目标文件夹
        ''' </summary>
        ''' <param name="newLocation"></param>
        Sub New(newLocation As String)
            Call MyBase.New(DIR:=makeDirectoryExists(newLocation))

            previous = App.CurrentDirectory
            App.CurrentDirectory = newLocation
        End Sub

        Private Shared Function makeDirectoryExists(dir As String) As String
            If Not dir.DirectoryExists Then
                Call dir.MkDIR
            End If

            Return dir
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    App.CurrentDirectory = previous
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
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