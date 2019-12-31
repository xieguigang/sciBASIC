Imports System.Text

Namespace CommandLine.Reflection

    ''' <summary>
    ''' Data reader and view for <see cref="ExportAPIAttribute"/>
    ''' </summary>
    Public Class ExportApiInformation : Implements IExportAPI

        Public ReadOnly Property Name As String Implements IExportAPI.Name
        Public ReadOnly Property Info As String Implements IExportAPI.Info
        Public ReadOnly Property Usage As String Implements IExportAPI.Usage
        Public ReadOnly Property Example As String Implements IExportAPI.Example

        Sub New(info As IExportAPI)
            Me.Name = info.Name
            Me.Info = info.Info
            Me.Usage = info.Usage
            Me.Example = info.Example
        End Sub

        Public Function PrintView(HTML As Boolean) As String
            If HTML Then
                Return printViewHTML()
            Else
                Return printView()
            End If
        End Function

        Private Function printView()
            Dim sb As New StringBuilder(1024)

            Call sb.AppendLine($"{NameOf(Name)}    = ""{Name}""")
            Call sb.AppendLine($"{NameOf(Info)}    = ""{Info}""")
            Call sb.AppendLine($"{NameOf(Usage)}   = ""{Usage}""")
            Call sb.AppendLine($"{NameOf(Example)} = ""{Example}""")

            Return sb.ToString
        End Function

        Private Function printViewHTML() As String
            Return ExportApiInformation.GenerateHtmlDoc(Me, "", "")
        End Function

        Public Shared Function GenerateHtmlDoc(Command As IExportAPI, addNode As String, addValue As String) As String
            Dim add As String = If(Not String.IsNullOrEmpty(addValue), $"           <tr>
    <td>{addNode}</td>
    <td>{addValue}</td>
  </tr>", "")

            Return $"<p>Help for ""{Command.Name}"":</p>
<table frame=""hsides"">
  <tr>
    <th>DocNode</th>
    <th>Content Text</th>
                 <th><a href=""#""><strong><font size=3>[&#8593;]</font></strong></a></th>
  </tr>
  <tr>
    <td><strong>{NameOf(Name)}</strong></td>
    <td><strong><a name=""{Command.Name}"">{Command.Name}</a></strong></td>
  </tr>
                <tr>
    <td>{NameOf(Info)}</td>
    <td>{Command.Info}</td>
  </tr>
                <tr>
    <td>{NameOf(Usage)}</td>
    <td>{Command.Usage}</td>
  </tr>
                <tr>
    <td>{NameOf(Example)}</td>
    <td>{Command.Example}</td>
  </tr>
    {add}
</table>"
        End Function
    End Class
End Namespace