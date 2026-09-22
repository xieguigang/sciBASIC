#Region "Microsoft.VisualBasic::9a5398730502b31c9c237c1e894ec358, Data_science\MachineLearning\MachineLearning\RandomForests\File.vb"

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

    '   Total Lines: 299
    '    Code Lines: 153 (51.17%)
    ' Comment Lines: 121 (40.47%)
    '    - Xml Docs: 65.29%
    ' 
    '   Blank Lines: 25 (8.36%)
    '     File Size: 14.01 KB


    '     Enum LF_c
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class Data
    ' 
    '         Properties: attributeNames, Genotype, ID, N_attributes, N_tot
    '                     phenotype
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class File
    ' 
    '         Function: Read
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure

Namespace RandomForests

    ''' <summary>
    ''' Loss function used for continuous features
    ''' </summary>
    Public Enum LF_c
        ''' <summary>
        ''' Split the branch by maximizing the information gain.
        ''' </summary>
        <Description("Information Gain")> Information_Gain = 1
        ''' <summary>
        ''' Split the branch by minimizing the mean squared error (the L2 loss function).
        ''' </summary>
        <Description("Mean Squared Error (L2 function)")> Mean_Squared_Error = 2
        ''' <summary>
        ''' Split the branch by the pseudo Huber loss, which is a smooth 
        ''' approximation of the L1 loss.
        ''' </summary>
        <Description("Pseudo Huber")> Pseudo_Huber = 3
        ''' <summary>
        ''' Split the branch by a personalized cost function for the category features.
        ''' </summary>
        <Description("Personalized Cost Function for categories")> Personalized_Cost_Function_for_categories = 4
        ''' <summary>
        ''' Split the branch by minimizing the Gini impurity index.
        ''' </summary>
        <Description("Gini Index")> Gini_Index = 5
    End Enum

    ''' <summary>
    ''' the training dataset
    ''' </summary>
    Public Class Data

        ''' <summary>
        ''' The unique reference id of each sample.
        ''' </summary>
        ''' <returns>An array of the sample id strings.</returns>
        Public Property ID As String()
        ''' <summary>
        ''' the actual label
        ''' </summary>
        ''' <returns>An array of the label value of each sample.</returns>
        Public Property phenotype As Double()

        ''' <summary>
        ''' The feature matrix of the training dataset, in which each row is the 
        ''' feature vector of one sample.
        ''' </summary>
        ''' <returns>A jagged array in layout ``[sample, feature]``.</returns>
        Public Property Genotype As Double()()

        ''' <summary>
        ''' the feature names
        ''' </summary>
        ''' <returns>An array of the feature names.</returns>
        Public Property attributeNames As String()

        ''' <summary>
        ''' The number of the feature attributes in each sample.
        ''' </summary>
        ''' <returns>The length of the <see cref="attributeNames"/> array.</returns>
        Public ReadOnly Property N_attributes As Integer
            Get
                Return attributeNames.Length
            End Get
        End Property

        ''' <summary>
        ''' The total number of the samples in this training dataset.
        ''' </summary>
        ''' <returns>The length of the <see cref="phenotype"/> array.</returns>
        Public ReadOnly Property N_tot As Integer
            Get
                Return phenotype.Length
            End Get
        End Property

        ''' <summary>
        ''' Create a new empty training dataset.
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Create the training dataset from a machine learning data frame.
        ''' </summary>
        ''' <param name="df">
        ''' A <see cref="MLDataFrame"/> object; only the first label value of 
        ''' each sample will be used as the phenotype value.
        ''' </param>
        Sub New(df As MLDataFrame)
            ID = df.samples.Select(Function(a) a.id).ToArray
            phenotype = df.samples.Select(Function(a) a.labels(0)).ToArray
            Genotype = df.samples.Select(Function(a) a.features).ToArray
            attributeNames = df.featureNames
        End Sub

    End Class

    ''' <summary>
    ''' The legacy parameter file reader of the RanFog program.
    ''' </summary>
    ''' <remarks>
    ''' This type is kept for reading the original RanFoG parameter/training 
    ''' file format, the file streams are declared but not assigned, so this 
    ''' reader is not functional anymore; use the <see cref="Data"/> class 
    ''' together with the <see cref="RanFog"/> model instead.
    ''' </remarks>
    Public Class File

        ''' <summary>
        ''' Load the RanFoG program parameters from a property dictionary, and 
        ''' then read the training/testing data files.
        ''' </summary>
        ''' <param name="demoProperties">
        ''' A property dictionary which should contains the keys ``ForestSize``, 
        ''' ``N_features``, ``mtry``, ``max_branch`` and ``LossFunction``.
        ''' </param>
        ''' <remarks>
        ''' The training and testing <see cref="FileStream"/> objects are declared 
        ''' but never assigned in the current implementation, so no data will be 
        ''' loaded at all; only the parameter values are echoed to the console.
        ''' </remarks>
        Public Shared Function Read(demoProperties As Dictionary(Of String, String))
            'Max number of trees to be constructed
            Dim max_tree = Integer.Parse(demoProperties("ForestSize"))
            'Number of classified Features
            Dim N_SNP = Integer.Parse(demoProperties("N_features"))
            'Name of training file; Load training file
            Dim trnFile As FileStream '(demoProperties("training"))
            'Name of testing file; Load testing file
            Dim tstFile As FileStream '(demoProperties("testing"))
            'Number of Features randomly selected at each node
            Dim m = Double.Parse(demoProperties("mtry")) 'Percentage of Features randomly selected at each node
            'Max number of branches allowed
            Dim max_branch = Integer.Parse(demoProperties("max_branch"))
            'Loss function used for discrete features
            '		String LF_d=demoProperties("LossFunction_discrete");
            'Loss function used for continuous features
            Dim LF_c As String = demoProperties("LossFunction")

            '
            ' End loading parameter file
            ' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            '

            'Set the arguments to the respective variables.
            Console.WriteLine("Number of trees to be grown: " & max_tree)
            'int N_SNP=Integer.parseInt(args[3]); //Number of classified Features
            Dim N_attributes = 0 'Number of total features
            Console.WriteLine("Number of SNPs (classified Features): " & N_SNP)
            Console.Write("RanFoG will run with Loss Function option: ")
            If Integer.Parse(LF_c) = 1 Then
                Console.WriteLine("Information Gain")
            ElseIf Integer.Parse(LF_c) = 2 Then
                Console.WriteLine("Mean Squared Error (L2 function)")
            ElseIf Integer.Parse(LF_c) = 3 Then
                Console.WriteLine("Pseudo Huber")
            ElseIf Integer.Parse(LF_c) = 4 Then
                Console.WriteLine("Personalized Cost Function for categories")
            ElseIf Integer.Parse(LF_c) = 5 Then
                Console.WriteLine("Gini Index")
            End If
            Console.WriteLine()


            '
            ' Initialize counter variables
            Dim j = 0, jj = 0, k = 0, i = 0, N_tot = 0, N_tst = 0, N_oob = 0

            ' read the number of lines in the training file
            Try
                Dim inFile As StreamReader = New StreamReader(trnFile)
                Dim line As Value(Of String) = ""
                While Not (line = inFile.ReadLine()) Is Nothing
                    N_tot = N_tot + 1
                    Dim st As StringTokenizer = New StringTokenizer(line, " ")
                    ' b/w "" put the delimiter(,). 
                    ' st.nextToken() is a String, so you can manipulate on it e.g.:
                    'System.out.println("number of columns="+st.countTokens());
                    If N_tot = 1 Then
                        N_attributes = st.countTokens() - 2
                    End If
                    If st.countTokens() <> N_attributes + 2 Then
                        Console.WriteLine("Training file with less columns than expected at line " & N_tot)
                        GC.WaitForPendingFinalizers()
                    End If
                End While
                inFile.Close()
            Catch __unusedFileNotFoundException1__ As FileNotFoundException
                Console.WriteLine("Training file for training set not found. ")
            End Try
            Console.WriteLine("Number of genotypes (lines) in training set: " & N_tot)
            Console.WriteLine("Number of total Attributes detected: " & N_attributes)
            Console.WriteLine()

            ' read the number of lines in the testing file
            Try
                Dim testing As StreamReader = New StreamReader(tstFile)
                Dim line As Value(Of String) = ""
                While Not (line = testing.ReadLine()) Is Nothing
                    N_tst = N_tst + 1
                    Dim st As StringTokenizer = New StringTokenizer(line, " ")
                    If st.countTokens() <> N_attributes + 2 Then
                        Console.WriteLine("Testing file with less columns than expected at line " & i)
                        GC.WaitForPendingFinalizers()
                    End If
                End While
                testing.Close()
            Catch __unusedFileNotFoundException1__ As FileNotFoundException
                Console.WriteLine("Testing file for training set not found. ")
            End Try
            Console.WriteLine("Number of genotypes (lines) in testing set: " & N_tst)

            '
            ' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            ' Declaracion de variables                                                 
            '

            'Variables read from files
            Dim phenotype = New Double(N_tot - 1) {}
            Dim ID = New String(N_tot - 1) {}
            Dim phenotype_tst = New Double(N_tst - 1) {}
            Dim ID_tst = New String(N_tst - 1) {}
            Dim Genotype = RectangularArray.Matrix(Of Double)(N_tot, N_attributes)
            Dim Genotype_tst = RectangularArray.Matrix(Of Double)(N_tst, N_attributes)

            'Variables involved in the trees
            Dim mean_j, minLoss, MSE_tree, MSEval_tree, node_mse, temp As Double
            Dim node, n_branch, n_tree As Integer
            Dim FeatureSel = New Integer(1) {} 'Feature selected at each node [regression feature,classified features]
            'out of bag variables
            Dim oob = New Integer(N_tot - 1) {}
            Dim MSE_oob, MSE_vi As Double, MSE_oob_ave As Double = 0
            'Predictive and estimated variables
            Dim GEBV = RectangularArray.Matrix(Of Double)(N_tot, 2) 'Predicted phenotype in training set
            Dim y_hat = New Double(N_tst - 1) {} 'Predicted phenotype in testing set
            Dim Selected = New Integer(N_attributes - 1) {} 'number of times SNPs are selected
            Dim VI = New Double(N_attributes - 1) {}


            '
            ' 1. Start reading files
            '
            'read the training set file
            i = 0
            Try
                Dim inFile As StreamReader = New StreamReader(trnFile)
                Dim line As Value(Of String) = ""
                'inFile.readLine(); //Read header
                While Not (line = inFile.ReadLine()) Is Nothing
                    i += 1
                    Dim st As StringTokenizer = New StringTokenizer(line, " ")
                    ' b/w "" put the delimiter(,). In case there are arbitrary spaces
                    ' in addition to commas, e.g., a, b, c, add a single space after t$
                    ' st.nextToken() is a String, so you can manipulate on it e.g.:
                    phenotype(i - 1) = Double.Parse(st.nextToken())
                    ID(i - 1) = st.nextToken()
                    'st.nextToken();//***** to not read other fields!!****
                    For j = 0 To N_attributes - 1 'for each SNP two dummy variables are created
                        Genotype(i - 1)(j) = Double.Parse(st.nextToken())
                    Next
                End While
                inFile.Close()
            Catch __unusedFileNotFoundException1__ As FileNotFoundException
                Console.WriteLine("Training file for Features with wrong format. ")
            End Try 'end reading training file

            'read the testing set file
            i = 0
            Try
                Dim testing As StreamReader = New StreamReader(tstFile)
                Dim line As Value(Of String) = ""
                'inFile.readLine(); //Read header
                While Not (line = testing.ReadLine()) Is Nothing
                    i += 1
                    Dim st As StringTokenizer = New StringTokenizer(line, " ")
                    phenotype_tst(i - 1) = Double.Parse(st.nextToken())
                    ID_tst(i - 1) = st.nextToken()
                    'st.nextToken();//***** to not read other fields!!****
                    For j = 0 To N_attributes - 1 'for each SNP
                        Genotype_tst(i - 1)(j) = Double.Parse(st.nextToken())
                    Next
                End While
                testing.Close()
            Catch __unusedFileNotFoundException1__ As FileNotFoundException
                Console.WriteLine("Testing file for Features with wrong format. ")
            End Try 'end reading testing file
        End Function
    End Class
End Namespace
