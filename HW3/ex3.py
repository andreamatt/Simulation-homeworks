import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot
from pandas import read_csv

data = 'HW3/data'
data_ex1 = read_csv(f'{data}\\theory_ex3.csv', header=None)
values = data_ex1.to_numpy()


def std(values):
	return np.std(values, ddof=1)


def mean_confidence_asymptotic(values, confidence):
	# when variables iid and high n
	n = len(values)
	z = norm.ppf((1 + confidence) / 2)
	s = std(values)
	m = mean(values)
	j = m - z * s / sqrt(n)
	k = m + z * s / sqrt(n)
	return (j, k)


# probability that node at stage k+1 doesnt receive = p^(nodes that received at stage k)

# p = 0.5 # failure prob
confidence = 0.95
X = np.arange(0, 1, 1 / 100)

figs, axs = plt.subplots(3)

R = 2 # stages
N = 2
Ntr = 2000
probs_2_2 = []
probs_2_2_CI = []
for p in X:
	successful_nodes_per_stage = np.zeros((Ntr, R))
	successes = np.zeros(Ntr)
	for t in range(Ntr):
		number_successes_prev = 1
		for s in range(0, R):
			prob = pow(p, number_successes_prev)
			results = np.random.choice([0, 1], size=N, p=[prob, 1 - prob])
			number_successes_prev = sum(results)
			successful_nodes_per_stage[t][s] = number_successes_prev

		prob = pow(p, number_successes_prev) # prob failure
		if np.random.rand() >= prob:
			successes[t] = 1

	D_failure_prob = 1 - mean(successes)
	probs_2_2.append(D_failure_prob)
	probs_2_2_CI.append(mean_confidence_asymptotic(-1 * successes + 1, confidence)) # use failures, not successes

	axs[1].scatter([p, p], np.mean(successful_nodes_per_stage, axis=0), s=1, c=['r', 'b'])
	axs[1].plot([p, p], mean_confidence_asymptotic( successful_nodes_per_stage[:,0] , confidence), 'r-', linewidth=0.2)
	axs[1].plot([p, p], mean_confidence_asymptotic( successful_nodes_per_stage[:,1] , confidence), 'b-', linewidth=0.2)

R = 5 # stages
N = 10
Ntr = 200
probs_5_10 = []
probs_5_10_CI = []
for p in X:
	successful_nodes_per_stage = np.zeros((Ntr, R))
	successes = np.zeros(Ntr)
	for t in range(Ntr):
		number_successes_prev = 1
		for s in range(0, R):
			prob = pow(p, number_successes_prev)
			results = np.random.choice([0, 1], size=N, p=[prob, 1 - prob])
			number_successes_prev = sum(results)
			successful_nodes_per_stage[t][s] = number_successes_prev

		prob = pow(p, number_successes_prev)
		if np.random.rand() > prob:
			successes[t] = 1

	D_failure_prob = 1 - mean(successes)
	probs_5_10.append(D_failure_prob)
	probs_5_10_CI.append(mean_confidence_asymptotic(-1 * successes + 1, confidence)) # use failures, not successes

	axs[2].scatter([p, p, p, p, p], np.mean(successful_nodes_per_stage, axis=0), s=1, c=['r', 'b', 'k', 'g', 'y'])
	axs[2].plot([p, p], mean_confidence_asymptotic( successful_nodes_per_stage[:,0] , confidence), 'r-', linewidth=0.2)
	axs[2].plot([p, p], mean_confidence_asymptotic( successful_nodes_per_stage[:,1] , confidence), 'b-', linewidth=0.2)
	axs[2].plot([p, p], mean_confidence_asymptotic( successful_nodes_per_stage[:,2] , confidence), 'k-', linewidth=0.2)
	axs[2].plot([p, p], mean_confidence_asymptotic( successful_nodes_per_stage[:,3] , confidence), 'g-', linewidth=0.2)
	axs[2].plot([p, p], mean_confidence_asymptotic( successful_nodes_per_stage[:,4] , confidence), 'y-', linewidth=0.2)

axs[0].scatter(X, probs_2_2, s=0.5, c='r')
axs[0].scatter(X, probs_5_10, s=0.5, c='r')

for i in range(len(X)):
	CI = probs_2_2_CI[i]
	axs[0].plot([X[i], X[i]], CI, 'k-', linewidth=0.2)
	CI = probs_5_10_CI[i]
	axs[0].plot([X[i], X[i]], CI, 'k-', linewidth=0.2)

X = values[:, 0]
Y = values[:, 1]
axs[0].scatter(X, Y, s=0.2, c='b')
Y = values[:, 2]
axs[0].scatter(X, Y, s=0.2, c='b')







plt.show()
