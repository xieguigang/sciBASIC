Option Strict On
Option Explicit On

Imports System
Imports System.Globalization
Imports FL = Microsoft.VisualBasic.DataMining.FuzzyLogic.Logical.FuzzyLogic

''' <summary>
''' 基线快照工具：在改动 Mamdani 求值路径之前，把历史实现的行为录下来。
''' </summary>
Module Program

    Function Main(args As String()) As Integer
        Call PrintBaseline()
        Return 0
    End Function

    ''' <summary>README 里的 Water/Power 示例（含两个退化肩点）。</summary>
    Private Function CreateEngine() As FL.FuzzyEngine
        Dim water As New FL.LinguisticVariable("Water")
        Call water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Cold", 0, 0, 20, 40))
        Call water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Tepid", 30, 50, 50, 70))
        Call water.MembershipFunctionCollection.Add(New FL.MembershipFunction("Hot", 50, 80, 100, 100))

        Dim power As New FL.LinguisticVariable("Power")
        Call power.MembershipFunctionCollection.Add(New FL.MembershipFunction("Low", 0, 25, 25, 50))
        Call power.MembershipFunctionCollection.Add(New FL.MembershipFunction("High", 25, 50, 50, 75))

        Dim engine As New FL.FuzzyEngine()
        Call engine.LinguisticVariableCollection.Add(water)
        Call engine.LinguisticVariableCollection.Add(power)
        engine.Consequent = "Power"
        Call engine.FuzzyRuleCollection.Add(New FL.FuzzyRule("IF (Water IS Cold) OR (Water IS Tepid) THEN Power IS High"))
        Call engine.FuzzyRuleCollection.Add(New FL.FuzzyRule("IF (Water IS Hot) THEN Power IS Low"))

        Return engine
    End Function

    Private Sub PrintBaseline()
        Dim engine As FL.FuzzyEngine = CreateEngine()
        Dim water As FL.LinguisticVariable = engine.LinguisticVariableCollection.Find("Water")

        Dim points As Double() = {
            -10.0, -1.0, 0.0, 1.0, 5.0, 10.0, 19.9999, 20.0, 25.0, 30.0,
            39.9999, 40.0, 49.0, 50.0, 55.0, 60.0, 70.0, 79.0, 80.0, 90.0,
            99.0, 99.9999, 100.0, 101.0, 110.0
        }

        For Each x As Double In points
            water.InputValue = x

            Dim cold As Double = water.Fuzzify("Cold")
            Dim tepid As Double = water.Fuzzify("Tepid")
            Dim hot As Double = water.Fuzzify("Hot")
            Dim y As String

            Try
                y = F(engine.Defuzzify())
            Catch ex As Exception
                y = "ERR:" & ex.GetType().Name
            End Try

            Call Console.WriteLine($"{F(x)}|{F(cold)}|{F(tepid)}|{F(hot)}|{y}")
        Next
    End Sub

    Private Function F(value As Double) As String
        Return value.ToString("R", CultureInfo.InvariantCulture)
    End Function

End Module
