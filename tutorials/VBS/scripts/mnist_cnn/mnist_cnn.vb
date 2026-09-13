#include "Microsoft.VisualBasic.MachineLearning.DataStorage.dll"

imports Microsoft.VisualBasic.MachineLearning.DataStorage

dim data_repo = ?"--mnist-data"
dim mnist as new MNIST(
    $"{data_repo}\train-images-idx3-ubyte", 
    $"{data_repo}\train-labels-idx1-ubyte")

    