#Region "Microsoft.VisualBasic::a212d2ae70ae9cbe31bb6918a074df57, Data\Trinity\StopWords.vb"

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

    '   Total Lines: 411
    '    Code Lines: 390
    ' Comment Lines: 12
    '   Blank Lines: 9
    '     File Size: 11.24 KB


    ' Class StopWords
    ' 
    '     Properties: Count, DefaultStopWords
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetEnumerator, IEnumerable_GetEnumerator, Removes
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq

Public Class StopWords : Implements IEnumerable(Of String)
    Implements IReadOnlyCollection(Of String)
    Implements IReadOnlyList(Of String)

    ''' <summary>
    ''' https://www.ranks.nl/stopwords/
    ''' </summary>
    ReadOnly stopwords As Index(Of String) = {
 _
        "a", "able", "about", "above", "abst", "accordance", "according", "accordingly", "across", "act", "actually", "added", "adj",
        "affected", "affecting", "affects", "after", "afterwards", "again", "against", "ah", "all", "almost", "alone", "along", "already",
        "also", "although", "always", "am", "among", "amongst", "an", "and", "announce", "another", "any", "anybody", "anyhow", "anymore",
        "anyone", "anything", "anyway", "anyways", "anywhere", "apparently", "approximately", "are", "aren", "arent", "arise", "around",
        "as", "aside", "ask", "asking", "at", "auth", "available", "away", "awfully",
 _
        "b", "back", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "begin", "beginning",
        "beginnings", "begins", "behind", "being", "believe", "below", "beside", "besides", "between", "beyond", "biol", "both", "brief",
        "briefly", "but", "by",
 _
        "c", "ca", "came", "can", "cannot", "can't", "cause", "causes", "certain", "certainly", "co", "com", "come", "comes", "contain",
        "containing", "contains", "could", "couldnt",
 _
        "d", "date", "did", "didn't", "different", "do", "does", "doesn't", "doing", "done", "don't", "down", "downwards", "due", "during",
 _
        "e", "each", "ed", "edu", "effect", "eg", "eight", "eighty", "either", "else", "elsewhere", "end", "ending", "enough", "especially",
        "et", "et-al", "etc", "even", "ever", "every", "everybody", "everyone", "everything", "everywhere", "ex", "except",
 _
        "f", "far", "few", "ff", "fifth", "first", "five", "fix", "followed", "following", "follows", "for", "former", "formerly", "forth",
        "found", "four", "from", "further", "furthermore",
 _
        "g", "gave", "get", "gets", "getting", "give", "given", "gives", "giving", "go", "goes", "gone", "got", "gotten",
 _
        "h",
        "had",
        "happens",
        "hardly",
        "has",
        "hasn't",
        "have",
        "haven't",
        "having",
        "he",
        "hed",
        "hence",
        "her",
        "here",
        "hereafter",
        "hereby",
        "herein",
        "heres",
        "hereupon",
        "hers",
        "herself",
        "hes",
        "hi",
        "hid",
        "him",
        "himself",
        "his",
        "hither",
        "home",
        "how",
        "howbeit",
        "however",
        "hundred",
 _
        "i",
        "id",
        "ie",
        "if",
        "i'll",
        "im",
        "immediate",
        "immediately",
        "importance",
        "important",
        "in",
        "inc",
        "indeed",
        "index",
        "information",
        "instead",
        "into",
        "invention",
        "inward",
        "is",
        "isn't",
        "it",
        "itd",
        "it'll",
        "its",
        "itself",
        "i've",
 _
        "j", "just",
 _
        "k",
        "keep	keeps",
        "kept",
        "kg",
        "km",
        "know",
        "known",
        "knows",
 _
        "l",
        "largely",
        "last",
        "lately",
        "later",
        "latter",
        "latterly",
        "least",
        "less",
        "lest",
        "let",
        "lets",
        "like",
        "liked",
        "likely",
        "line",
        "little",
        "'ll",
        "look",
        "looking",
        "looks",
        "ltd",
 _
        "m",
        "made",
        "mainly",
        "make",
        "makes",
        "many",
        "may",
        "maybe",
        "me",
        "mean",
        "means",
        "meantime",
        "meanwhile",
        "merely",
        "mg",
        "might",
        "million",
        "miss",
        "ml",
        "more",
        "moreover",
        "most",
        "mostly",
        "mr",
        "mrs",
        "much",
        "mug",
        "must",
        "my",
        "myself",
 _
        "n",
        "na",
        "name",
        "namely",
        "nay",
        "nd",
        "near",
        "nearly",
        "necessarily",
        "necessary",
        "need",
        "needs",
        "neither",
        "never",
        "nevertheless",
        "new",
        "next",
        "nine",
        "ninety",
        "no",
        "nobody",
        "non",
        "none",
        "nonetheless",
        "noone",
        "nor",
        "normally",
        "nos",
        "not",
        "noted",
        "nothing",
        "now",
        "nowhere",
 _
        "o", "obtain", "obtained", "obviously", "of", "off", "often", "oh", "ok", "okay", "old", "omitted", "on",
        "once", "one", "ones", "only", "onto", "or", "ord", "other", "others", "otherwise", "ought", "our", "ours",
        "ourselves", "out", "outside", "over", "overall", "owing", "own",
 _
        "p", "page", "pages", "part", "particular", "particularly", "past", "per", "perhaps", "placed", "please",
        "plus", "poorly", "possible", "possibly", "potentially", "pp", "predominantly", "present", "previously",
        "primarily", "probably", "promptly", "proud", "provides", "put",
 _
        "q", "que", "quickly", "quite", "qv",
 _
        "r", "ran", "rather", "rd", "re", "readily", "really", "recent", "recently", "ref", "refs", "regarding",
        "regardless", "regards", "related", "relatively", "research", "respectively", "resulted", "resulting",
        "results", "right", "run",
 _
        "s",
        "said",
        "same",
        "saw",
        "say",
        "saying",
        "says",
        "sec",
        "section",
        "see",
        "seeing",
        "seem",
        "seemed",
        "seeming",
        "seems",
        "seen",
        "self",
        "selves",
        "sent",
        "seven",
        "several",
        "shall",
        "she",
        "shed",
        "she'll",
        "shes",
        "should",
        "shouldn't",
        "show",
        "showed",
        "shown",
        "showns",
        "shows",
        "significant",
        "significantly",
        "similar",
        "similarly",
        "since",
        "six",
        "slightly",
        "so",
        "some",
        "somebody",
        "somehow",
        "someone",
        "somethan",
        "something",
        "sometime",
        "sometimes",
        "somewhat",
        "somewhere",
        "soon",
        "sorry",
        "specifically",
        "specified",
        "specify",
        "specifying",
        "still",
        "stop",
        "strongly",
        "sub",
        "substantially",
        "successfully",
        "such",
        "sufficiently",
        "suggest",
        "sup",
        "sure",
 _
        "t",
        "take",
        "taken",
        "taking",
        "tell",
        "tends",
        "th",
        "than",
        "thank",
        "thanks",
        "thanx",
        "that",
        "that'll",
        "thats",
        "that've",
        "the",
        "their",
        "theirs",
        "them",
        "themselves",
        "then",
        "thence",
        "there",
        "thereafter",
        "thereby",
        "thered",
        "therefore",
        "therein",
        "there'll",
        "thereof",
        "therere",
        "theres",
        "thereto",
        "thereupon",
        "there've",
        "these",
        "they",
        "theyd",
        "they'll",
        "theyre",
        "they've",
        "think",
        "this",
        "those",
        "thou",
        "though",
        "thoughh",
        "thousand",
        "throug",
        "through",
        "throughout",
        "thru",
        "thus",
        "til",
        "tip",
        "to",
        "together",
        "too",
        "took",
        "toward",
        "towards",
        "tried",
        "tries",
        "truly",
        "try",
        "trying",
        "ts",
        "twice",
        "two",
 _
        "u", "un", "under", "unfortunately", "unless", "unlike", "unlikely", "until", "unto", "up", "upon", "ups", "us",
        "use", "used", "useful", "usefully", "usefulness", "uses", "using", "usually",
 _
        "v", "value", "various", "'ve", "very", "via", "viz", "vol", "vols", "vs",
 _
        "w", "want", "wants", "was", "wasnt", "way", "we", "wed", "welcome", "we'll", "went", "were", "werent", "we've",
        "what", "whatever", "what'll", "whats", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby",
        "wherein", "wheres", "whereupon", "wherever", "whether", "which", "while", "whim", "whither", "who", "whod",
        "whoever", "whole", "who'll", "whom", "whomever", "whos", "whose", "why", "widely", "willing", "wish", "with",
        "within", "without", "wont", "words", "world", "would", "wouldnt", "www",
 _
        "x",
 _
        "y", "yes", "yet", "you", "youd", "you'll", "your", "youre", "yours", "yourself", "yourselves", "you've",
 _
        "z", "zero"
    }.Indexing

    Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of String).Count

    ''' <summary>
    ''' Using list of stop words from https://www.ranks.nl/stopwords/ as default.
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property DefaultStopWords As New [Default](Of  StopWords)(New StopWords)

    Default Public ReadOnly Property Item(index As Integer) As String Implements IReadOnlyList(Of String).Item
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return stopwords(index)
        End Get
    End Property

    Sub New()
        Count = stopwords.Count
    End Sub

    ''' <summary>
    ''' Removes all of the stop words from the <paramref name="tokens"/> list.
    ''' </summary>
    ''' <param name="tokens"></param>
    ''' <returns></returns>
    Public Iterator Function Removes(tokens As IEnumerable(Of String)) As IEnumerable(Of String)
        For Each word As String In tokens.SafeQuery
            If Not word.ToLower Like stopwords Then
                Yield word
            End If
        Next
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
        For Each word$ In stopwords.Objects
            Yield word
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
