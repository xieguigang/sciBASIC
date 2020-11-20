The following is a specific pseudocode implementation of differential evolution, written similar to the Java language. For more generalized pseudocode, please see the listing in the Algorithm section above.

```java
//definition of one individual in population
public class Individual {
	//normally DifferentialEvolution uses floating point variables
	float data1, data2
	//but using integers is possible too  
	int data3
}

public class DifferentialEvolution {
	//Variables
	//linked list that has our population inside
	LinkedList<Individual> population=new LinkedList<Individual>()
	//New instance of Random number generator
	Random random=new Random()
	int PopulationSize=20

	//differential weight [0,2]
	float F=1
	//crossover probability [0,1]
	float CR=0.5
	//dimensionality of problem, means how many variables problem has. this case 3 (data1,data2,data3)
	int N=3;

	//This function tells how well given individual performs at given problem.
	public float fitnessFunction(Individual in) {
		...
		return	fitness	
	}

	//this is main function of program
	public void Main() {
		//Initialize population with individuals that have been initialized with uniform random noise
		//uniform noise means random value inside your search space
		int i=0
		while(i<populationSize) {
			Individual individual= new Individual()
			individual.data1=random.UniformNoise()
			individual.data2=random.UniformNoise()
			//integers cant take floating point values and they need to be either rounded
			individual.data3=Math.Floor( random.UniformNoise())
			population.add(individual)
			i++
		}
		
		i=0
		int j
		//main loop of evolution.
		while (!StoppingCriteria) {
			i++
			j=0
			while (j<populationSize) {
				//calculate new candidate solution
			
				//pick random point from population
				int x=Math.floor(random.UniformNoise()%(population.size()-1))
				int a,b,c

				//pick three different random points from population
				do{
					a=Math.floor(random.UniformNoise()%(population.size()-1))
				}while(a==x);
				do{
					b=Math.floor(random.UniformNoise()%(population.size()-1))
				}while(b==x| b==a);
				do{
					c=Math.floor(random.UniformNoise()%(population.size()-1))
				}while(c==x | c==a | c==b);
				
				// Pick a random index [0-Dimensionality]
				int R=rand.nextInt()%N;
				
				//Compute the agent's new position
				Individual original=population.get(x)
				Individual candidate=original.clone()
				
				Individual individual1=population.get(a)
				Individual individual2=population.get(b)
				Individual individual3=population.get(c)
				
				//if(i==R | i<CR)
					//candidate=a+f*(b-c)
				//else
					//candidate=x
				if( 0==R | random.UniformNoise()%1<CR){	
					candidate.data1=individual1.data1+F*(individual2.data1-individual3.data1)
				}// else isn't needed because we cloned original to candidate
				if( 1==R | random.UniformNoise()%1<CR){	
					candidate.data2=individual1.data2+F*(individual2.data2-individual3.data2)
				}
				//integer work same as floating points but they need to be rounded
				if( 2==R | random.UniformNoise()%1<CR){	
					candidate.data3=Math.floor(individual1.data3+F*(individual2.data3-individual3.data3))
				}
				
				//see if is better than original, if so replace
				if(fitnessFunction(original)<fitnessFunction(candidate)){
					population.remove(original)
					population.add(candidate)
				}
				j++
			}
		}
		
		//find best candidate solution
		i=0
		Individual bestFitness=new Individual()
		while (i<populationSize) {
			Individual individual=population.get(i)
			if(fitnessFunction(bestFitness)<fitnessFunction(individual)){
				bestFitness=individual
			}
			i++
		}
		
		//your solution
		return bestFitness
	}
}
```
