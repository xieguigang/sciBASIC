[![npm package](https://nodei.co/npm/mary-markov.png?downloads=true&downloadRank=true&stars=true)](https://nodei.co/npm/request/)

[![npm version](https://img.shields.io/npm/v/mary-markov.svg?style=for-the-badge)](https://www.npmjs.com/package/mary-markov) [![](https://img.shields.io/npm/dt/mary-markov.svg?style=for-the-badge)](https://www.npmjs.com/package/mary-markov) ![](https://img.shields.io/bundlephobia/min/mary-markov.svg?style=for-the-badge)  [![](https://img.shields.io/github/followers/Mary62442.svg?label=Follow&style=for-the-badge)](https://github.com/Mary62442/)

[![DeepScan grade](https://deepscan.io/api/teams/3675/projects/5403/branches/41450/badge/grade.svg)](https://deepscan.io/dashboard#view=project&tid=3675&pid=5403&bid=41450)


# mary-markov

## An npm package to calculate probabilities from Markov Chains and Hidden Markov Models
  
	const maryMarkov = require('mary-markov');  

## Markov Chain

A simple Markov Chain requires states, transition probabilities and initial probabilities.

The states array must be an array of objects with the properties state and prob.

The prob property is an array representing the corresponding line of the matrix of the states' transition probabilities.

For example, given a series of states **S = { 'sunny', 'cloudy', 'rainy'}** the transition matrix would be: 

		| 0.4 0.4 0.2 |	
	A =	| 0.3 0.3 0.4 |
		| 0.2 0.5 0.3 |
(represents the transition probabilities between the weather sunny, cloudy and rainy)

We'd instantiate a series of states as such:  

    let states = [
	    {state: 'sunny', prob:[0.4, 0.4, 0.2]},
	    {state: 'cloudy', prob:[0.3, 0.3, 0.4]},
	    {state: 'rainy', prob:[0.2, 0.5, 0.3]}
    ];

The initial probabilities of the states will be a simple array.
Each element of the array has the same index of the corresponding state in the states array.

Therefore, in this example, 0.4 is the sunny probability, 0.3 is the cloudy probability, and the final 0.3 is the rainy probability.

	let init = [0.4, 0.3, 0.3];

To instantiate the Markov Chain we pass the states, and the initial probabilities as parameters of the MarkovChain function.

	let markovChain = maryMarkov.MarkovChain(states, init);

### Markov Chain sequence probability
To then calculate the probability of a state sequence we call the sequenceProb() function on the object just instantiated, and we pass a state sequence array.

	let stateSeq = ['sunny', 'rainy', 'sunny', 'sunny', 'cloudy'];
	let seqProbability = markovChain.sequenceProb(stateSeq); //0.002560000000000001

### Markov Chain properties
|Property | Description|
|------------ | -------------|
|states | Array of the names of the states|
|transMatrix | Array of arrays representing thetransition probabilities|
|initialProb | Array of initial probabilities|

Example:

    console.log(markovChain.transMatrix) // [ [ 0.4, 0.4, 0.2 ], [ 0.3, 0.3, 0.4 ], [ 0.2, 0.5, 0.3 ] ]

   
## Hidden Markov Model

A Hidden Markov Model requires hidden states, transition probabilities, observables, emission probabilities, and initial probabilities.

For example, given a series of states **S = { 'AT-rich', 'CG-rich'}** the transition matrix would look like this:

		| 0.95 0.05	|
	A =	|          	|
		| 0.1  0.9	|
(represents the transition probabilities between AT-rich and CG-rich segments in a DNA sequence)

In the program we'd instantiate a series of hidden states as such:

    let hiddenStates = [    
	    {state: 'AT-rich', prob: [0.95, 0.05]},    
	    {state: 'CG-rich', prob: [0.1, 0.9]}     
    ];

The hidden states array must be an array of objects with the properties state and prob.

The prob property is the array representing the corresponding line of the matrix of the hidden state.


The observables array is similar to the hiddenStates array.
Given a series of observables **O = { 'A', 'C', 'G', 'T' }** the emission probabilities would be represented in the matrix:

		| 0.4  0.1  0.1  0.4  |
	B =	|                     |
		| 0.05 0.45 0.45 0.05 |

(represents the emission probabilities of the observables A, C, G, T given the hidden states AT-rich and CG-rich)

In the program the observables would be instantiated as:

    let observables = [    
	    {obs: 'A', prob: [0.4, 0.05]},    
	    {obs: 'C', prob: [0.1, 0.45]},    
	    {obs: 'G', prob: [0.1, 0.45]},    
	    {obs: 'T', prob: [0.4, 0.05]}    
    ];

The initial probabilities of the hidden states will be a simple array.
Each element of the array has the same index of the corresponding hidden state in the hidden states array.

	let hiddenInit = [0.65, 0.35];

In this example, 0.65 is the AT-rich probability, and the final 0.35 is the CG-rich probability.


To instantiate the Hidden Markov Model we pass the states, the observables and the initial probabilities as parameters of the HMM function.

	let HMModel = maryMarkov.HMM(hiddenStates, observables, hiddenInit);

### Hidden Markov Model: Bayes Theorem

To calculate the probability of a specific hidden state given an observable we can call the bayesTheorem() function and pass two parameters: the observable and the hidden state of which we want to know the probability.  

	let observation = 'A';
	let hiddenState = 'AT-rich';
	let bayesResult = HMModel.bayesTheorem(observation, hiddenState); //0.9369369369369369

### Hidden Markov Model: Forward Algorithm and Backward Algorithm (Problem 1: Likelihood)

To find the probability of an observation sequence given a model we can use either the forwardAlgorithm() function or the backwardAlgorithm() function and pass the observable sequence as parameter.

The forwardAlgorithm() function returns an object with:
* alphas : an array of arrays representing every forward probability of each state at every step of the sequence from start to end
* alphaF : the final value of the Forward probability

The backwardAlgorithm() function returns an object with:
* betas : an array of arrays representing every forward probability of each state at every step of the sequence from end to start
* betaF : the final value of the Backward probability

So,

	let obSequence = ['T','C','G','G','A']; 

    let forwardProbability = HMModel.forwardAlgorithm(obSequence);
	console.log(forwardProbability.alphaF); // 0.0003171642187500001

	let backwardProbability = HMModel.backwardAlgorithm(obSequence);
	console.log(backwardProbability.betaF); // 0.0003171642187500001

  
### Hidden Markov Model: Viterbi Algorithm (Problem 2: Decoding)

To calculate the most likely sequence of hidden states given a specific sequence of observables we can call the viterbiAlgorithm() function and pass it the observable sequence.

    let obSequence = ['A', 'T', 'C', 'G', 'C', 'G', 'T', 'C', 'A', 'T', 'C', 'G', 'T', 'C', 'G', 'T', 'C', 'C', 'G']; 
    let viterbiResult = HMModel.viterbiAlgorithm(obSequence);

The viterbiAlgorithm() function returns an object with the following properties:

* stateSequence : the array of the hidden state sequence found through the backtracking step.

* trellisSequence : an array of the trellis values at each time t of the hidden states. 

* terminationProbability : is the probability of the entire state sequence up to point T + 1 having been produced given the observation and the HMM’s parameters. 

So, 

    console.log(viterbiResult.stateSequence) //[ 'AT-rich', 'AT-rich', 'CG-rich', 'CG-rich', 'CG-rich', ... ] 


### Hidden Markov Model: Baum-Welch Algorithm (Problem 3: Learning)

To adjust the model parameters (A,B,π) to maximize the probability of the observation sequence given the model λ, we use what is called the Baum-Welch algorithm, or forward-backward algorithm, a special case of the EM (Expectation-Maximization) algorithm.

The function in this package is called baumWelchAlgorithm() and requires an observation sequence as sole parameter.
This trains and adapts the current Hidden Markov Model and provides a new model λ' = (A',B',π').

	let obSequence = ['A', 'T', 'C', 'G', 'C', 'G', 'T', 'C', 'A', 'T', 'C', 'G', 'T', 'C', 'G', 'T', 'C', 'C', 'G']; 
	let maximizedModel = HMModel.baumWelchAlgorithm(obSequence);

The function returns an HMM object with the initialProb, transMatrix, emissionMatrix properties updated with the values found by the algorithm.

	console.log(maximizedModel.transMatrix);  //[ [ 0.748722257770877, 0.251277742229123 ], [ 0.08173322039272721, 0.9182667796072727 ] ]


### Hidden Markov Model properties

|Property | Description|
|------------ | -------------|
|states | Array of the names of the states|
|transMatrix | Array of arrays representing the transition probabilities|
|initialProb | Array of initial probabilities|
|observables | Array of the names of the observables|
|emissionMatrix | Array of arrays representing the emission probabilities|

Example:

    console.log(HMModel.transMatrix) // [ [ 0.95, 0.05 ], [ 0.1, 0.9 ] ]


