# svmnet


This is an clean .NET conversion of libsvm 3.18, specifically from the Java version. Full functionality and efficiency is maintained, but the object structure has been modified to be more appropriate for the .NET platform (including C# and VB.NET). Using the library is quite straightforward. For example, if you have downloaded the sample data, then (using C#) you would train and test an SVM in the following manner:

```c
	// We'll use some simulated data

	Problem train = SVMUtilities.CreateTwoClassProblem(100);
	Problem test = SVMUtilities.CreateTwoClassProblem(100, false);
	
	
	// Scale the data
	
	RangeTransform transform = RangeTransform.Compute(train);
	train = transform.Scale(train);
	test = transform.Scale(test);


	//For this example (and indeed, many scenarios), the default
	//parameters will suffice.
	
	Parameter parameters = new Parameter();
	parameters.Gamma = 1.0 / train.MaxIndex; // this is the default setting for Gamma


	//Train the model using the optimal parameters.

	Model model = Training.Train(train, parameters);


	//Perform classification on the test data, putting the
	//results in results.txt.

	double score = Prediction.Predict(test, "results.txt", model, false);
```

Each class which needs to be saved has a Write() and Read() method that can be used for that purpose. Problem files are of the form:

	[label] [index]:[value] [index]:[value] ...
	...
	
Please refer to the [libsvm](http://www.csie.ntu.edu.tw/~cjlin/libsvm/) site for more tutorials or information on support vector machines.