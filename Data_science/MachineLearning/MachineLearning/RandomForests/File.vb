Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java

Namespace RandomForests

    Public Class File

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
            Dim false_positive_cost As Double
            Dim false_negative_cost As Double
            If demoProperties("false_positive_cost") Is Nothing Then
                false_positive_cost = 0
            Else
                false_positive_cost = Double.Parse(demoProperties("false_positive_cost"))
            End If
            If demoProperties("false_negative_cost") Is Nothing Then
                false_negative_cost = 0
            Else
                false_negative_cost = Double.Parse(demoProperties("false_negative_cost"))
            End If

            ''' <summary>
            ''' End loading parameter file
            ''' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            ''' </summary>

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


            ''' <summary>
            ''' Initialize counter variables </summary>
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

            ''' <summary>
            ''' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
            ''' Declaracion de variables                                                 
            ''' </summary>

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


            'Information gain variables
            Dim Loss As Double = 0

            'Output files
            Dim outTree As StreamWriter '("Trees.txt")
            Dim outTreeTest As StreamWriter '("Trees.test")
            Dim outSel As StreamWriter '("TimesSelected.txt")
            Dim outVI As StreamWriter '("Variable_Importance.txt")
            Dim outEGBV As StreamWriter '("EGBV.txt")
            Dim outPred As StreamWriter '("Predictions.txt")
            ''' <summary>
            ''' %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% </summary>

            ''' <summary>
            ''' 1. Start reading files
            ''' </summary>
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