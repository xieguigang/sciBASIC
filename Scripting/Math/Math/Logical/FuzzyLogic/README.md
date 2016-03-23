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
    LinguisticVariable water = new LinguisticVariable("Water"); 
    water.MembershipFunctionCollection.Add(new MembershipFunction("Cold", 0, 0, 20, 40)); 
    water.MembershipFunctionCollection.Add(new MembershipFunction("Tepid", 30, 50, 50, 70)); 
    water.MembershipFunctionCollection.Add(new MembershipFunction("Hot", 50, 80, 100, 100));

    LinguisticVariable power = new LinguisticVariable("Power"); 
    power.MembershipFunctionCollection.Add(new MembershipFunction("Low", 0, 25, 25, 50)); 
    power.MembershipFunctionCollection.Add(new MembershipFunction("High", 25, 50, 50, 75));

    FuzzyEngine fuzzyEngine = new FuzzyEngine(); 
    fuzzyEngine.LinguisticVariableCollection.Add(water); 
    fuzzyEngine.LinguisticVariableCollection.Add(power); 
    fuzzyEngine.Consequent = "Power"; 
    fuzzyEngine.FuzzyRuleCollection.Add(new FuzzyRule("IF (Water IS Cold) OR (Water IS Tepid) THEN Power IS High")); 
    fuzzyEngine.FuzzyRuleCollection.Add(new FuzzyRule("IF (Water IS Hot) THEN Power IS Low"));

    water.InputValue = 60;

    try 
    { 
        MessageBox.Show(fuzzyEngine.Defuzzify().ToString()); 
    } 
    catch (Exception e) 
    { 
        MessageBox.Show(e.Message); 
    }