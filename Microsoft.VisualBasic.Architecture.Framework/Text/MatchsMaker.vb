'
'Matching two strings
'Author: Thanh Ngoc Dao - Thanh.dao@gmx.net
'Copyright (c) 2005 by Thanh Ngoc Dao, All rights reserved.
'---------
'Thanks to Malcolm Crowe,Troy Simpson, Jeff Martin for the .NET WordNet port
'Thanks to Carl Mercier for his french characters conversion function.
'
'Please test carefully before using.
'

Imports System.Collections
Imports System.Text.RegularExpressions
Imports System.Diagnostics

Namespace Text

    ''' <summary>
    ''' Summary description for StringMatcher.
    ''' </summary>
    ''' 
    Public Delegate Function Similarity(s1 As String, s2 As String) As Single

    Public Class StringSimilarityMatchs
        Private _lString As String, _rString As String
        Private _leftTokens As String(), _rightTokens As String()
        Private leftLen As Integer, rightLen As Integer
        Private cost As Single(,)
        Private getSimilarity As Similarity

        Private _accentInsensitive As Boolean

        Public Sub New(left As String, right As String)
            Me.New(left, right, False)
        End Sub
        Public Sub New(left As String, right As String, accentInsensitive As Boolean)
            _accentInsensitive = accentInsensitive

            _lString = left
            _rString = right

            If _accentInsensitive Then
                _lString = StripAccents(_lString)
                _rString = StripAccents(_rString)
            End If

            MyInit()
        End Sub


        Private Function StripAccents(input As String) As String
            Dim beforeConversion As String = "嗬饴淠崃樯枞晔胨焯钗锵蛞粼鲋缜採"
            Dim afterConversion As String = "aAaAaAaAeEeEeEeEiIiIiIoOoOoOuUuUuUcC'n"

            Dim sb As New System.Text.StringBuilder(input)

            For i As Integer = 0 To beforeConversion.Length - 1
                Dim beforeChar As Char = beforeConversion(i)
                Dim afterChar As Char = afterConversion(i)

                sb.Replace(beforeChar, afterChar)
            Next

            sb.Replace("?", "oe")
            sb.Replace("?", "ae")

            Return sb.ToString()
        End Function

        Private Sub MyInit()
            Dim editdistance As ISimilarity = New Leven()
            getSimilarity = New Similarity(AddressOf editdistance.GetSimilarity)

            'ISimilarity lexical=new LexicalSimilarity() ;
            'getSimilarity=new Similarity(lexical.GetSimilarity) ;

            _leftTokens = Tokeniser.Partition(_lString)
            _rightTokens = tokeniser.Partition(_rString)
            If _leftTokens.Length > _rightTokens.Length Then
                Dim tmp As String() = _leftTokens
                _leftTokens = _rightTokens
                _rightTokens = tmp
                Dim s As String = _lString
                _lString = _rString
                _rString = s
            End If

            leftLen = _leftTokens.Length - 1
            rightLen = _rightTokens.Length - 1
            Initialize()

        End Sub

        Private Sub Initialize()
            cost = New Single(leftLen, rightLen) {}
            For i As Integer = 0 To leftLen
                For j As Integer = 0 To rightLen
                    cost(i, j) = getSimilarity(_leftTokens(i), _rightTokens(j))
                Next
            Next
        End Sub


        Public Function GetScore() As Single
            Dim match As New BipartiteMatcher(_leftTokens, _rightTokens, cost)
            Return match.Score
        End Function


        Public ReadOnly Property Score() As Single
            Get
                Return GetScore()
            End Get
        End Property

    End Class

    '
    'Tokenization string 
    'Author: Thanh Ngoc Dao - Thanh.dao@gmx.net
    'Copyright (c) 2005 by Thanh Ngoc Dao.
    '

    ''' <summary>
    ''' Summary description for Tokeniser.
    ''' Partition string off into subwords
    ''' </summary>
    Module Tokeniser

        Const BlanksControl As String = vbCr & vbLf & vbTab & "  "

        Private Function Tokenize(input As System.String) As ArrayList
            Dim returnVect As New ArrayList(10)
            Dim nextGapPos As Integer
            Dim curPos As Integer = 0
            While curPos < input.Length
                Dim ch As Char = input(curPos)
                If System.[Char].IsWhiteSpace(ch) Then
                    curPos += 1
                End If
                nextGapPos = input.Length
                For i As Integer = 0 To BlanksControl.Length - 1
                    Dim testPos As Integer = input.IndexOf(CType(BlanksControl(i), [Char]), curPos)
                    If testPos < nextGapPos AndAlso testPos <> -1 Then
                        nextGapPos = testPos
                    End If
                Next

                Dim term As System.String = input.Substring(curPos, (nextGapPos) - (curPos))
                'if (!stopWordHandler.isWord(term))
                returnVect.Add(term)
                curPos = nextGapPos
            End While

            Return returnVect
        End Function

        Private Sub InternalNormalizeCasing(ByRef input As String)
            'if it is formed by Pascal/Carmel casing
            For i As Integer = 0 To input.Length - 1
                If [Char].IsSeparator(input(i)) Then
                    input = input.Replace(input(i).ToString(), " ")
                End If
            Next
            Dim idx As Integer = 1
            While idx < input.Length - 2
                idx += 1
                If ([Char].IsUpper(input(idx)) AndAlso [Char].IsLower(input(idx + 1))) AndAlso (Not [Char].IsWhiteSpace(input(idx - 1)) AndAlso Not [Char].IsSeparator(input(idx - 1))) Then
                    input = input.Insert(idx, " ")
                    idx += 1
                End If
            End While
        End Sub

        Public Function Partition(input As String) As String()

            Call InternalNormalizeCasing(input)

            input = input.ToLower()

            Dim r As New Regex("([ \t{}():;])")
            Dim Tokens As [String]() = r.Split(input)
            Dim Filter As New List(Of String)

            For i As Integer = 0 To Tokens.Length - 1
                Dim mc As MatchCollection = r.Matches(Tokens(i))
                If mc.Count <= 0 AndAlso Tokens(i).Trim().Length > 0 Then
                    Filter.Add(Tokens(i))
                End If
            Next

            Tokens = Filter.ToArray

            Return Tokens
        End Function
    End Module

    '
    'Matching two strings
    'Author: Thanh Ngoc Dao - Thanh.dao@gmx.net
    'Copyright (c) 2005 by Thanh Ngoc Dao.
    '

    ''' <summary>
    ''' Summary description for Leven.
    ''' </summary>
    Friend Class Leven
        Implements ISimilarity
        Private Function Min3(a As Integer, b As Integer, c As Integer) As Integer
            Return System.Math.Min(System.Math.Min(a, b), c)
        End Function

        Private Function ComputeDistance(s As String, t As String) As Integer
            Dim n As Integer = s.Length
            Dim m As Integer = t.Length
            Dim distance As Integer(,) = New Integer(n, m) {}
            ' matrix
            Dim cost As Integer = 0

            If n = 0 Then
                Return m
            End If
            If m = 0 Then
                Return n
            End If
            'init1
            Dim i As Integer = 0
            While i <= n


                distance(i, 0) = System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
            End While
            Dim j As Integer = 0
            While j <= m


                distance(0, j) = System.Math.Max(System.Threading.Interlocked.Increment(j), j - 1)
            End While

            'find min distance
            For i = 1 To n
                For j = 1 To m
                    cost = (If(t.Substring(j - 1, 1) = s.Substring(i - 1, 1), 0, 1))
                    distance(i, j) = Min3(distance(i - 1, j) + 1, distance(i, j - 1) + 1, distance(i - 1, j - 1) + cost)
                Next
            Next

            Return distance(n, m)
        End Function

        Public Function GetSimilarity(string1 As System.String, string2 As System.String) As Single Implements ISimilarity.GetSimilarity

            Dim dis As Single = ComputeDistance(string1, string2)
            Dim maxLen As Single = string1.Length
            If maxLen < CSng(string2.Length) Then
                maxLen = string2.Length
            End If

            Dim minLen As Single = string1.Length
            If minLen > CSng(string2.Length) Then
                minLen = string2.Length
            End If


            If maxLen = 0.0F Then
                Return 1.0F
            Else
                'return 1.0F - dis/maxLen ;
                'return (float) Math.Round(1.0F - dis/maxLen, 1) * 10 ;
                Return maxLen - dis
            End If
        End Function

        '
        ' TODO: Add constructor logic here
        '
        Public Sub New()
        End Sub
    End Class

    ''' <summary>
    ''' Summary description for IEditDistance.
    ''' </summary>
    Interface ISimilarity
        Function GetSimilarity(string1 As String, string2 As String) As Single
    End Interface

    '
    'Maximize the total weight of bipartite grapth 
    'Author: Thanh Ngoc Dao - Thanh.dao@gmx.net
    'Copyright (c) 2005 by Thanh Ngoc Dao.
    '

    ''' <summary>
    ''' Summary description for StringMatcher.
    ''' </summary>
    ''' 

    Public Class BipartiteMatcher
        Private _leftTokens As String(), _rightTokens As String()
        Private _cost As Single(,)
        Private leftLabel As Single(), rightLabel As Single()
        Private _previous As Integer(), _incomming As Integer(), _outgoing As Integer()
        'connect with the left and right
        Private _leftVisited As Boolean(), _rightVisited As Boolean()
        Private leftLen As Integer, rightLen As Integer
        Private _errorOccured As Boolean = False

        Public Sub New(left As String(), right As String(), cost As Single(,))
            If left Is Nothing OrElse right Is Nothing OrElse cost Is Nothing Then
                _errorOccured = True
                Return
            End If

            _leftTokens = left
            _rightTokens = right
            'swap
            If _leftTokens.Length > _rightTokens.Length Then
                Dim tmpCost As Single(,) = New Single(_rightTokens.Length - 1, _leftTokens.Length - 1) {}
                For i As Integer = 0 To _rightTokens.Length - 1
                    For j As Integer = 0 To _leftTokens.Length - 1
                        tmpCost(i, j) = cost(j, i)
                    Next
                Next

                _cost = DirectCast(tmpCost.Clone(), Single(,))

                Dim tmp As String() = _leftTokens
                _leftTokens = _rightTokens
                _rightTokens = tmp
            Else
                _cost = DirectCast(cost.Clone(), Single(,))
            End If


            MyInit()

            Make_Matching()
        End Sub

        Private Sub MyInit()
            Initialize()

            _leftVisited = New Boolean(leftLen) {}
            _rightVisited = New Boolean(rightLen) {}
            _previous = New Integer((leftLen + rightLen) + 1) {}

        End Sub

        Private Sub Initialize()
            leftLen = _leftTokens.Length - 1
            rightLen = _rightTokens.Length - 1

            leftLabel = New Single(leftLen) {}
            rightLabel = New Single(rightLen) {}
            For i As Integer = 0 To leftLabel.Length - 1
                leftLabel(i) = 0
            Next
            For i As Integer = 0 To rightLabel.Length - 1
                rightLabel(i) = 0
            Next

            'init distance
            For i As Integer = 0 To leftLen
                Dim maxLeft As Single = Single.MinValue
                For j As Integer = 0 To rightLen
                    If _cost(i, j) > maxLeft Then
                        maxLeft = _cost(i, j)
                    End If
                Next

                leftLabel(i) = maxLeft
            Next

        End Sub

        Private Sub Flush()
            For i As Integer = 0 To _previous.Length - 1
                _previous(i) = -1
            Next
            For i As Integer = 0 To _leftVisited.Length - 1
                _leftVisited(i) = False
            Next
            For i As Integer = 0 To _rightVisited.Length - 1
                _rightVisited(i) = False
            Next
        End Sub

        Private [stop] As Boolean = False
        Private Function FindPath(source As Integer) As Boolean
            Flush()
            [stop] = False
            Walk(source)
            Return [stop]

        End Function

        Private Sub Increase_Matchs(li As Integer, lj As Integer)
            Dim tmpOut As Integer() = DirectCast(_outgoing.Clone(), Integer())
            Dim i As Integer, j As Integer, k As Integer
            i = li
            j = lj
            _outgoing(i) = j
            _incomming(j) = i
            If _previous(i) <> -1 Then
                Do
                    j = tmpOut(i)
                    k = _previous(i)
                    _outgoing(k) = j
                    _incomming(j) = k
                    i = k
                Loop While _previous(i) <> -1
            End If
        End Sub


        Private Sub Walk(i As Integer)
            _leftVisited(i) = True

            For j As Integer = 0 To rightLen
                If [stop] Then
                    Return
                ElseIf Not _rightVisited(j) AndAlso (leftLabel(i) + rightLabel(j) = _cost(i, j)) Then
                    If _incomming(j) = -1 Then
                        ' if found a path
                        [stop] = True
                        Increase_Matchs(i, j)
                        Return
                    Else
                        Dim k As Integer = _incomming(j)
                        _rightVisited(j) = True
                        _previous(k) = i
                        Walk(k)
                    End If
                End If
            Next
        End Sub

