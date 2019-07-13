#Region "Microsoft.VisualBasic::db50ed336482acfe493e2f2a552d39d6, Data_science\Mathematica\Math\Math\test\Program.vb"

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

    ' Module Program
    ' 
    '     Function: Hash
    ' 
    '     Sub: Main, RankingTest, Test, uncheckedTest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.HashMaps
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.SecurityString
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub RankingTest()
        Call {1, 2, 2, 3}.StandardCompetitionRanking.GetJson.__DEBUG_ECHO
        Call {1, 2, 2, 3}.ModifiedCompetitionRanking.GetJson.__DEBUG_ECHO
        Call {1, 2, 2, 3}.OrdinalRanking.GetJson.__DEBUG_ECHO
        Call {1, 2, 2, 3}.DenseRanking.GetJson.__DEBUG_ECHO
        Call {1.0, 1.0, 2.0, 3.0, 3.0, 4.0, 5.0, 5.0, 5.0}.FractionalRanking.GetJson.__DEBUG_ECHO
    End Sub

    Public Function Hash(key$) As Long
        Return key.MD5.ToLong
    End Function

    Sub uncheckedTest()
        Const ConstantMax% = Integer.MaxValue

        Dim int1 As New UncheckedInteger(2147483647)
        Dim int2% = 10

        Dim i% = int1 + int2

        Console.WriteLine(i)

        Pause()
    End Sub

    Sub Main()

        Call uncheckedTest()

        Call RankingTest()

        Pause()

        Dim l As New List(Of String)
        Dim uid As New Uid(False)

        For i As Integer = 0 To 5000
            Dim KEGG = "C" & i.FormatZero("00000")
            l.Add(KEGG.MD5)
        Next



        '  Pause()

        Dim blizzard As New HashMaps.HashBlizzard
        Dim l1 As New List(Of ULong)
        Dim l2 As New List(Of ULong)
        Dim l3 As New List(Of ULong)
        ' Dim uid As New Uid


        Call blizzard.HashBlizzard("XC_1183").ToString.__INFO_ECHO
        Call blizzard.HashBlizzard("XC_1184").ToString.__INFO_ECHO
        Call blizzard.HashBlizzard("XC_2252").ToString.__INFO_ECHO

        Pause()


        For i As Integer = 0 To 200000
            Dim ID = "C" & i.FormatZero("00000")
            l1.Add(blizzard.HashBlizzard(ID, HashBlizzard.dwHashTypes.Position))
            l2.Add(blizzard.HashBlizzard(ID, HashBlizzard.dwHashTypes.Validate1))
            l3.Add(blizzard.HashBlizzard(ID, HashBlizzard.dwHashTypes.Validate2))
        Next


        Dim g1 = l1.GroupBy(Function(u) u).Where(Function(gg) gg.Count > 1).ToArray
        Dim g2 = l2.GroupBy(Function(u) u).Where(Function(gg) gg.Count > 1).ToArray
        Dim g3 = l3.GroupBy(Function(u) u).Where(Function(gg) gg.Count > 1).ToArray

        Pause()

        Dim hash = blizzard.HashBlizzard("unitneutralacritter.grp")

        Call unchecked(&HA26067F3).uncheckedULong.ToString.__INFO_ECHO
        Call hash.ToString.__INFO_ECHO

        Pause()

        '  Dim g1 = l.GroupBy(Function(u) u).Where(Function(gg) gg.Count > 1).Select(Function(x) (x.Key, x.ToArray)).ToArray


        Pause()

        Pause()

        Dim aaa = 23
        Dim bbb = 4.5

        Dim func As Expression(Of Func(Of Double)) = Function() 2 + 3 * aaa / bbb


        Pause()

        ' Call (0#, 100000.0#).DoubleRange.rand(2000).Summary.EchoLine
        ' Call {0#, 569.0#, 63.0#, 59, 345.0#, 456, 423}.Summary.EchoLine

        Pause()



        Call Test(1, 2, 3, z:="a+b")

        Pause()

        Call "x <- 123+3^3!".Evaluate
        ' Call "log((x+699)*9/3!)".Evaluate
        Call "x <- log((x+699)*9/3!)+sin(99)".Evaluate
        Call "x!".Evaluate
        Call "(1+2)!/5".Evaluate

        Pause()
    End Sub

    Sub Test(a!, b&, c#, Optional x$ = "(A + b^2)! * 100", Optional y$ = "(cos(x/33)+1)^2 -3", Optional z$ = "log(-Y) + 9")
        'Dim params As Dictionary(Of String, Double) = ParameterExpression.Evaluate(Function() {a, b, x, y, z})
        'Dim json$ = params.GetJson(True)

        Dim before = {a, b, c, x, y, z}

        '    Call ParameterExpression.Apply(Function() {a, b, x, y, z})

        Dim after = {a, b, c, x, y, z}

        'Call json.__DEBUG_ECHO
    End Sub
End Module
