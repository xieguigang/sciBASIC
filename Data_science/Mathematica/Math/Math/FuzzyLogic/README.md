https://github.com/MicheleBertoli/DotFuzzy

DotFuzzy
========

Disclaimer
----------
This is the project I did for my bachelor thesis in 2008 and I never updated nor mantained it.
I published the source because some developers are using it and I hope they can improve the library.

What is DotFuzzy?
-----------------
DotFuzzy is an open source stand-alone class library for fuzzy logic. The library is built in C# and can therefore be used by all languages the .NET environment supports. Because of a clean natural Object Oriented approach the library is easy to use and implement. DotFuzzy is designed to be a flexible, robust and scalable.
DotFuzzy implements fuzzification, rules validation/evaluation and defuzzification with the centroid method. It is also possible to save and load projects in XML, whose tags are similar to Fuzzy Control Language.

Usage
-----
        Dim water As New LinguisticVariable("Water")
        water.MembershipFunctionCollection.Add(New MembershipFunction("Cold", 0, 0, 20, 40))
        water.MembershipFunctionCollection.Add(New MembershipFunction("Tepid", 30, 50, 50, 70))
        water.MembershipFunctionCollection.Add(New MembershipFunction("Hot", 50, 80, 100, 100))

        Dim power As LinguisticVariable = New LinguisticVariable("Power")
        power.MembershipFunctionCollection.Add(New MembershipFunction("Low", 0, 25, 25, 50))
        power.MembershipFunctionCollection.Add(New MembershipFunction("High", 25, 50, 50, 75))

        Dim FuzzyEngine As New FuzzyEngine()
        FuzzyEngine.LinguisticVariableCollection.Add(water)
        FuzzyEngine.LinguisticVariableCollection.Add(power)
        FuzzyEngine.Consequent = "Power"
        FuzzyEngine.FuzzyRuleCollection.Add(New FuzzyRule("IF (Water IS Cold) OR (Water IS Tepid) THEN Power IS High"))
        FuzzyEngine.FuzzyRuleCollection.Add(New FuzzyRule("IF (Water IS Hot) THEN Power IS Low"))

        water.InputValue = 60

        Dim xml As String = "fuzzyModel.xml"

        Call FuzzyEngine.Save(xml, Encodings.UTF8)

        FuzzyEngine = Nothing
        FuzzyEngine = Models.FuzzyModel.FromXml(xml)

        Try
            MsgBox(FuzzyEngine.Defuzzify().ToString())
        Catch ex As Exception
            Call ex.PrintException
        End Try
