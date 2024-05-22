#Region "Microsoft.VisualBasic::76334608bbcff3839c42f84eed4ddab8, Microsoft.VisualBasic.Core\src\Extensions\Security\SecurityString.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 59
    '    Code Lines: 36 (61.02%)
    ' Comment Lines: 12 (20.34%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (18.64%)
    '     File Size: 2.55 KB


    '     Class SecurityStringModel
    ' 
    '         Function: ToString
    ' 
    '         Sub: (+2 Overloads) Dispose
    '         Interface ISecurityStringModel
    ' 
    '             Function: Decrypt, DecryptString, Encrypt, EncryptData
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace SecurityString

    Public MustInherit Class SecurityStringModel
        Implements IDisposable
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
