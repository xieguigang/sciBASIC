#Region "Microsoft.VisualBasic::8c62335a7d04996c211d9e1da172eb29, sciBASIC#\Data_science\Mathematica\data\Student's T-test\t.test\Program.vb"

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

    '   Total Lines: 213
    '    Code Lines: 48
    ' Comment Lines: 111
    '   Blank Lines: 54
    '     File Size: 6.80 KB


    ' Module Program
    ' 
    '     Sub: Main, oneSample, twoSample
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main()
        Call oneSample()
        Call twoSample()


        Dim a#() = {115, 108, 108, 119, 105, 101, 120, 115, 104, 100.9}
        Dim b#() = {185, 169, 173, 173, 188, 186, 175, 174, 179, 180}

        With t.Test(a, b)
            Call $"alternative hypothesis: { .Valid}".__DEBUG_ECHO
            Call .GetJson(True).__DEBUG_ECHO
        End With

        Dim x#() = {0, 1, 1, 1}

        ' ttest([0,1,1,1], {mu: 1}).valid() // true
        With t.Test(x, mu:=1)
            Call $"alternative hypothesis: { .Valid}".__DEBUG_ECHO
            Call .GetJson(True).__DEBUG_ECHO
        End With

        ' ttest([0,1,1,1], [1,2,2,2], {mu: -1}).valid() // true
        Call Console.Write(t.Test({0, 1, 1, 1}, {1, 2, 2, 2}, mu:=-1).ToString)

        a = {6846523.253, 6840877.665, 5806323.704}
        b = {3056565.388, 1831431.105, 2933659.497}

        Call t.Test(a, b).GetJson(indent:=True).__DEBUG_ECHO
        Call t.Test(a, b, varEqual:=False).GetJson(indent:=True).__DEBUG_ECHO

        Pause()
    End Sub

    Sub twoSample()
        With t.Test({1, 2, 2, 2, 4}, {0, 3, 3, 3, 2}, mu:=1, varEqual:=True, alpha:=0.05, alternative:=Hypothesis.TwoSided)
            'valid: true,
            '    freedom: 8,

            '    pValue: 0.225571973816597132200811870462,
            '    testValue: -1.313064328597225660644198796945,

            '    confidence: [
            '      -1.756200427489884585696700014523,
            '      1.756200427489884585696700014523
            '    ]

            Call Console.WriteLine(.ToString)
        End With

        With t.Test({1, 2, 2, 2, 4}, {0, 3, 3, 3, 2}, mu:=1, varEqual:=True, alpha:=0.05, alternative:=Hypothesis.Less)
            'valid: true,
            '    freedom: 8,

            '    pValue: 0.112785986908298566100405935231,
            '    testValue: -1.313064328597225660644198796945,

            '    confidence: [
            '      -Infinity,
            '      1.416189593328981199960026060580
            '    ]

            Call Console.WriteLine(.ToString)
        End With

        With t.Test({1, 2, 2, 2, 4}, {0, 3, 3, 3, 2}, mu:=1, varEqual:=True, alpha:=0.05, alternative:=Hypothesis.Greater)
            'valid: true,
            '    freedom: 8,

            '    pValue: 0.887214013091701447777381872584,
            '    testValue: -1.313064328597225660644198796945,

            '    confidence: [
            '      -1.416189593328981199960026060580,
            '      Infinity
            '    ]

            Call Console.WriteLine(.ToString)
        End With

        Pause()
    End Sub

    '    > t.test(c(1,2,2,2,4), mu=2, alpha = 0.05, alternative = "two.sided")

    '	One Sample t-test

    'data:  c(1, 2, 2, 2, 4)
    't = 0.40825, df = 4, p-value = 0.704
    'alternative hypothesis :  true mean Is Not equal to 2
    '95 percent confidence interval:
    ' 0.8398252 3.5601748
    'sample estimates : 
    'mean of x 
    '      2.2 

    '> t.test(c(1,2,2,2,4), mu=2, alpha = 0.05, alternative = "less")

    '	One Sample t-test

    'data:  c(1, 2, 2, 2, 4)
    't = 0.40825, df = 4, p-value = 0.648
    'alternative hypothesis :  true mean Is less than 2
    '95 percent confidence interval:
    '     -Inf 3.244387
    'sample estimates : 
    'mean of x 
    '      2.2 

    '> t.test(c(1,2,2,2,4), mu=2, alpha = 0.05, alternative = "greater")

    '	One Sample t-test

    'data:  c(1, 2, 2, 2, 4)
    't = 0.40825, df = 4, p-value = 0.352
    'alternative hypothesis :  true mean Is greater than 2
    '95 percent confidence interval:
    ' 1.155613      Inf
    'sample estimates : 
    'mean of x 
    '      2.2 

    '> t.test(c(1, 2, 2, 2, 4),c(0, 3, 3, 3, 2), mu = 1, var.equal = TRUE, alpha = 0.05, alternative = "two.sided")

    '	Two Sample t-test

    'data:  c(1, 2, 2, 2, 4) And c(0, 3, 3, 3, 2)
    't = -1.3131, df = 8, p-value = 0.2256
    'alternative hypothesis :  true difference in means Is Not equal to 1
    '95 percent confidence interval:
    ' -1.7562  1.7562
    'sample estimates : 
    'mean of x mean of y 
    '      2.2       2.2 

    '> t.test(c(1, 2, 2, 2, 4),c(0, 3, 3, 3, 2), mu = 1, var.equal = TRUE, alpha = 0.05, alternative = "less")

    '	Two Sample t-test

    'data:  c(1, 2, 2, 2, 4) And c(0, 3, 3, 3, 2)
    't = -1.3131, df = 8, p-value = 0.1128
    'alternative hypothesis :  true difference in means Is less than 1
    '95 percent confidence interval:
    '    -Inf 1.41619
    'sample estimates : 
    'mean of x mean of y 
    '      2.2       2.2 

    '> t.test(c(1, 2, 2, 2, 4),c(0, 3, 3, 3, 2), mu = 1, var.equal = TRUE, alpha = 0.05, alternative = "greater")

    '	Two Sample t-test

    'data:  c(1, 2, 2, 2, 4) And c(0, 3, 3, 3, 2)
    't = -1.3131, df = 8, p-value = 0.8872
    'alternative hypothesis :  true difference in means Is greater than 1
    '95 percent confidence interval:
    ' -1.41619      Inf
    'sample estimates : 
    'mean of x mean of y 
    '      2.2       2.2 

    Sub oneSample()
        With t.Test({1, 2, 2, 2, 4}, mu:=2, alpha:=0.05, alternative:=Hypothesis.TwoSided)
            '    valid: true,
            '    freedom: 4,

            '    pValue: 0.703999999999999737099187768763,
            '    testValue: 0.408248290463863405808098150374,

            '    confidence: [
            '      0.839825238683489017077477001294,
            '      3.560174761316511560238495803787
            '    ]
            Call Console.WriteLine(.ToString)
        End With

        With t.Test({1, 2, 2, 2, 4}, mu:=2, alpha:=0.05, alternative:=Hypothesis.Less)
            '    valid: true,
            '    freedom: 4,

            '    pValue: 0.648000000000000131450406115619,
            '    testValue: 0.408248290463863405808098150374,

            '    confidence: [
            '      -Infinity,
            '      3.244387367258481980059059424093
            '    ]

            Call Console.WriteLine(.ToString)
        End With

        With t.Test({1, 2, 2, 2, 4}, mu:=2, alpha:=0.05, alternative:=Hypothesis.Greater)
            '    valid: true,
            '    freedom: 4,

            '    pValue: 0.351999999999999868549593884381,
            '    testValue: 0.408248290463863405808098150374,

            '    confidence: [
            '      1.155612632741518375212308455957,
            '      Infinity
            '    ]
            Call Console.WriteLine(.ToString)
        End With

        ' Pause()
    End Sub

End Module
