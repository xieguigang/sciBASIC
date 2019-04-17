Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel

Namespace Text.Xml.Linq

    ''' <summary>
    ''' Write a very large dataset in Xml format
    ''' </summary>
    Public Class DataSetWriter(Of T) : Implements IDisposable

        Dim file As StreamWriter
        Dim offsetLength As Integer = NodeIterator.XmlDeclare.Length
        Dim indentBlank$ = "   "

        Sub New(file As String, Optional encoding As Encodings = Encodings.UTF8)
            Me.file = file.OpenWriter(encoding)
            Me.file.WriteLine(NodeIterator.XmlDeclare)
            Me.file.WriteLine($"<DataSetOf{GetType(T).Name} xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
            Me.file.WriteLine(indentBlank & "<!--")
            Me.file.WriteLine(XmlDataModel.GetTypeReferenceComment(GetType(T), 6))
            Me.file.WriteLine(indentBlank & "-->")
        End Sub

        Public Sub Write(data As T)
            Dim xml As String() = data.GetXml.LineTokens.Skip(1).ToArray

            xml(0) = xml(0) _
                .Replace("xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "") _
                .Replace("xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "") _
                .Replace("  ", "")

            For Each line As String In xml
                Call file.Write(indentBlank)
                Call file.WriteLine(line)
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Flush()
            Call file.Flush()
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call file.WriteLine($"</DataSetOf{GetType(T).Name}>")
                    Call file.Flush()
                    Call file.Close()
                    Call file.Dispose()
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