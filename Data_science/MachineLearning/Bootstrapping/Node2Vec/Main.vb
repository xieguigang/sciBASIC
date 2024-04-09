Imports System
Imports System.Collections.Generic
Imports System.IO

Namespace node2vec
    'using ArgumentParsers = net.sourceforge.argparse4j.ArgumentParsers;
    'using Arguments = net.sourceforge.argparse4j.impl.Arguments;
    'using ArgumentParser = net.sourceforge.argparse4j.inf.ArgumentParser;
    'using ArgumentParserException = net.sourceforge.argparse4j.inf.ArgumentParserException;
    'using Namespace = net.sourceforge.argparse4j.inf.Namespace;
    'using Model = word2vec.Model;


    ''' <summary>
    ''' Created by freemso on 17-3-14.
    ''' </summary>
    Public Class Main2
        Public Shared Sub Main(args As String())
            ' parse arguments
            'ArgumentParser parser = ArgumentParsers.newArgumentParser("node2vec").defaultHelp(true).description("Run node2vec");
            'parser.addArgument("-i", "--input").nargs("?").setDefault("graph/karate.edgelist").help("Input graph edge information path");
            'parser.addArgument("-o", "--output").nargs("?").setDefault("emb/karate.emb").help("Output embedding path");
            'parser.addArgument("--dimensions").type(typeof(Integer)).setDefault(128).help("Number of dimensions. Default is 128");
            'parser.addArgument("--walkLength").type(typeof(Integer)).setDefault(80).help("Length og walk per source. Default is 80");
            'parser.addArgument("--numWalks").type(typeof(Integer)).setDefault(10).help("Number of walks per source. Default is 10");
            'parser.addArgument("--windowSize").type(typeof(Integer)).setDefault(10).help("Context size for optimization. Default is 10");
            'parser.addArgument("--iter").type(typeof(Integer)).setDefault(1).help("Number of epochs in SGD");
            'parser.addArgument("--workers").type(typeof(Integer)).setDefault(8).help("Number of parallel workers. Default is 8");
            'parser.addArgument("-p", "--p").type(typeof(Double)).setDefault(1.0).help("Return hyperparameter. Default is 1");
            'parser.addArgument("-q", "--q").type(typeof(Double)).setDefault(1.0).help("Inout hyperparameter. Default is 1");
            'parser.addArgument("--weighted").dest("weighted").action(Arguments.storeTrue()).help("Boolean specifying (un)weighted. Default is unweighted");
            'parser.addArgument("--unweighted").dest("weighted").action(Arguments.storeFalse());
            'parser.setDefault("weighted", false);
            'parser.addArgument("--directed").dest("directed").action(Arguments.storeTrue()).help("node2vec.Graph is (un)directed. Default is undirected");
            'parser.addArgument("--undirected").dest("directed").action(Arguments.storeFalse());
            'parser.setDefault("directed", false);


            ' Namespace ns = parser.parseArgs(args);
            Dim ns As Object = Nothing

            Dim graph As Graph = New Graph(ns.[get]("input"), ns.getBoolean("directed"), ns.getDouble("p"), ns.getDouble("q"))
            Dim pathList As IList(Of IList) = graph.simulateWalks(ns.getInt("numWalks"), ns.getInt("walkLength"))

            Console.WriteLine("Learning Embedding...")

            ' convert path list to string
            Dim sentList = ""
            For Each path As IList(Of Graph.Node) In pathList
                Dim sent = ""
                For Each node In path
                    sent += node.Id.ToString() & " "
                Next
                sentList += sent & vbLf
            Next
            ' write to temp file
            'string tempPath = System.getProperty("java.io.tmpdir");
            'File tempFile = File.createTempFile("pathList", "txt", new File(tempPath));
            Dim fw As StreamWriter = Nothing '= new StreamWriter(tempFile);
            fw.Write(sentList)
            fw.Flush()
            fw.Close()
            ' use word2vec to do word embedding
            '	Model model = new Model(false, ns.getInt("dimensions"), ns.getInt("windowSize"), null, null);
            '	model.learnFile(tempFile);
            '	model.storeModel(new File(ns.getString("output")));



        End Sub
    End Class

End Namespace
