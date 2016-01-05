Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace SecurityString

    Public MustInherit Class SecurityStringModel
        Implements System.IDisposable
        Implements ISecurityStringModel

        Protected Friend strPassphrase As String = "<Guid(""768DF93E-AC45-4AD5-A0D9-C143CDA5BC55"")>"

        Public Overrides Function ToString() As String
            Return strPassphrase
        End Function

        Public MustOverride Function Encrypt(input() As Byte) As Byte() Implements ISecurityStringModel.Encrypt
        Public MustOverride Function EncryptData(text As String) As String Implements ISecurityStringModel.EncryptData
        Public MustOverride Function Decrypt(input() As Byte) As Byte() Implements ISecurityStringModel.Decrypt
        Public MustOverride Function DecryptString(text As String) As String Implements ISecurityStringModel.DecryptString

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    strPassphrase = ""
                    Call FlushMemory()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

        Public Interface ISecurityStringModel
            Function Encrypt(input() As Byte) As Byte()
            Function EncryptData(text As String) As String
            Function Decrypt(input() As Byte) As Byte()
            Function DecryptString(text As String) As String
        End Interface

    End Class
End Namespace