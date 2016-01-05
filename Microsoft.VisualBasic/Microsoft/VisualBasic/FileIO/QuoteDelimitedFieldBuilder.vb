Imports System
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Microsoft.VisualBasic.FileIO
    Friend Class QuoteDelimitedFieldBuilder
        ' Methods
        Public Sub New(DelimiterRegex As Regex, SpaceChars As String)
            Me.m_DelimiterRegex = DelimiterRegex
            Me.m_SpaceChars = SpaceChars
        End Sub

        Public Sub BuildField(Line As String, StartAt As Integer)
            Me.m_Index = StartAt
            Dim length As Integer = Line.Length
            Do While (Me.m_Index < length)
                Dim numRef As Integer
                If (Line.Chars(Me.m_Index) = """"c) Then
                    If ((Me.m_Index + 1) = length) Then
                        Me.m_FieldFinished = True
                        Me.m_DelimiterLength = 1
                        numRef = CInt(AddressOf Me.m_Index) = (numRef + 1)
                        Return
                    End If
                    If Not (((Me.m_Index + 1) < Line.Length) And (Line.Chars((Me.m_Index + 1)) = """"c)) Then
                        Dim num2 As Integer
                        Dim match As Match = Me.m_DelimiterRegex.Match(Line, CInt((Me.m_Index + 1)))
                        If Not match.Success Then
                            num2 = (length - 1)
                        Else
                            num2 = (match.Index - 1)
                        End If
                        Dim num3 As Integer = num2
                        Dim i As Integer = (Me.m_Index + 1)
                        Do While (i <= num3)
                            If (Me.m_SpaceChars.IndexOf(Line.Chars(i)) < 0) Then
                                Me.m_MalformedLine = True
                                Return
                            End If
                            i += 1
                        Loop
                        Me.m_DelimiterLength = ((1 + num2) - Me.m_Index)
                        If match.Success Then
                            numRef = CInt(AddressOf Me.m_DelimiterLength) = (numRef + match.Length)
                        End If
                        Me.m_FieldFinished = True
                        Return
                    End If
                    Me.m_Field.Append(""""c)
                    numRef = CInt(AddressOf Me.m_Index) = (numRef + 2)
                Else
                    Me.m_Field.Append(Line.Chars(Me.m_Index))
                    numRef = CInt(AddressOf Me.m_Index) = (numRef + 1)
                End If
            Loop
        End Sub


        ' Properties
        Public ReadOnly Property DelimiterLength As Integer
            Get
                Return Me.m_DelimiterLength
            End Get
        End Property

        Public ReadOnly Property Field As String
            Get
                Return Me.m_Field.ToString
            End Get
        End Property

        Public ReadOnly Property FieldFinished As Boolean
            Get
                Return Me.m_FieldFinished
            End Get
        End Property

        Public ReadOnly Property Index As Integer
            Get
                Return Me.m_Index
            End Get
        End Property

        Public ReadOnly Property MalformedLine As Boolean
            Get
                Return Me.m_MalformedLine
            End Get
        End Property


        ' Fields
        Private m_DelimiterLength As Integer
        Private m_DelimiterRegex As Regex
        Private m_Field As StringBuilder = New StringBuilder
        Private m_FieldFinished As Boolean
        Private m_Index As Integer
        Private m_MalformedLine As Boolean
        Private m_SpaceChars As String
    End Class
End Namespace

