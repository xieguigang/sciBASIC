Imports System.IO
Imports BufferedReader = System.IO.StreamReader
Imports FileReader = System.IO.FileStream

''' <summary>
''' Performs principal component analysis on a set of data and returns the resulting data set. The
''' QR algorithm is used to find the eigenvalues and orthonormal eigenvectors of the covariance
''' matrix of the data set. The eigenvectors corresponding to the largest eigenvalues are the
''' principal components. The data file should be in the same directory as the PCA.class file.
''' All numbers should be tab-delimited. The first line of the data should be two numbers: the 
''' number of rows R followed by the number of columns C. After that, there should be R lines of 
''' C tab-delimited values. The columns would most likely represent the dimensions of measure; the
''' rows would each represent a single data point.
''' @author	Kushal Ranjan
''' @version	051513
''' </summary>
''' <remarks>https://github.com/kranjan94/Principal-Component-Analysis</remarks>
Public Class PCA

    ''' <summary>
    ''' Performs PCA on a set of data and prints the resulting transformed data set. </summary>
    ''' <param name="args">	args[0] is the name of the file containing the data. args[1] is an integer
    ''' 				giving the number of desired principal components. </param>
    Public Shared Sub Main(args As String())
        If args.Length <> 2 Then
            Console.WriteLine("Invalid number of arguments. Arguments should be <filename> <# components>.")
            Environment.[Exit](0)
        End If
        Dim data__1 As Double()() = Nothing
        Try
            data__1 = parseData(args(0))
        Catch e As System.IndexOutOfRangeException
            Console.[Error].WriteLine("Malformed data table.")
        Catch e As Exception
            Console.[Error].WriteLine("Malformed data file.")
        End Try
        Dim numComps As Integer = Convert.ToInt32(args(1))

        '		Uses previous method
        '		double[][] results = Data.principalComponentAnalysis(data, numComps);
        '		System.out.println(numComps + " principal components:");
        '		Matrix.print(results);
        '		saveResults(results, args[0]);

        Dim scores As Double()() = Data.PCANIPALS(data__1, numComps)
        '		System.out.println("Scores:");
        '		Matrix.print(scores);
        saveResults(scores, args(0))

        Console.WriteLine(Matrix.numMults & " multiplications performed.")
    End Sub

    ''' <summary>
    ''' Uses the file given by filename to construct a table for use by the application.
    ''' All numbers should be tab-delimited.
    ''' The first line of the data should be two numbers: the number of rows R followed by the
    ''' number of columns C. After that, there should be R lines of C tab-delimited values. </summary>
    ''' <param name="filename">	the name of the file containing the data
    ''' @return			a double[][] containing the data in filename </param>
    ''' <exception cref="IOException">	if an error occurs while reading the file </exception>
     'ORIGINAL LINE: private static double[][] parseData(String filename) throws IOException
    Private Shared Function parseData(filename As String) As Double()()
        Dim [in] As BufferedReader = Nothing
        Try
            [in] = New BufferedReader(File.OpenRead(filename))
        Catch e As FileNotFoundException
            Console.[Error].WriteLine("File " & filename & " not found.")
        End Try
        Dim firstLine As String = [in].ReadLine()
        Dim dims As String() = firstLine.StringSplit(",", True)
        ' <# points> <#dimensions>
        'ORIGINAL LINE: double[][] data = new double[Convert.ToInt32(dims[1])][Convert.ToInt32(dims[0])];
        Dim data As Double()() = MAT(Of Double)(Convert.ToInt32(dims(1)), Convert.ToInt32(dims(0)))
        For j As Integer = 0 To data(0).Length - 1
            Dim text As String = [in].ReadLine()
            Dim vals As String() = text.StringSplit(",", True)
            For i As Integer = 0 To data.Length - 1
                data(i)(j) = Convert.ToDouble(vals(i))
            Next
        Next
        Try
            [in].Close()
        Catch e As Exception
            Console.[Error].WriteLine(e)
        End Try
        Return data
    End Function

    ''' <summary>
    ''' Saves the results of PCA to a file. The filename has "_processed" appended to it before
    ''' the extension. </summary>
    ''' <param name="results">	double[][] of PCA results </param>
    ''' <param name="filename">	original filename of data </param>
    Private Shared Sub saveResults(results As Double()(), filename As String)
        Dim filenameComps As String() = filename.StringSplit("\.", True)
        Dim newFilename As String = filenameComps(0) & "_processed"
        If filenameComps.Length = 2 Then
            'Add filename extension
            newFilename += "." & filenameComps(1)
        End If
        Dim out As StreamWriter = Nothing
        Try
            out = New StreamWriter(File.OpenWrite(newFilename))
        Catch e As Exception
            Console.[Error].WriteLine("Error trying to write new file.")
        End Try
        For i As Integer = 0 To results(0).Length - 1
            For j As Integer = 0 To results.Length - 1
                Try
                    out.write("" & results(j)(i))
                    If j <> results.Length - 1 Then
                        out.write(",")
                    Else
                        out.write(vbLf)
                    End If
                Catch e As Exception
                    Console.[Error].WriteLine("Error trying to write new file.")
                End Try
            Next
        Next
    End Sub
End Class

