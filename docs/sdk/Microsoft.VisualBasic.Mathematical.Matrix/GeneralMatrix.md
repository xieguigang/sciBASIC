# GeneralMatrix
_namespace: [Microsoft.VisualBasic.Mathematical.Matrix](./index.md)_

.NET GeneralMatrix class.
 
 The .NET GeneralMatrix Class provides the fundamental operations of numerical
 linear algebra. Various constructors create Matrices from two dimensional
 arrays of double precision floating point numbers. Various "gets" and
 "sets" provide access to submatrices and matrix elements. Several methods 
 implement basic matrix arithmetic, including matrix addition and
 multiplication, matrix norms, and element-by-element array operations.
 Methods for reading and printing matrices are also included. All the
 operations in this version of the GeneralMatrix Class involve real matrices.
 Complex matrices may be handled in a future version.
 
 Five fundamental matrix decompositions, which consist of pairs or triples
 of matrices, permutation vectors, and the like, produce results in five
 decomposition classes. These decompositions are accessed by the GeneralMatrix
 class to compute solutions of simultaneous linear equations, determinants,
 inverses and other matrix functions. 
 
 The five decompositions are:
 
 + Cholesky Decomposition of symmetric, positive definite matrices.
 + LU Decomposition of rectangular matrices.
 + QR Decomposition of rectangular matrices.
 + Singular Value Decomposition of rectangular matrices.
 + Eigenvalue Decomposition of both symmetric and nonsymmetric square matrices.
 
 Example of use:
 
 Solve a linear system A x = b and compute the residual norm, ||b - A x||.
 
 ```csharp
 double[][] vals;
 GeneralMatrix A = new GeneralMatrix(vals);
 GeneralMatrix b = GeneralMatrix.Random(3,1);
 GeneralMatrix x = A.Solve(b);
 GeneralMatrix r = A.Multiply(x).Subtract(b);
 double rnorm = r.NormInf();
 ```



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.#ctor(System.Double[],System.Int32)
```
Construct a matrix from a one-dimensional packed array

|Parameter Name|Remarks|
|--------------|-------|
|vals|One-dimensional array of doubles, packed by columns (ala Fortran).
 |
|m|   Number of rows.
 |


#### Add
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Add(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
C = A + B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A + B
 _

#### AddEquals
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.AddEquals(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
A = A + B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A + B
 _

#### ArrayLeftDivide
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.ArrayLeftDivide(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Element-by-element left division, C = A.\B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A.\B
 _

#### ArrayLeftDivideEquals
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.ArrayLeftDivideEquals(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Element-by-element left division in place, A = A.\B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A.\B
 _

#### ArrayMultiply
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.ArrayMultiply(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Element-by-element multiplication, C = A.*B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A.*B
 _

#### ArrayMultiplyEquals
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.ArrayMultiplyEquals(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Element-by-element multiplication in place, A = A.*B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A.*B
 _

#### ArrayRightDivide
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.ArrayRightDivide(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Element-by-element right division, C = A./B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A./B
 _

#### ArrayRightDivideEquals
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.ArrayRightDivideEquals(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Element-by-element right division in place, A = A./B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A./B
 _

#### CheckMatrixDimensions
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.CheckMatrixDimensions(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Check if size(A) == size(B) *

#### chol
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.chol
```
Cholesky Decomposition

_returns:      CholeskyDecomposition
 _

#### Clone
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Clone
```
Clone the GeneralMatrix object.

#### Condition
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Condition
```
Matrix condition (2 norm)

_returns:      ratio of largest to smallest singular value.
 _

#### Copy
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Copy
```
Make a deep copy of a matrix

#### Create
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Create(System.Double[][])
```
Construct a matrix from a copy of a 2-D array.

|Parameter Name|Remarks|
|--------------|-------|
|A|   Two-dimensional array of doubles.
 |


#### Determinant
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Determinant
```
GeneralMatrix determinant

_returns:      determinant
 _

#### Dispose
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Dispose(System.Boolean)
```
Dispose(bool disposing) executes in two distinct scenarios.
 If disposing equals true, the method has been called directly
 or indirectly by a user's code. Managed and unmanaged resources
 can be disposed.
 If disposing equals false, the method has been called by the 
 runtime from inside the finalizer and you should not reference 
 other objects. Only unmanaged resources can be disposed.

|Parameter Name|Remarks|
|--------------|-------|
|disposing|-|


#### Eigen
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Eigen
```
Eigenvalue Decomposition

_returns:      EigenvalueDecomposition
 _

#### Finalize
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Finalize
```
This destructor will run only if the Dispose method 
 does not get called.
 It gives your base class the opportunity to finalize.
 Do not provide destructors in types derived from this class.