#Region "BreadFirst"
        '		int FindPath(int source)
        '		{
        '			int head, tail, idxHead=0;
        '			int[] visited=new int[(leftLen+rightLen)+2] , 
        '				q=new int[(leftLen+rightLen)+2] ;
        '			head=0;
        '			for (int i=0; i < visited.Length; i++) visited[i]=0;
        '			Flush ();
        '								
        '			head=-1;
        '			tail=0;
        '			q[tail]=source;
        '			visited[source]=1;
        '			leftVisited[source]=true;
        '			int nMerge=leftLen + rightLen + 1;
        '
        '			while (head <= tail)
        '			{
        '				++head;
        '				idxHead=q[head];
        '
        '
        '				for (int j=0; j <= (leftLen + rightLen + 1); j++)
        '					if(visited[j] == 0)
        '				{
        '					if (j > leftLen) //j is stay at the RightSide
        '					{
        '						int idxRight=j - (leftLen + 1);
        '						if (idxHead <= leftLen &&  (leftLabel[idxHead] + rightLabel[idxRight] == cost[idxHead, idxRight]))
        '						{
        '							++tail;
        '							q[tail]=j;
        '							visited[j]=1;
        '							previous[j]=idxHead;
        '							rightVisited[idxRight]=true;
        '							if (In[idxRight] == -1) // pretty good, found a path															
        '								return j;		
        '							
        '						}
        '					}
        '					else if ( j <= leftLen) // is stay at the left
        '					{
        '						if (idxHead > leftLen && In[idxHead - (leftLen + 1)] == j)
        '						{
        '							++tail;
        '							q[tail]=j;
        '							visited[j]=1;
        '							previous[j]=idxHead;
        '							leftVisited[j]=true;
        '						}
        '					}
        '				}
        '			}
        '
        '			return -1;//not found
        '		}
        '
        '		void Increase_Matchs(int j)
        '		{			
        '			if (previous [j] != -1)
        '				do
        '				{
        '					int i=previous[j];
        '					Out[i]=j-(leftLen + 1);
        '					In[j-(leftLen + 1)]=i;
        '					j=previous[i];
        '				} while ( j != -1);
        '		}
        '

