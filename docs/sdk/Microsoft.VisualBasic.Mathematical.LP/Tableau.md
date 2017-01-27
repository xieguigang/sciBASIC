# Tableau
_namespace: [Microsoft.VisualBasic.Mathematical.LP](./index.md)_





### Methods

#### getColumn
```csharp
Microsoft.VisualBasic.Mathematical.LP.Tableau.getColumn(System.Int32)
```
Returns the nth column of the matrix *

#### getPivotRow
```csharp
Microsoft.VisualBasic.Mathematical.LP.Tableau.getPivotRow(System.Int32)
```
Finds the index of the pivot row (leaving basic variable) by applying
 the minimum ratio test. *

#### getRow
```csharp
Microsoft.VisualBasic.Mathematical.LP.Tableau.getRow(System.Int32)
```
Returns the nth row of the matrix *

#### inProperForm
```csharp
Microsoft.VisualBasic.Mathematical.LP.Tableau.inProperForm
```
Returns true if the tableau is in Proper Form, ie. it has
 exactly one basic variable per equation.


### Properties

#### Constraints
Returns a copy of constraint rows of the matrix *
#### Infeasible
Returns true if the problem is infeasible *
#### PivotColumn
Return the index of the left-most negative variable in the objective function *
#### TableauNotProper
Tableau not in proper form. Repeated basic variable for equation
#### Unbounded
Returns true if the model is unbounded *
