# LUDecomposition
_namespace: [Microsoft.VisualBasic.Mathematical.Matrix](./index.md)_

LU Decomposition.
 For an m-by-n matrix A with m >= n, the LU decomposition is an m-by-n
 unit lower triangular matrix L, an n-by-n upper triangular matrix U,
 and a permutation vector piv of length m so that A(piv,:) = L*U.
 ' If m < n, then L is m-by-m and U is m-by-n. '
 The LU decompostion with pivoting always exists, even if the matrix is
 singular, so the constructor will never fail. The primary use of the
 LU decomposition is in the solution of square systems of simultaneous
 linear equations. This will fail if IsNonSingular() returns false.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.LUDecomposition.#ctor(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
LU Decomposition, returns Structure to access L, U and piv.

|Parameter Name|Remarks|
|--------------|-------|
|A|  Rectangular matrix
 |


#### Determinant
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.LUDecomposition.Determinant
```
Determinant

_returns:      det(A)
 _

#### Solve
```csharp
Microsoft.VisualBasic.Mathematical.Matrix.LUDecomposition.Solve(Microsoft.VisualBasic.Mathematical.Matrix.GeneralMatrix)
```
Solve A*X = B

|Parameter Name|Remarks|
|--------------|-------|
|B|  A Matrix with as many rows as A and any number of columns.
 |


_returns:      X so that L*U*X = B(piv,:)
 _


### Properties

#### DoublePivot
Return pivot permutation vector as a one-dimensional double array
#### IsNonSingular
Is the matrix nonsingular?
#### L
Return lower triangular factor
#### LU
Array for internal storage of decomposition.
 @serial internal array storage.
#### m
Row and column dimensions, and pivot sign.
 @serial column dimension.
 @serial row dimension.
 @serial pivot sign.
#### n
Row and column dimensions, and pivot sign.
 @serial column dimension.
 @serial row dimension.
 @serial pivot sign.
#### piv
Internal storage of pivot vector.
 @serial pivot vector.
#### Pivot
Return pivot permutation vector
#### pivsign
Row and column dimensions, and pivot sign.
 @serial column dimension.
 @serial row dimension.
 @serial pivot sign.
#### U
Return upper triangular factor
