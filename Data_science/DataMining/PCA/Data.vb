
''' <summary>
''' Holds the information of a data set. Each row contains a single data point. Primary computations
''' of PCA are performed by the Data object.
''' @author	Kushal Ranjan
''' @version	051313
''' </summary>
Friend Class Data
    Friend matrixData As Double()()
    'matrix[i] is the ith row; matrix[i][j] is the ith row, jth column
    ''' <summary>
    ''' Constructs a new data matrix. </summary>
    ''' <param name="vals">	data for new Data object; dimensions as columns, data points as rows. </param>
    Friend Sub New(vals As Double()())
        matrixData = Matrix.copy(vals)
    End Sub

	''' <summary>
	''' Test code. Constructs an arbitrary data table of 5 data points with 3 variables, normalizes
	''' it, and computes the covariance matrix and its eigenvalues and orthonormal eigenvectors.
	''' Then determines the two principal components.
	''' </summary>
	Public Shared Sub Main(args As String())
		Dim data__1 As Double()() = {New Double() {4, 4.2, 3.9, 4.3, 4.1}, New Double() {2, 2.1, 2, 2.1, 2.2}, New Double() {0.6, 0.59, 0.58, 0.62, 0.63}}
		Console.WriteLine("Raw data:")
		Matrix.print(data__1)
		Dim dat As New Data(data__1)
		dat.center()
		Dim cov As Double()() = dat.covarianceMatrix()
		Console.WriteLine("Covariance matrix:")
		Matrix.print(cov)
		Dim eigen As EigenSet = dat.CovarianceEigenSet
		Dim vals As Double()() = {eigen.values}
		Console.WriteLine("Eigenvalues:")
		Matrix.print(vals)
		Console.WriteLine("Corresponding eigenvectors:")
		Matrix.print(eigen.vectors)
		Console.WriteLine("Two principal components:")
		Matrix.print(dat.buildPrincipalComponents(2, eigen))
		Console.WriteLine("Principal component transformation:")
        Matrix.print(Data.principalComponentAnalysis(data__1, 2))

        Dim scores#()() = Data.PCANIPALS(data__1, 3)
        Matrix.print(scores)

        Pause()
    End Sub

	''' <summary>
	''' PCA implemented using the NIPALS algorithm. The return value is a double[][], where each
	''' double[] j is an array of the scores of the jth data point corresponding to the desired
	''' number of principal components. </summary>
	''' <param name="input">			input raw data array </param>
	''' <param name="numComponents">	desired number of PCs
	''' @return				the scores of the data array against the PCS </param>
	Friend Shared Function PCANIPALS(input As Double()(), numComponents As Integer) As Double()()
		Dim data As New Data(input)
		data.center()
		Dim PCA As Double()()() = data.NIPALSAlg(numComponents)
        'ORIGINAL LINE: double[][] scores = new double[numComponents][input[0].Length];
        Dim scores As Double()() = MAT(Of Double)(numComponents, input(0).Length)
        For point As Integer = 0 To scores(0).Length - 1
			For comp As Integer = 0 To PCA.Length - 1
				scores(comp)(point) = PCA(comp)(0)(point)
			Next
		Next
		Return scores
	End Function

	''' <summary>
	''' Implementation of the non-linear iterative partial least squares algorithm on the data
	''' matrix for this Data object. The number of PCs returned is specified by the user. </summary>
	''' <param name="numComponents">	number of principal components desired
	''' @return				a double[][][] where the ith double[][] contains ti and pi, the scores
	''' 						and loadings, respectively, of the ith principal component. </param>
	Friend Overridable Function NIPALSAlg(numComponents As Integer) As Double()()()
		Const  THRESHOLD As Double = 1E-05
		Dim out As Double()()() = New Double(numComponents - 1)()() {}
        Dim E As Double()() = Matrix.copy(matrixData)
        For i As Integer = 0 To out.Length - 1
			Dim eigenOld As Double = 0
			Dim eigenNew As Double = 0
            Dim p As Double() = New Double(matrixData(0).Length - 1) {}
            Dim t As Double() = New Double(matrixData(0).Length - 1) {}
            Dim tMatrix As Double()() = {t}
			Dim pMatrix As Double()() = {p}
			For j As Integer = 0 To t.Length - 1
                t(j) = matrixData(i)(j)
            Next
			Do
				eigenOld = eigenNew
				Dim tMult As Double = 1 / Matrix.dot(t, t)
				tMatrix(0) = t
				p = Matrix.scale(Matrix.multiply(Matrix.transpose(E), tMatrix), tMult)(0)
				p = Matrix.normalize(p)
				Dim pMult As Double = 1 / Matrix.dot(p, p)
				pMatrix(0) = p
				t = Matrix.scale(Matrix.multiply(E, pMatrix), pMult)(0)
				eigenNew = Matrix.dot(t, t)
			Loop While Math.Abs(eigenOld - eigenNew) > THRESHOLD
			tMatrix(0) = t
			pMatrix(0) = p
			Dim PC As Double()() = {t, p}
			'{scores, loadings}
			E = Matrix.subtract(E, Matrix.multiply(tMatrix, Matrix.transpose(pMatrix)))
			out(i) = PC
		Next
		Return out
	End Function

	''' <summary>
	''' Previous algorithms for performing PCA
	''' </summary>

	''' <summary>
	''' Performs principal component analysis with a specified number of principal components. </summary>
	''' <param name="input">			input data; each double[] in input is an array of values of a single
	''' 						variable for each data point </param>
	''' <param name="numComponents">	number of components desired
	''' @return				the transformed data set </param>
	Friend Shared Function principalComponentAnalysis(input As Double()(), numComponents As Integer) As Double()()
		Dim data As New Data(input)
		data.center()
		Dim eigen As EigenSet = data.CovarianceEigenSet
		Dim featureVector As Double()() = data.buildPrincipalComponents(numComponents, eigen)
		Dim PC As Double()() = Matrix.transpose(featureVector)
		Dim inputTranspose As Double()() = Matrix.transpose(input)
		Return Matrix.transpose(Matrix.multiply(PC, inputTranspose))
	End Function

	''' <summary>
	''' Returns a list containing the principal components of this data set with the number of
	''' loadings specified. </summary>
	''' <param name="numComponents">	the number of principal components desired </param>
	''' <param name="eigen">			EigenSet containing the eigenvalues and eigenvectors
	''' @return				the numComponents most significant eigenvectors </param>
	Friend Overridable Function buildPrincipalComponents(numComponents As Integer, eigen As EigenSet) As Double()()
		Dim vals As Double() = eigen.values
		If numComponents > vals.Length Then
			Throw New Exception("Cannot produce more principal components than those provided.")
		End If
		Dim chosen As Boolean() = New Boolean(vals.Length - 1) {}
		Dim vecs As Double()() = eigen.vectors
		Dim PC As Double()() = New Double(numComponents - 1)() {}
		For i As Integer = 0 To PC.Length - 1
			Dim max As Integer = 0
			While chosen(max)
				max += 1
			End While
			For j As Integer = 0 To vals.Length - 1
				If Math.Abs(vals(j)) > Math.Abs(vals(max)) AndAlso Not chosen(j) Then
					max = j
				End If
			Next
			chosen(max) = True
			PC(i) = vecs(max)
		Next
		Return PC
	End Function

	''' <summary>
	''' Uses the QR algorithm to determine the eigenvalues and eigenvectors of the covariance 
	''' matrix for this data set. Iteration continues until no eigenvalue changes by more than 
	''' 1/10000.
	''' @return	an EigenSet containing the eigenvalues and eigenvectors of the covariance matrix
	''' </summary>
	Friend Overridable ReadOnly Property CovarianceEigenSet() As EigenSet
		Get
			Dim data As Double()() = covarianceMatrix()
			Return Matrix.eigenDecomposition(data)
		End Get
	End Property

	''' <summary>
	''' Constructs the covariance matrix for this data set.
	''' @return	the covariance matrix of this data set
	''' </summary>
	Friend Overridable Function covarianceMatrix() As Double()()
        'ORIGINAL LINE: double[][] @out = new double[matrix.Length][matrix.Length];
        Dim out As Double()() = MAT(Of Double)(matrixData.Length, matrixData.Length)
        For i As Integer = 0 To out.Length - 1
			For j As Integer = 0 To out.Length - 1
                Dim dataA As Double() = matrixData(i)
                Dim dataB As Double() = matrixData(j)
                out(i)(j) = covariance(dataA, dataB)
			Next
		Next
		Return out
	End Function

	''' <summary>
	''' Returns the covariance of two data vectors. </summary>
	''' <param name="a">	double[] of data </param>
	''' <param name="b">	double[] of data
	''' @return	the covariance of a and b, cov(a,b) </param>
	Friend Shared Function covariance(a As Double(), b As Double()) As Double
		If a.Length <> b.Length Then
			Throw New MatrixException("Cannot take covariance of different dimension vectors.")
		End If
		Dim divisor As Double = a.Length - 1
		Dim sum As Double = 0
		Dim aMean As Double = mean(a)
		Dim bMean As Double = mean(b)
		For i As Integer = 0 To a.Length - 1
			sum += (a(i) - aMean) * (b(i) - bMean)
		Next
		Return sum / divisor
	End Function

	''' <summary>
	''' Centers each column of the data matrix at its mean.
	''' </summary>
	Friend Overridable Sub center()
        matrixData = normalize(matrixData)
    End Sub


	''' <summary>
	''' Normalizes the input matrix so that each column is centered at 0.
	''' </summary>
	Friend Overridable Function normalize(input As Double()()) As Double()()
        'ORIGINAL LINE: double[][] @out = new double[input.Length][input[0].Length];
        Dim out As Double()() = MAT(Of Double)(input.Length, input(0).Length)
        For i As Integer = 0 To input.Length - 1
            Dim meanValue As Double = mean(input(i))
            For j As Integer = 0 To input(i).Length - 1
                out(i)(j) = input(i)(j) - meanValue
            Next
		Next
		Return out
	End Function

	''' <summary>
	''' Calculates the mean of an array of doubles. </summary>
	''' <param name="entries">	input array of doubles </param>
	Friend Shared Function mean(entries As Double()) As Double
		Dim out As Double = 0
		For Each d As Double In entries
			out += d / entries.Length
		Next
		Return out
	End Function
End Class