#End Region

        Private Function GetMinDeviation() As Single
            Dim min As Single = Single.MaxValue

            For i As Integer = 0 To leftLen
                If _leftVisited(i) Then
                    For j As Integer = 0 To rightLen
                        If Not _rightVisited(j) Then
                            If leftLabel(i) + rightLabel(j) - _cost(i, j) < min Then
                                min = (leftLabel(i) + rightLabel(j)) - _cost(i, j)
                            End If
                        End If
                    Next
                End If
            Next

            Return min
        End Function

        Private Sub Relabels()
            Dim dev As Single = GetMinDeviation()

            For k As Integer = 0 To leftLen
                If _leftVisited(k) Then
                    leftLabel(k) -= dev
                End If
            Next

            For k As Integer = 0 To rightLen
                If _rightVisited(k) Then
                    rightLabel(k) += dev
                End If
            Next
        End Sub

        Private Sub Make_Matching()
            _outgoing = New Integer(leftLen) {}
            _incomming = New Integer(rightLen) {}
            For i As Integer = 0 To _outgoing.Length - 1
                _outgoing(i) = -1
            Next
            For i As Integer = 0 To _incomming.Length - 1
                _incomming(i) = -1
            Next

            For k As Integer = 0 To leftLen
                If _outgoing(k) = -1 Then
                    Dim found As Boolean = False
                    Do
                        found = FindPath(k)
                        If Not found Then
                            Relabels()

                        End If
                    Loop While Not found
                End If
            Next
        End Sub


        Private Function GetTotal() As Single
            Dim nTotal As Single = 0
            Dim nA As Single = 0
            Trace.Flush()
            For i As Integer = 0 To leftLen
                If _outgoing(i) <> -1 Then
                    nTotal += _cost(i, _outgoing(i))
                    Trace.WriteLine(_leftTokens(i) & " <-> " & _rightTokens(_outgoing(i)) & " : " & _cost(i, _outgoing(i)))
                    Dim a As Single = If(1.0F - System.Math.Max(_leftTokens(i).Length, _rightTokens(_outgoing(i)).Length) <> 0, _cost(i, _outgoing(i)) / System.Math.Max(_leftTokens(i).Length, _rightTokens(_outgoing(i)).Length), 1)
                    nA += a
                End If
            Next
            Return nTotal
        End Function

        Public Function GetScore() As Single
            Dim dis As Single = GetTotal()

            Dim maxLen As Single = rightLen + 1
            Dim l1 As Integer = 0
            Dim l2 As Integer = 0
            For Each s As String In _rightTokens
                l1 += s.Length
            Next
            For Each s As String In _leftTokens
                l2 += s.Length
            Next
            maxLen = Math.Max(l1, l2)

            If maxLen > 0 Then
                Return dis / maxLen
            Else
                Return 1.0F
            End If
        End Function


        Public ReadOnly Property Score() As Single
            Get
                If _errorOccured Then
                    Return 0
                Else
                    Return GetScore()
                End If
            End Get
        End Property

    End Class
End Namespace