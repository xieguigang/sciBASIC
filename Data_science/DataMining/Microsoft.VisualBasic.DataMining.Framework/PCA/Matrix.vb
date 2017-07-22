
''' <summary>
''' Class for performing matrix calculations specific to PCA.
''' @author	Kushal Ranjan
''' @version	051413
''' </summary>
Friend Class Matrix

	Friend Shared numMults As Integer = 0
	'Keeps track of the number of multiplications performed
	''' <summary>
	''' Test code for SVD. Uses example from MIT video: http://www.youtube.com/watch?v=cOUTpqlX-Xs
	''' </summary>
	Public Shared Sub Main(args As String())
		Console.WriteLine("Original matrix:")
		Dim test As Double()() = {New Double() {5, -1}, New Double() {5, 7}}
		'C
		Matrix.print(test)
		Dim SVD As Double()()() = Matrix.singularValueDecomposition(test)
		Dim U As Double()() = SVD(0)
		Dim S As Double()() = SVD(1)
		Dim V As Double()() = SVD(2)
		Console.WriteLine("U-matrix:")
		Matrix.print(U)
		Console.WriteLine("Sigma-matrix:")
		Matrix.print(S)
		Console.WriteLine("V-matrix:")
		Matrix.print(V)
		Console.WriteLine("Decomposition product (C = US(V^T)):")
		Matrix.print(Matrix.multiply(U, Matrix.multiply(S, Matrix.transpose(V))))
		'Should be C
	End Sub


	''' <summary>
	''' Computes the singular value decomposition (SVD) of the input matrix. </summary>
	''' <param name="input">		the input matrix
	''' @return			the SVD of input, {U,S,V}, such that input = US(V^T). U and S are
	''' 					orthogonal matrix, and the non-zero entries of the diagonal matrix S are
	''' 					the  </param>
	Friend Shared Function singularValueDecomposition(input As Double()()) As Double()()()
		Dim C As Double()() = Matrix.copy(input)
		Dim CTC As Double()() = multiply(transpose(C), C)
		'(C^T)C = V(S^T)S(V^T)
		Dim eigenC As EigenSet = eigenDecomposition(CTC)
        'ORIGINAL LINE: double[][] S = new double[C.Length][C.Length]; //Diagonal matrix
        Dim S As Double()() = MAT(Of Double)(C.Length, C.Length)
        'Diagonal matrix
        For i As Integer = 0 To S.Length - 1
				'Squareroots of eigenvalues are entries of S
			S(i)(i) = Math.Sqrt(eigenC.values(i))
		Next
		Dim V As Double()() = eigenC.vectors
		Dim CV As Double()() = multiply(C, V)
		'CV = US
		Dim invS As Double()() = copy(S)
		'Inverse of S
		For i As Integer = 0 To invS.Length - 1
			invS(i)(i) = 1.0 / S(i)(i)
		Next
		Dim U As Double()() = multiply(CV, invS)
		'U = CV(S^-1)
		Return New Double()()() {U, S, V}
	End Function

	''' <summary>
	''' Determines the eigenvalues and eigenvectors of a matrix by using the QR algorithm. Repeats
	''' until no eigenvalue changes by more than 1/100000.
	''' @param	input	input matrix; must be square
	''' @return			an EigenSet containing the eigenvalues and corresponding eigenvectors of
	''' 					input
	''' </summary>
	Friend Shared Function eigenDecomposition(input As Double()()) As EigenSet
		If input.Length <> input(0).Length Then
			Throw New MatrixException("Eigendecomposition not defined on nonsquare matrices.")
		End If
        Dim copy As Double()() = Matrix.copy(input)
        'ORIGINAL LINE: double[][] Q = new double[copy.Length][copy.Length];
        Dim Q As Double()() = MAT(Of Double)(copy.Length, copy.Length)
        For i As Integer = 0 To Q.Length - 1
				'Q starts as an identity matrix
			Q(i)(i) = 1
		Next
		Dim done As Boolean = False
		While Not done
			Dim fact As Double()()() = Matrix.QRFactorize(copy)
			Dim newMat As Double()() = Matrix.multiply(fact(1), fact(0))
			'[A_k+1] := [R_k][Q_k]
			Q = Matrix.multiply(fact(0), Q)
			'Stop the loop if no eigenvalue changes by more than 1/100000
			For i As Integer = 0 To copy.Length - 1
				If Math.Abs(newMat(i)(i) - copy(i)(i)) > 1E-05 Then
					copy = newMat
					Exit For
				ElseIf i = copy.Length - 1 Then
					'End of copy table
					done = True
				End If
			Next
		End While
		Dim ret As New EigenSet()
		ret.values = Matrix.extractDiagonalEntries(copy)
		'Eigenvalues lie on diagonal
		ret.vectors = Q
		'Columns of Q converge to the eigenvectors
		Return ret
	End Function

	''' <summary>
	''' Produces an array of the diagonal entries in the input matrix. </summary>
	''' <param name="input">	input matrix
	''' @return		the entries on the diagonal of input </param>
	Friend Shared Function extractDiagonalEntries(input As Double()()) As Double()
		Dim out As Double() = New Double(input.Length - 1) {}
		For i As Integer = 0 To input.Length - 1
			out(i) = input(i)(i)
		Next
		Return out
	End Function

	''' <summary>
	''' Performs a QR factorization on the input matrix. </summary>
	''' <param name="input">	input matrix
	''' @return		{Q, R}, the QR factorization of input. </param>
	Friend Shared Function QRFactorize(input As Double()()) As Double()()()
		Dim out As Double()()() = New Double(1)()() {}
		Dim orthonorm As Double()() = gramSchmidt(input)
		out(0) = orthonorm
        'Q is the matrix of the orthonormal vectors formed by GS on input
        'ORIGINAL LINE: double[][] R = new double[orthonorm.Length][orthonorm.Length];
        Dim R As Double()() = MAT(Of Double)(orthonorm.Length, orthonorm.Length)
        For i As Integer = 0 To R.Length - 1
			For j As Integer = 0 To i
				R(i)(j) = dot(input(i), orthonorm(j))
			Next
		Next
		out(1) = R
		Return out
	End Function

	''' <summary>
	''' Converts the input list of vectors into an orthonormal list with the same span. </summary>
	''' <param name="input">	list of vectors
	''' @return		orthonormal list with the same span as input </param>
	Friend Shared Function gramSchmidt(input As Double()()) As Double()()
        'ORIGINAL LINE: double[][] @out = new double[input.Length][input[0].Length];
        Dim out As Double()() = MAT(Of Double)(input.Length, input(0).Length)
        For outPos As Integer = 0 To out.Length - 1
			Dim v As Double() = input(outPos)
			For j As Integer = outPos - 1 To 0 Step -1
				Dim [sub] As Double() = proj(v, out(j))
					'Subtract off non-orthogonal components
				v = subtract(v, [sub])
			Next
				'return an orthonormal list
			out(outPos) = normalize(v)
		Next
		Return out
	End Function

	''' <summary>
	''' Returns the Givens rotation matrix with parameters (i, j, th). </summary>
	''' <param name="size">	total number of rows/columns in the matrix </param>
	''' <param name="i">		the first axis of the plane of rotation; i > j </param>
	''' <param name="j">		the second axis of the plane of rotation; i > j </param>
	''' <param name="th">	the angle of the rotation
	''' @return		the Givens rotation matrix G(i,j,th) </param>
	Friend Shared Function GivensRotation(size As Integer, i As Integer, j As Integer, th As Double) As Double()()
        'ORIGINAL LINE: double[][] @out = new double[size][size];
        Dim out As Double()() = MAT(Of Double)(size, size)
        Dim sine As Double = Math.Sin(th)
		Dim cosine As Double = Math.Cos(th)
		For x As Integer = 0 To size - 1
			If x <> i AndAlso x <> j Then
				out(x)(x) = cosine
			Else
				out(x)(x) = 1
			End If
		Next
		out(i)(j) = -sine
		'ith column, jth row
		out(j)(i) = sine
		Return out
	End Function

    ''' <summary>
    ''' Returns the transpose of the input matrix. </summary>
    ''' <param name="matrix">	double[][] matrix of values
    ''' @return			the matrix transpose of matrix </param>
    Public Shared Function transpose(matrix As Double()()) As Double()()
        'ORIGINAL LINE: double[][] @out = new double[matrix[0].Length][matrix.Length];
        Dim out As Double()() = MAT(Of Double)(matrix(0).Length, matrix.Length)
        For i As Integer = 0 To out.Length - 1
            For j As Integer = 0 To out(0).Length - 1
                out(i)(j) = matrix(j)(i)
            Next
        Next
        Return out
    End Function

    ''' <summary>
    ''' Returns the sum of a and b. </summary>
    ''' <param name="a">	double[][] matrix of values </param>
    ''' <param name="b">	double[][] matrix of values
    ''' @return	the matrix sum a + b </param>
    Friend Shared Function add(a As Double()(), b As Double()()) As Double()()
		If a.Length <> b.Length OrElse a(0).Length <> b(0).Length Then
			Throw New MatrixException("Matrices not same size.")
		End If
        'ORIGINAL LINE: double[][] @out = new double[a.Length][a[0].Length];
        Dim out As Double()() = MAT(Of Double)(a.Length, a(0).Length)
        For i As Integer = 0 To out.Length - 1
			For j As Integer = 0 To out(0).Length - 1
				out(i)(j) = a(i)(j) + b(i)(j)
			Next
		Next
		Return out
	End Function

	''' <summary>
	''' Returns the difference of a and b. </summary>
	''' <param name="a">	double[][] matrix of values </param>
	''' <param name="b">	double[][] matrix of values
	''' @return	the matrix difference a - b </param>
	Friend Shared Function subtract(a As Double()(), b As Double()()) As Double()()
		If a.Length <> b.Length OrElse a(0).Length <> b(0).Length Then
			Throw New MatrixException("Matrices not same size.")
		End If
        'ORIGINAL LINE: double[][] @out = new double[a.Length][a[0].Length];
        Dim out As Double()() = MAT(Of Double)(a.Length, a(0).Length)
        For i As Integer = 0 To out.Length - 1
			For j As Integer = 0 To out(0).Length - 1
				out(i)(j) = a(i)(j) - b(i)(j)
			Next
		Next
		Return out
	End Function

	''' <summary>
	''' Returns the sum of a and b. </summary>
	''' <param name="a">	double[] vector of values </param>
	''' <param name="b">	double[] vector of values
	''' @return	the vector sum a + b </param>
	Friend Shared Function add(a As Double(), b As Double()) As Double()
		If a.Length <> b.Length Then
			Throw New MatrixException("Vectors are not same length.")
		End If
		Dim out As Double() = New Double(a.Length - 1) {}
		For i As Integer = 0 To out.Length - 1
			out(i) = a(i) + b(i)
		Next
		Return out
	End Function

	''' <summary>
	''' Returns the difference of a and b. </summary>
	''' <param name="a">	double[] vector of values </param>
	''' <param name="b">	double[] vector of values
	''' @return	the vector difference a - b </param>
	Friend Shared Function subtract(a As Double(), b As Double()) As Double()
		If a.Length <> b.Length Then
			Throw New MatrixException("Vectors are not same length.")
		End If
		Dim out As Double() = New Double(a.Length - 1) {}
		For i As Integer = 0 To out.Length - 1
			out(i) = a(i) - b(i)
		Next
		Return out
	End Function

	''' <summary>
	''' Returns the matrix product of a and b; if the horizontal length of a is not equal to the
	''' vertical length of b, throws an exception. </summary>
	''' <param name="a">	double[][] matrix of values </param>
	''' <param name="b">	double[][] matrix of values
	''' @return	the matrix product ab </param>
	Friend Shared Function multiply(a As Double()(), b As Double()()) As Double()()
		If a.Length <> b(0).Length Then
			Throw New MatrixException("Matrices not compatible for multiplication.")
		End If
        'ORIGINAL LINE: double[][] @out = new double[b.Length][a[0].Length];
        Dim out As Double()() = MAT(Of Double)(b.Length, a(0).Length)
        For i As Integer = 0 To out.Length - 1
			For j As Integer = 0 To out(0).Length - 1
				Dim row As Double() = getRow(a, j)
				Dim column As Double() = getColumn(b, i)
				out(i)(j) = dot(row, column)
			Next
		Next
		Return out
	End Function

	''' <summary>
	''' Returns a version of mat scaled by a constant. </summary>
	''' <param name="mat">	input matrix </param>
	''' <param name="coeff">	constant by which to scale
	''' @return		mat scaled by coeff </param>
	Friend Shared Function scale(mat As Double()(), coeff As Double) As Double()()
        'ORIGINAL LINE: double[][] @out = new double[mat.Length][mat[0].Length];
        Dim out As Double()() = Microsoft.VisualBasic.MAT(Of Double)(mat.Length, mat(0).Length)
        For i As Integer = 0 To out.Length - 1
			For j As Integer = 0 To out(0).Length - 1
				out(i)(j) = mat(i)(j) * coeff
			Next
		Next
		Return out
	End Function

	''' <summary>
	''' Takes the dot product of two vectors, {a[0]b[0], ..., a[n]b[n]}. </summary>
	''' <param name="a">	double[] of values </param>
	''' <param name="b">	double[] of values
	''' @return	the dot product of a with b </param>
	Friend Shared Function dot(a As Double(), b As Double()) As Double
		If a.Length <> b.Length Then
			Throw New MatrixException("Vector lengths not equal: " & a.Length & "=/=" & b.Length)
		End If
		Dim sum As Double = 0
		For i As Integer = 0 To a.Length - 1
			numMults += 1
			sum += a(i) * b(i)
		Next
		Return sum
	End Function

	''' <summary>
	''' Returns a copy of the input matrix. </summary>
	''' <param name="input">	double[][] to be copied </param>
	Friend Shared Function copy(input As Double()()) As Double()()
        'ORIGINAL LINE: double[][] copy = new double[input.Length][input[0].Length];
        Dim copyValue As Double()() = MAT(Of Double)(input.Length, input(0).Length)
        For i As Integer = 0 To copyValue.Length - 1
            For j As Integer = 0 To copyValue(i).Length - 1
                copyValue(i)(j) = input(i)(j)
            Next
        Next
        Return copyValue
    End Function

	''' <summary>
	''' Returns the ith column of the input matrix.
	''' </summary>
	Private Shared Function getColumn(matrix As Double()(), i As Integer) As Double()
		Return matrix(i)
	End Function

	''' <summary>
	''' Returns the ith row of the input matrix.
	''' </summary>
	Private Shared Function getRow(matrix As Double()(), i As Integer) As Double()
		Dim vals As Double() = New Double(matrix.Length - 1) {}
		For j As Integer = 0 To vals.Length - 1
			vals(j) = matrix(j)(i)
		Next
		Return vals
	End Function

    ''' <summary>
    ''' Returns the projection of vec onto the subspace spanned by proj </summary>
    ''' <param name="vec">	vector to be projected </param>
    ''' <param name="proj">	spanning vector of the target subspace
    ''' @return		proj_proj(vec) </param>
    Friend Shared Function proj(vec As Double(), projValue As Double()) As Double()
        Dim constant As Double = dot(projValue, vec) / dot(projValue, projValue)
        Dim projection As Double() = New Double(vec.Length - 1) {}
        For i As Integer = 0 To projValue.Length - 1
            projection(i) = projValue(i) * constant
        Next
        Return projection
    End Function

    ''' <summary>
    ''' Returns a normalized version of the input vector, i.e. vec scaled such that ||vec|| = 1.
    ''' @return	vec/||vec||
    ''' </summary>
    Friend Shared Function normalize(vec As Double()) As Double()
		Dim newVec As Double() = New Double(vec.Length - 1) {}
        Dim normValue As Double = norm(vec)
        For i As Integer = 0 To vec.Length - 1
            newVec(i) = vec(i) / normValue
        Next
		Return newVec
	End Function

	''' <summary>
	''' Computes the norm of the input vector </summary>
	''' <returns> ||vec|| </returns>
	Friend Shared Function norm(vec As Double()) As Double
		Return Math.Sqrt(dot(vec, vec))
	End Function

    ''' <summary>
    ''' Prints the input matrix with each value rounded to 4 significant figures
    ''' </summary>
    Public Shared Sub print(matrix As Double()())
        For j As Integer = 0 To matrix(0).Length - 1
            For i As Integer = 0 To matrix.Length - 1
                'ORIGINAL LINE: double formattedValue = Double.parseDouble(String.format("%.4g%n", matrix[i][j]));
                Dim formattedValue As Double = Convert.ToDouble(matrix(i)(j).ToString("F4"))
                If Math.Abs(formattedValue) < 0.00001 Then
                    'Hide negligible values
                    formattedValue = 0
                End If
                Console.Write(formattedValue & vbTab)
            Next
            Console.Write(vbLf)
        Next
        Console.WriteLine("")
    End Sub
End Class

''' <summary>
''' Exception class thrown when invalid matrix calculations are attempted
''' </summary>
Friend Class MatrixException
	Inherits Exception
	Friend Sub New([string] As String)
		MyBase.New([string])
	End Sub
End Class

''' <summary>
''' Data holder class that contains a set of eigenvalues and their corresponding eigenvectors.
''' @author	Kushal Ranjan
''' @version 051413
''' </summary>
Friend Class EigenSet
	Friend values As Double()
	Friend vectors As Double()()
End Class

