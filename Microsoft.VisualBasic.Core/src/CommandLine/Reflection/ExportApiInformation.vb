#Region "Microsoft.VisualBasic::13c0d0aa24d0f0328ded6d32f18d33d1, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\ExportApiInformation.vb"

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

    '   Total Lines: 78
    '    Code Lines: 64
    ' Comment Lines: 3
    '   Blank Lines: 11
    '     File Size: 2.53 KB


    '     Class ExportApiInformation
    ' 
    '         Properties: Example, Info, Name, Usage
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GenerateHtmlDoc, printView, PrintView, printViewHTML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
