#Region "Microsoft.VisualBasic::b616ede9055110bddee8f617480ea394, Data_science\Mathematica\Math\GeneticProgramming\test\Program.vb"

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

    '   Total Lines: 112
    '    Code Lines: 59 (52.68%)
    ' Comment Lines: 36 (32.14%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (15.18%)
    '     File Size: 4.52 KB


    ' Module Program
    ' 
    '     Function: test1, test2
    ' 
    '     Sub: exportResults, Main, runGA, runGP
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.factory
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.impl
Imports std = System.Math

Module Program
    Sub Main(args As String())
        Dim f1 As DataGenerator.DataFunction = AddressOf test1
        Dim data1 = DataGenerator.generateDataTuples(f1, 0.0, 2.0 * std.PI, 200).ToArray

        Dim f2 As DataGenerator.DataFunction = AddressOf test2
        Dim data2 = DataGenerator.generateDataTuples(f2, 0.0, 10.0, 100).ToArray

        Dim factory As ExpressionFactory = New ExpressionFactory()
        factory.TerminalExpressions = New Expression() {Variable.X, New Number(1.0), E.e, PI.Pi, Tau.Tau, New Number(12)}
        factory.BinaryExpressions = New CompositeExpression() {CompositeExpression.PLUS, CompositeExpression.MINUS, CompositeExpression.MULTIPLY, CompositeExpression.DIVIDE}
        factory.UnaryExpressions = New CompositeExpression() {CompositeExpression.SINE}

        Dim evolution As Evolution = New Evolution()
        evolution.ExpressionFactory = factory

        runGP(data1, evolution, 10)
        runGA(data2, evolution, 10)
    End Sub

    Private Function test1(x As Double) As Double
        Return std.Cos(2.0 * x)
    End Function

    Private Function test2(x As Double) As Double
        Return x * x * x - 2.0 * x * x + x
    End Function

    Private Sub runGP(data As DataPoint(), evolution As Evolution, n As Integer)
        Dim config As GPConfiguration = GPConfiguration.createDefaultConfig()
        config.objective = ObjectiveFunction.MAE
        config.fitnessThreshold = 0.01
        config.initTreeDepth = 4

        Dim results As IList(Of EvolutionResult) = New List(Of EvolutionResult)(n)
        For i = 0 To n - 1
            results.Add(evolution.evolveTreeFor(data, config))
        Next
        exportResults("GP", results)
    End Sub

    Private Sub runGA(data As DataPoint(), evolution As Evolution, n As Integer)
        Dim config As GAConfiguration = GAConfiguration.createDefaultConfig()
        config.objective = ObjectiveFunction.MAE
        config.fitnessThreshold = 0.1
        config.initPolyOrder = 3
        config.paramRangeFrom = -5.0
        config.paramRangeTo = +5.0

        Dim results As IList(Of EvolutionResult) = New List(Of EvolutionResult)(n)
        For i = 0 To n - 1
            results.Add(evolution.evolvePolyFor(data, config))
        Next
        exportResults("GA", results)
    End Sub

    Private Sub exportResults(filePrefix As String, results As IList(Of EvolutionResult))
        For Each item In results
            Call Console.WriteLine($"{filePrefix}  --  {item.ToString}")
        Next

        'try
        '{
        '	StreamWriter @out = new StreamWriter(filePrefix + ".out");
        '	StreamWriter fit = new StreamWriter(filePrefix + ".fit");
        '	StreamWriter time = new StreamWriter(filePrefix + ".time");

        '	foreach (Evolution.Result result in results)
        '	{
        '		@out.Write(Convert.ToString(result.fitness));
        '		@out.BaseStream.WriteByte(' ');
        '		@out.Write(Convert.ToString(result.time));
        '		@out.BaseStream.WriteByte(' ');
        '		@out.Write(Convert.ToString(result.epochs));
        '		@out.BaseStream.WriteByte('\n');
        '		@out.Flush();

        '		IEnumerator<double> fitIter = result.fitnessProgress.GetEnumerator();
        '		IEnumerator<long> timeIter = result.timeProgress.GetEnumerator();
        '		while (fitIter.MoveNext())
        '		{
        '			fit.Write(Convert.ToString(fitIter.Current));
        '			fit.BaseStream.WriteByte(' ');
        '			time.Write(Convert.ToString(timeIter.next()));
        '			time.BaseStream.WriteByte(' ');
        '		}
        '		fit.BaseStream.WriteByte('\n');
        '		fit.Flush();
        '		time.BaseStream.WriteByte('\n');
        '		time.Flush();
        '	}

        '	@out.Close();
        '	fit.Close();
        '	time.Close();
        '}
        'catch (IOException e)
        '{
        '	e.printStackTrace(System.err);
        '}
    End Sub

End Module
