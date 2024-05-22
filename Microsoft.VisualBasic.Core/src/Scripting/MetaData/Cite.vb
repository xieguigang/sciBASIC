#Region "Microsoft.VisualBasic::7832f705d2d954f961650e5493d73356, Microsoft.VisualBasic.Core\src\Scripting\MetaData\Cite.vb"

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

    '   Total Lines: 156
    '    Code Lines: 99 (63.46%)
    ' Comment Lines: 42 (26.92%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (9.62%)
    '     File Size: 7.69 KB


    '     Class Cite
    ' 
    '         Properties: Abstract, AuthorAddress, Authors, DOI, ISSN
    '                     Issue, Journal, Keywords, Notes, Pages
    '                     PubMed, StartPage, Title, URL, Volume
    '                     Year
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: __formatAbstractPreview, (+2 Overloads) GetCiteList, HTML, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Scripting.MetaData

    ''' <summary>
    ''' EndNote tags
    ''' </summary>
    <AttributeUsage(AttributeTargets.Module Or AttributeTargets.Method Or AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class Cite : Inherits Attribute

        ''' <summary>
        ''' author(eMail);author(eMail)
        ''' </summary>
        ''' <returns></returns>
        Public Property Authors As String
        Public Property Year As Integer
        Public Property Title As String
        Public Property Journal As String
        Public Property Volume As Integer
        Public Property Issue As String
        Public Property Pages As String
        Public Property StartPage As Integer
        ''' <summary>
        ''' ISSN_1; ISSN_2; ISSN_3
        ''' </summary>
        ''' <returns></returns>
        Public Property ISSN As String
        Public Property DOI As String
        Public Property PubMed As Integer
        Public Property Keywords As String
        ''' <summary>
        ''' 文章的摘要，可以使用&lt;p>或者&lt;br/>来进行分段
        ''' </summary>
        ''' <returns></returns>
        Public Property Abstract As String
        Public Property Notes As String
        Public Property URL As String
        Public Property AuthorAddress As String

        ''' <summary>
        ''' %x 是文章的摘要，可以使用&lt;p>或者&lt;br/>来进行分段
        ''' </summary>
        ''' <param name="EndNoteImports"></param>
        ''' <remarks>
        ''' %A Griffiths-Jones, Sam
        ''' %A Bateman, Alex
        ''' %A Marshall, Mhairi
        ''' %A Khanna, Ajay
        ''' %A Eddy, Sean R.
        ''' %T Rfam: an RNA family database
        ''' %0 Journal Article
        ''' %D 2003 
        ''' %8 January 1, 2003 
        ''' %J Nucleic Acids Research 
        ''' %P 439-441 
        ''' %R 10.1093/nar/gkg006 
        ''' %V 31 
        ''' %N 1 
        ''' %U http://nar.oxfordjournals.org/content/31/1/439.abstract 
        ''' %X Rfam Is a collection of multiple sequence alignments And covariance models representing non-coding RNA families. Rfam Is available on the web in the UK at http://www.sanger.ac.uk/Software/Rfam/ And in the US at http://rfam.wustl.edu/. These websites allow the user to search a query sequence against a library of covariance models, And view multiple sequence alignments And family annotation. The database can also be downloaded in flatfile form And searched locally using the INFERNAL package (http://infernal.wustl.edu/). The first release of Rfam (1.0) contains 25 families, which annotate over 50 000 non-coding RNA genes in the taxonomic divisions of the EMBL nucleotide database. 
        ''' </remarks>
        Sub New(EndNoteImports As String)
            Dim lines As String() = EndNoteImports.LineTokens
            Dim dict As Dictionary(Of String, String()) = (From line As String In lines
                                                           Where Not String.IsNullOrWhiteSpace(line)
                                                           Let flag As String = line.Split.First
                                                           Let value As String = Mid(line, Len(flag) + 1).Trim
                                                           Select flag, value
                                                           Group By flag Into Group) _
                                                                .ToDictionary(Function(x) x.flag,
                                                                              Function(x) x.Group.Select(Function(xx) xx.value).ToArray)
            Authors = dict.TryGetValue("%A").JoinBy("; ")
            Title = dict.TryGetValue("%T", [default]:=New String() {}).FirstOrDefault
            DOI = dict.TryGetValue("%R", [default]:=New String() {}).FirstOrDefault
            Year = CInt(Val(dict.TryGetValue("%D", [default]:=New String() {}).FirstOrDefault))
            Pages = dict.TryGetValue("%P", [default]:=New String() {}).FirstOrDefault
            Journal = dict.TryGetValue("%J", [default]:=New String() {}).FirstOrDefault
            Volume = CInt(Val(dict.TryGetValue("%V", [default]:=New String() {}).FirstOrDefault))
            URL = dict.TryGetValue("%U", [default]:=New String() {}).FirstOrDefault
            Abstract = dict.TryGetValue("%X", [default]:=New String() {}).FirstOrDefault
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Authors}, {Title}, {Journal}. {Volume} ({Issue}) ({Year}) {Pages}."
        End Function

        Public Shared Function GetCiteList(Of T)() As Cite()
#If NET_40 = 0 Then
            Dim typeDef As Type = GetType(T)
            Dim attrs = typeDef.GetCustomAttributes(Of Cite)(inherit:=True)
            Return attrs
#Else
            Throw New NotSupportedException
#End If
        End Function

        Public Shared Function GetCiteList(typeDef As Type) As Cite()
#If NET_40 = 0 Then
            Dim attrs As Cite() = typeDef.GetCustomAttributes(Of Cite)(inherit:=True)
            Return attrs
#Else
            Throw New NotSupportedException
#End If
        End Function

        Public Function HTML(absLen As Integer) As String
            Dim htmlBuilder As StringBuilder = New StringBuilder(1024)

            Call htmlBuilder.Append($"<i>{Authors}</i>.({Year}). ""<strong>{Title}</strong>"", {Journal}. <strong>{Volume}</strong>({Issue}) {Pages}.")
            Call htmlBuilder.AppendLine($"<p><pre>{Me.__formatAbstractPreview(absLen)}</pre></p>")
            Call htmlBuilder.AppendLine("<font size=""2"">")
            Call htmlBuilder.AppendLine($"<p><font size=""2""><strong>Keywords: </strong> <i>{Keywords}</i><br /></font></p>")
            Call htmlBuilder.AppendLine($"<p><font size=""2""><strong>Author Contacts: </strong>  {AuthorAddress}</font></p>")
            Call htmlBuilder.AppendLine("<i>")
            If Not String.IsNullOrEmpty(DOI) Then
                Call htmlBuilder.AppendLine($"<li>DOI: <a href=""http://doi.org/{DOI}"">{DOI}</a></li><br />")
            End If
            If Not PubMed = 0 Then
                Call htmlBuilder.AppendLine($"<li>PMC FullText: <a href=""http://www.ncbi.nlm.nih.gov/pubmed/{PubMed}"">{PubMed}</a></li>")
            End If
            Call htmlBuilder.AppendLine("
</i>
</font>")

            Return htmlBuilder.ToString
        End Function

        ''' <summary>
        ''' 摘要可能会有空值的
        ''' </summary>
        ''' <param name="absLen"></param>
        ''' <returns></returns>
        Private Function __formatAbstractPreview(absLen As String) As String
            If String.IsNullOrEmpty(Abstract) Then
                Return ""
            End If

            Dim Tokens As String() = Regex.Split(Abstract.Replace(vbCr, "").Replace(vbLf, " ").Replace("  ", " "), "<p>", RegexOptions.IgnoreCase Or RegexOptions.Singleline)
            Tokens = (From s As String In Tokens Select Regex.Split(s, "<br/>", RegexOptions.Singleline Or RegexOptions.IgnoreCase)).ToArray.ToVector
            Dim sbr As StringBuilder = New StringBuilder(1024)

            For Each s As String In Tokens
                Call StringHelpers.Parts(s, absLen, sbr)
                Call sbr.AppendLine(" ")
            Next

            Return sbr.ToString
        End Function
    End Class
End Namespace
