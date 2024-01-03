
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.factory
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model.impl
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.model
Imports std = System.Math

Public Class Runtime

    Public Shared Sub Main(args As String())
        Dim f1 As [Function] = New FunctionAnonymousInnerClass()
        Dim data1 = DataGenerator.generateDataTuples(f1, 0.0, 2.0 * std.PI, 200)

        Dim f2 As [Function] = New FunctionAnonymousInnerClass2()
        Dim data2 = DataGenerator.generateDataTuples(f2, 0.0, 10.0, 100)

        Dim factory As ExpressionFactory = New ExpressionFactory()
        factory.TerminalExpressions = New Expression() {Variable.X, New Number(1.0)}
        factory.BinaryExpressions = New CompositeExpression() {CompositeExpression.PLUS, CompositeExpression.MINUS, CompositeExpression.MULTIPLY, CompositeExpression.DIVIDE}
        factory.UnaryExpressions = New CompositeExpression() {CompositeExpression.SINE}

        Dim evolution As evolution.Evolution = New evolution.Evolution()
        evolution.ExpressionFactory = factory

        runGP(data1, evolution, 10)
        runGA(data2, evolution, 10)
    End Sub

    Private Class FunctionAnonymousInnerClass
        Implements [Function]
        Public Function eval(x As Double) As Double Implements [Function].eval
            Return std.Cos(2.0 * x)
        End Function
    End Class

    Private Class FunctionAnonymousInnerClass2
        Implements [Function]
        Public Function eval(x As Double) As Double Implements [Function].eval
            Return x * x * x - 2.0 * x * x + x
        End Function
    End Class

    Private Shared Sub runGP(data As IList(Of Tuple), evolution As evolution.Evolution, n As Integer)
        Dim config As GPConfiguration = GPConfiguration.createDefaultConfig()
        config.objective = ObjectiveFunction.MAE
        config.fitnessThreshold = 0.01
        config.initTreeDepth = 4

        Dim results As IList(Of evolution.Evolution.Result) = New List(Of evolution.Evolution.Result)(n)
        For i = 0 To n - 1
            results.Add(evolution.evolveTreeFor(data, config))
        Next
        exportResults("GP", results)
    End Sub

    Private Shared Sub runGA(data As IList(Of Tuple), evolution As evolution.Evolution, n As Integer)
        Dim config As GAConfiguration = GAConfiguration.createDefaultConfig()
        config.objective = ObjectiveFunction.MAE
        config.fitnessThreshold = 0.1
        config.initPolyOrder = 3
        config.paramRangeFrom = -5.0
        config.paramRangeTo = +5.0

        Dim results As IList(Of evolution.Evolution.Result) = New List(Of evolution.Evolution.Result)(n)
        For i = 0 To n - 1
            results.Add(evolution.evolvePolyFor(data, config))
        Next
        exportResults("GA", results)
    End Sub

    Private Shared Sub exportResults(filePrefix As String, results As IList(Of evolution.Evolution.Result))
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

End Class
