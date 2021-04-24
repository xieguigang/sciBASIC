const findSequence = (sequence, states) => {
    return sequence
    .reduce((all, curr) => {        
        all.push(states.findIndex(x => x.state === curr));        
        return all;        
    }, [])
};

const gamma = (alpha,beta,forward) => {
    return (alpha*beta)/forward;
};

const xi = (alpha, trans, emiss, beta, forward) => {
    return (alpha*trans*emiss*beta)/forward;
};

const calculateProb = (stateTrans2, init, states) => ({
    sequenceProb : (sequence) => {
        return findSequence(sequence, states).reduce((total, curr, i, arr) => {   
            if (i === 0) total += init[curr];    
            else  total *= stateTrans2[arr[i-1]][curr];         
            return total;    
        },0);
    }    
});

const forwardFactory = (hmm,obSequence) => ({
    initForward : () => {
        let initTrellis = [];
        let obIndex = hmm.observables.indexOf(obSequence[0]);
        let obEmission = hmm.emissionMatrix[obIndex];  
        hmm.initialProb.forEach((p,i) => {
            initTrellis.push(p*obEmission[i]);
        });
        return initTrellis;
    },
    recForward : function(prevTrellis, i, alphas) {   
        let obIndex = i;        
        if (obIndex === obSequence.length) return alphas;             
        let nextTrellis = [];
        for (let s = 0; s < hmm.states.length; s++) {
            let trellisArr = [];
            prevTrellis.forEach((prob, i) => {
                let trans = hmm.transMatrix[i][s];
                let emiss = hmm.emissionMatrix[hmm.observables.indexOf(obSequence[obIndex])][s];                
                trellisArr.push(prob*trans*emiss);
            });              
            nextTrellis.push(trellisArr.reduce((tot,curr) => tot+curr));
        };      
        alphas.push(nextTrellis);
        return this.recForward(nextTrellis, obIndex+1, alphas);
    },
    termForward : (alphas) => {
        return alphas[alphas.length-1].reduce((tot,val) => tot+val);
    }
});

const backwardFactory = (hmm,obSequence) => ({
    recBackward : function(prevBetas, i, betas) {   
        let obIndex = i;        
        if (obIndex === 0) return betas;             
        let nextTrellis = [];
        for (let s = 0; s < hmm.states.length; s++) {
            let trellisArr = [];
            prevBetas.forEach((prob, i) => {
                let trans = hmm.transMatrix[s][i];
                let emiss = hmm.emissionMatrix[hmm.observables.indexOf(obSequence[obIndex])][i];                
                trellisArr.push(prob*trans*emiss);
            });              
            nextTrellis.push(trellisArr.reduce((tot,curr) => tot+curr));
        };      
        betas.push(nextTrellis);
        return this.recBackward(nextTrellis, obIndex-1, betas);
    },

    termBackward : (betas) => {
        let finalBetas = betas[betas.length-1].reduce((tot,curr,i) => {
            let obIndex = hmm.observables.indexOf(obSequence[0]);
            let obEmission = hmm.emissionMatrix[obIndex];  
            tot.push(curr*hmm.initialProb[i]*obEmission[i]);
            return tot;
        },[]);
        return finalBetas.reduce((tot,val) => tot+val);
    }
});

