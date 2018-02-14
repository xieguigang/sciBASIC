Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language.C

Namespace Text.Xml

    Public Module HtmlBuilder

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function sprintf(format As XElement, ParamArray args As Object()) As String
            Return CLangStringFormatProvider.sprintf(format.ToString, args)
        End Function

        ''' <summary>
        ''' Appends a copy of the specified string followed by the default line terminator
        ''' to the end of the current <see cref="StringBuilder"/> object.
        ''' </summary>
        ''' <param name="sb"></param>
        ''' <param name="html">The html string to append.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' A reference to this instance after the append operation has completed.
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AppendLine(sb As StringBuilder, html As XElement, ParamArray args As Object()) As StringBuilder
            If args.IsNullOrEmpty Then
                Return sb.AppendLine(html.ToString)
            Else
                Return sb.AppendLine(CLangStringFormatProvider.sprintf(html.ToString, args))
            End If
        End Function
    End Module
End Namespace