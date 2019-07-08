Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module CLI

    ''' <summary>
    ''' 这个命令只适用于一般性的非线性模型的计算
    ''' 
    ''' f(X) = x1 ^ p(X) + x2 ^ p(X) + x3 ^ p(X) + ....
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("/evolution")>
    <Description("Fit a general linear model use GA method")>
    <Usage("/evolution /in <traningSet.csv> /popSize <population_size, default=5000> [/out <dump.directory>]")>
    Public Function doEvolution(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim popSize% = args("/popSize") Or 5000
        Dim out$ = args("/out") Or $"{[in].TrimSuffix}.GA_fitting/"

    End Function
End Module
