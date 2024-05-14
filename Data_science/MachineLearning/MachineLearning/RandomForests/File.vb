#Region "Microsoft.VisualBasic::580318ff3d8d9223510fee323cb63ab6, Data_science\MachineLearning\MachineLearning\RandomForests\File.vb"

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

    '   Total Lines: 219
    '    Code Lines: 144
    ' Comment Lines: 53
    '   Blank Lines: 22
    '     File Size: 10.39 KB


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

Namespace RandomForests

    ''' <summary>
    ''' Loss function used for continuous features
    ''' </summary>
    Public Enum LF_c
        <Description("Information Gain")> Information_Gain = 1
        <Description("Mean Squared Error (L2 function)")> Mean_Squared_Error = 2
        <Description("Pseudo Huber")> Pseudo_Huber = 3
        <Description("Personalized Cost Function for categories")> Personalized_Cost_Function_for_categories = 4
        <Description("Gini Index")> Gini_Index = 5
    End Enum

    Public Class Data

        Public Property ID As String()
        ''' <summary>
        ''' the actual label
        ''' </summary>
        ''' <returns></returns>
        Public Property phenotype As Double()
        Public Property Genotype As Double()()

        ''' <summary>
        ''' the feature names
        ''' </summary>
        ''' <returns></returns>
        Public Property attributeNames As String()

        Public ReadOnly Property N_attributes As Integer
            Get
                Return attributeNames.Length
            End Get
        End Property

        Public ReadOnly Property N_tot As Integer
            Get
                Return phenotype.Length
            End Get
        End Property

    End Class

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