const EM = (hmm, forwardObj, backwardBetas, obSequence) => ({
    initialGamma : (stateI) => {
        return gamma(forwardObj.alphas[0][stateI], backwardBetas[0][stateI], forwardObj.alphaF);
    },

    gammaTimesInState : (stateI) => {        
        let gammas = [];
        for ( let t = 0; t < obSequence.length; t++) {
            gammas.push(gamma(forwardObj.alphas[t][stateI], backwardBetas[t][stateI], forwardObj.alphaF));
        };
        return gammas.reduce((tot,curr) => tot+curr);
    },

    gammaTransFromState : (stateI) => {        
        let gammas = [];
        for ( let t = 0; t < (obSequence.length-1); t++) {
            gammas.push(gamma(forwardObj.alphas[t][stateI], backwardBetas[t][stateI], forwardObj.alphaF));
        };
        return gammas.reduce((tot,curr) => tot+curr);
    },

    xiTransFromTo : (stateI, stateJ) => {       
        let xis = [];
        for ( let t = 0; t < (obSequence.length-1); t++) {
            let alpha = forwardObj.alphas[t][stateI];
            let trans = hmm.transMatrix[stateI][stateJ];
            let emiss = hmm.emissionMatrix[hmm.observables.indexOf(obSequence[t+1])][stateJ];            
            let beta = backwardBetas[t+1][stateJ];
            xis.push(xi(alpha,trans,emiss,beta,forwardObj.alphaF));
        };
        return xis.reduce((tot,curr) => tot+curr);
    },

    gammaTimesInStateWithOb : (stateI, obIndex) => {
        let obsK = hmm.observables[obIndex];
        let stepsWithOb = obSequence.reduce((tot,curr,i)=> {
            if (curr === obsK) tot.push(i);
            return tot;
        },[]);        
        let gammas = [];
        stepsWithOb.forEach( step => {
            gammas.push(gamma(forwardObj.alphas[step][stateI], backwardBetas[step][stateI], forwardObj.alphaF));
        });  
        return gammas.reduce((tot,curr) => tot+curr);
    }
});

const viterbiFactory = (hmm, obSequence) => ({
	initViterbi : () => {
		let initTrellis = [];
        let obIndex = hmm.observables.indexOf(obSequence[0]);
        let obEmission = hmm.emissionMatrix[obIndex];  
        hmm.initialProb.forEach((p,i) => {
            initTrellis.push(p*obEmission[i]);
        });
        return initTrellis;
	},
	recViterbi : function(prevTrellis, obIndex, psiArrays, trellisSequence)  {
        if (obIndex === obSequence.length) return {psiArrays, trellisSequence};  
        let nextTrellis = hmm.states.map((state,stateIndex) => {
            let trellisArr = prevTrellis.map((prob,i) => {
                let trans = hmm.transMatrix[i][stateIndex];
                let emiss = hmm.emissionMatrix[hmm.observables.indexOf(obSequence[obIndex])][stateIndex];
                return prob*trans*emiss;
            })
            let maximized = Math.max(...trellisArr);   
            psiArrays[stateIndex].push(trellisArr.indexOf(maximized));   
            return maximized;
        }, []);
        trellisSequence.push(nextTrellis);
        return  this.recViterbi(nextTrellis, obIndex+1, psiArrays, trellisSequence);
    },
	termViterbi : (recTrellisPsi) => {
        let finalTrellis = recTrellisPsi.trellisSequence[recTrellisPsi.trellisSequence.length-1]
        let maximizedProbability = Math.max(...finalTrellis);
        recTrellisPsi.psiArrays.forEach(psiArr => {
            psiArr.push(finalTrellis.indexOf(maximizedProbability)); 
        });        
        return {maximizedProbability, psiArrays:recTrellisPsi.psiArrays};     
    },
	backViterbi : (psiArrays) => {
        let backtraceObj = obSequence.reduce(( acc, currS, i) => {  
            if (acc.length === 0) {                
                let finalPsiIndex = psiArrays[0].length-1;
                let finalPsi = psiArrays[0][finalPsiIndex];
                acc.push({psi:finalPsi, index:finalPsiIndex});
                return acc;
            }                
            let prevPsi = acc[acc.length-1];
            let psi = psiArrays[prevPsi.psi][prevPsi.index-1];
            acc.push({psi, index:prevPsi.index-1});
            return acc;
        },[])
        return backtraceObj.reverse().map(e => hmm.states[e.psi]);       
    }
});

const Bayes = (hmm) => ({
    bayesTheorem : (ob, hState) => {
        let hStateIndex = hmm.states.indexOf(hState);
        let obIndex = hmm.observables.indexOf(ob);
        let emissionProb = hmm.emissionMatrix[obIndex][hStateIndex];
        let initHState = hmm.initialProb[hStateIndex];
        let obProb = hmm.emissionMatrix[obIndex].reduce((total, em, i) => {
            total += (em*hmm.initialProb[i]);
            return total;
        }, 0);
        let bayesResult = (emissionProb*initHState)/obProb;
        return bayesResult;
    }
});