#### GetMatrix
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.GetMatrix(System.Int32[],System.Int32,System.Int32)
```
Get a submatrix.

|Parameter Name|Remarks|
|--------------|-------|
|r|   Array of row indices.
 |
|j0|  Initial column index
 |
|j1|  Final column index
 |


_returns:      A(r(:),j0:j1)
 _

#### Identity
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Identity(System.Int32,System.Int32)
```
Generate identity matrix

|Parameter Name|Remarks|
|--------------|-------|
|m|   Number of rows.
 |
|n|   Number of colums.
 |


_returns:      An m-by-n matrix with ones on the diagonal and zeros elsewhere.
 _

#### Inverse
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Inverse
```
Matrix inverse or pseudoinverse

_returns:      inverse(A) if A is square, pseudoinverse otherwise.
 _

#### ISerializable_GetObjectData
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.ISerializable_GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)
```
A method called when serializing this class

|Parameter Name|Remarks|
|--------------|-------|
|info|-|
|context|-|


#### LUD
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.LUD
```
LU Decomposition

_returns:      LUDecomposition
 _

#### Multiply
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Multiply(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Linear algebraic matrix multiplication, A * B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      Matrix product, A * B
 _

#### MultiplyEquals
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.MultiplyEquals(System.Double)
```
Multiply a matrix by a scalar in place, A = s*A

|Parameter Name|Remarks|
|--------------|-------|
|s|   scalar
 |


_returns:      replace A by s*A
 _

#### Norm1
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Norm1
```
One norm

_returns:     maximum column sum.
 _

#### Norm2
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Norm2
```
Two norm

_returns:     maximum singular value.
 _

#### NormF
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.NormF
```
Frobenius norm

_returns:     sqrt of sum of squares of all elements.
 _

#### NormInf
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.NormInf
```
Infinity norm

_returns:     maximum row sum.
 _

#### op_Addition
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.op_Addition(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix,Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Addition of matrices

|Parameter Name|Remarks|
|--------------|-------|
|m1|-|
|m2|-|


#### op_Multiply
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.op_Multiply(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix,Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Multiplication of matrices

|Parameter Name|Remarks|
|--------------|-------|
|m1|-|
|m2|-|


#### op_Subtraction
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.op_Subtraction(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix,Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Subtraction of matrices

|Parameter Name|Remarks|
|--------------|-------|
|m1|-|
|m2|-|


#### op_UnaryNegation
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.op_UnaryNegation(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Unary minus

_returns:     -A
 _

#### QRD
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.QRD
```
QR Decomposition

_returns:      QRDecomposition
 _

#### Rank
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Rank
```
GeneralMatrix rank

_returns:      effective numerical rank, obtained from SVD.
 _

#### SetElement
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.SetElement(System.Int32,System.Int32,System.Double)
```
Set a single element.

|Parameter Name|Remarks|
|--------------|-------|
|i|   Row index.
 |
|j|   Column index.
 |
|s|   A(i,j).
 |


#### SetMatrix
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.SetMatrix(System.Int32,System.Int32,System.Int32[],Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Set a submatrix.

|Parameter Name|Remarks|
|--------------|-------|
|i0|  Initial row index
 |
|i1|  Final row index
 |
|c|   Array of column indices.
 |
|X|   A(i0:i1,c(:))
 |


#### Solve
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Solve(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Solve A*X = B

|Parameter Name|Remarks|
|--------------|-------|
|B|   right hand side
 |


_returns:      solution if A is square, least squares solution otherwise
 _

#### SolveTranspose
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.SolveTranspose(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Solve X*A = B, which is also A'*X' = B'

|Parameter Name|Remarks|
|--------------|-------|
|B|   right hand side
 |


_returns:      solution if A is square, least squares solution otherwise.
 _

#### Subtract
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Subtract(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
C = A - B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A - B
 _

#### SubtractEquals
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.SubtractEquals(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
A = A - B

|Parameter Name|Remarks|
|--------------|-------|
|B|   another matrix
 |


_returns:      A - B
 _

#### SVD
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.SVD
```
Singular Value Decomposition

_returns:      SingularValueDecomposition
 _

#### Trace
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Trace
```
Matrix trace.

_returns:      sum of the diagonal elements.
 _

#### Transpose
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.Transpose
```
Matrix transpose.

_returns:     A'
 _

#### UnaryMinus
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix.UnaryMinus
```
Unary minus

_returns:     -A
 _


### Properties

#### A
Array for internal storage of elements.
 @serial internal array storage.
#### Array
Access the internal two-dimensional array.
#### ArrayCopy
Copy the internal two-dimensional array.
#### ColumnDimension
Get column dimension.
#### ColumnPackedCopy
Make a one-dimensional column packed copy of the internal array.
#### GetElement
Get a single element.
#### m
Row and column dimensions.
 @serial row dimension.
 @serial column dimension.
#### n
Row and column dimensions.
 @serial row dimension.
 @serial column dimension.
#### RowDimension
Get row dimension.
#### RowPackedCopy
Make a one-dimensional row packed copy of the internal array.
