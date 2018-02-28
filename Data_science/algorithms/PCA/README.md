# Principal component analysis (PCA)

```vbnet
Dim data = DataSet.LoadDataSet("iris.csv", uidMap:="class")
Dim pca As New PCA(data.Matrix)

Call pca.ExplainedVariance.ToString.__DEBUG_ECHO

' [0.9246, 0.05302, 0.01719, 0.005183]

Dim newPoints = {
    {4.9, 3.2, 1.2, 0.4}.AsVector,
    {5.4, 3.3, 1.4, 0.9}.AsVector
}

For Each x In pca.Project(newPoints)
    Call x.ToString.__DEBUG_ECHO
Next

' [-2.831, 0.003402, -0.01107, -0.281]
' [-2.308, -0.3253, -0.06925, -0.6868]
```
