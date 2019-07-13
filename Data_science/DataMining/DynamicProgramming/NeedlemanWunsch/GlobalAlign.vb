#Region "Microsoft.VisualBasic::98e14fe7dba60a0bb5d627eda38da328, Data_science\DataMining\DynamicProgramming\NeedlemanWunsch\GlobalAlign.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Structure GlobalAlign
    ' 
    '         Properties: Length, PossibleSimilarity
    ' 
    '         Function: (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Namespace NeedlemanWunsch

    Public Structure GlobalAlign(Of T)

        Dim Score#
        Dim query As T()
        Dim subject As T()

        Public ReadOnly Property Length As Integer
            Get
                If query.Length <> subject.Length Then
                    Throw New InvalidExpressionException("")
                Else
                    Return query.Length
                End If
            End Get
        End Property

        Public ReadOnly Property PossibleSimilarity As Double
            Get
                Return Score / Length
            End Get
        End Property

        Public Overloads Function ToString(toChar As Func(Of T, Char)) As String
            Dim q As New List(Of Char)
            Dim c As New List(Of Char)
            Dim s As New List(Of Char)

            For i As Integer = 0 To query.Length - 1
                q.Add(toChar(query(i)))
                s.Add(toChar(subject(i)))

                If q.Last = s.Last Then
                    c.Add("*"c)
                Else
                    c.Add(" "c)
                End If
            Next

            Return {
                q.CharString,
                c.CharString,
                s.CharString
            }.JoinBy(ASCII.LF)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToString(Function(x) x.ToString.First)
        End Function
    End Structure
End Namespace
