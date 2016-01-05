Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Globalization

Namespace Microsoft.VisualBasic
    Friend NotInheritable Class FormatInfoHolder
        Implements IFormatProvider
        ' Methods
        Friend Sub New(nfi As NumberFormatInfo)
            Me.nfi = nfi
        End Sub

        Private Function GetFormat(service As Type) As Object Implements IFormatProvider.GetFormat
            If (Not service Is GetType(NumberFormatInfo)) Then
                Throw New ArgumentException(Utils.GetResourceString("InternalError"))
            End If
            Return Me.nfi
        End Function


        ' Fields
        Private nfi As NumberFormatInfo
    End Class
End Namespace