const Forward = (hmm) => ({
    forwardAlgorithm : (obSequence) => {
        let forward = forwardFactory(hmm, obSequence);
        let initAlphas = forward.initForward();
        let allAlphas = forward.recForward(initAlphas, 1, [initAlphas]);
        return {alphas: allAlphas, alphaF : forward.termForward(allAlphas)};
    }   
});

const Backward = (hmm) => ({
    backwardAlgorithm : (obSequence) => {
        let backward = backwardFactory(hmm, obSequence);
        let initBetas = hmm.states.map(s => 1);
        let allBetas = backward.recBackward(initBetas, obSequence.length-1, [initBetas]);
        return {betas: allBetas, betaF:backward.termBackward(allBetas)};
    }    
});

const Viterbi = (hmm) => ({
    viterbiAlgorithm : function(obSequence) {    	
        let viterbi = viterbiFactory(hmm, obSequence);
        let initTrellis = viterbi.initViterbi();
        let psiArrays = hmm.states.map(s => [null]); // Initialization of psi arrays is equal to 0, but I use null because 0 could later represent a state index
        let recTrellisPsi = viterbi.recViterbi(initTrellis, 1, psiArrays, [initTrellis]);
        let pTerm = viterbi.termViterbi(recTrellisPsi);
        let backtrace = viterbi.backViterbi(pTerm.psiArrays);       
        return {stateSequence:backtrace, trellisSequence:recTrellisPsi.trellisSequence, terminationProbability:pTerm.maximizedProbability};
    }    
});

const BaumWelch = (hmm) => ({
    baumWelchAlgorithm : (obSequence) => {
        let forwardObj = Forward(hmm).forwardAlgorithm(obSequence);
        let backwardBetas = Backward(hmm).backwardAlgorithm(obSequence).betas.reverse();
        let EMSteps = EM(hmm, forwardObj, backwardBetas, obSequence);
        let initProb = [];
        let transMatrix = [];
        let emissMatrix = [];
        for (let i = 0; i< hmm.states.length; i++) {
            initProb.push(EMSteps.initialGamma(i));
            let stateTrans = [];
            for (let j = 0; j< hmm.states.length; j++) {
                stateTrans.push(EMSteps.xiTransFromTo(i,j)/ EMSteps.gammaTransFromState(i));
            };
            transMatrix.push(stateTrans);
        };
        for (let o = 0; o < hmm.observables.length; o++) {
            let obsEmiss = [];
            for (let i = 0; i< hmm.states.length; i++) {
                obsEmiss.push(EMSteps.gammaTimesInStateWithOb(i,o)/ EMSteps.gammaTimesInState(i));
            };
            emissMatrix.push(obsEmiss);
        };
        let hiddenStates = transMatrix
        .reduce((tot,curr,i) => {
            let stateObj = {state: hmm.states[i], prob: curr}
            tot.push(stateObj);
            return tot;
        }, []);
        let observables = emissMatrix
        .reduce((tot,curr,i) => {
            let obsObj = {obs:hmm.observables[i], prob:curr};
            tot.push(obsObj);
            return tot;
        }, []);
        return HMM(hiddenStates, observables, initProb);
    }    
});

const MarkovChain = (states, init) => {
    let info = {  
        states: states.map(s => s.state),    
        transMatrix : states.map(s => s.prob),   
        initialProb : init  
    }     
    return Object.assign({}, info, calculateProb(info.transMatrix, init, states))
};

const HMM = (states, observables, init) => {
    let hmm = {  
        states: states.map(s => s.state),        
        transMatrix : states.map(s => s.prob),  
        initialProb : init,   
        observables : observables.map( o => o.obs ),
        emissionMatrix : observables.map(o => o.prob)     
    }     
    return Object.assign({}, hmm, Bayes(hmm), Viterbi(hmm), Forward(hmm), Backward(hmm), BaumWelch(hmm))
};


exports.MarkovChain = MarkovChain;
exports.HMM = HMM;

