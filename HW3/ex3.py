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


confidence = 0.95
X = np.arange(0, 1, 1 / 100)

R = 2  # stages
N = 2
Ntr = 2000
probs_2_2 = []
probs_2_2_CI = []
plot_1_Y = []
plot_1_Y_CI = []

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

		prob = pow(p, number_successes_prev)  # prob failure
		if np.random.rand() >= prob:
			successes[t] = 1

	D_failure_prob = 1 - mean(successes)
	probs_2_2.append(D_failure_prob)
	probs_2_2_CI.append(mean_confidence_asymptotic(-1 * successes + 1, confidence))  # use failures, not successes

	plot_1_Y.append(np.mean(successful_nodes_per_stage, axis=0))
	plot_1_Y_CI.append(mean_confidence_asymptotic(successful_nodes_per_stage[:, 0], confidence))
	plot_1_Y_CI.append(mean_confidence_asymptotic(successful_nodes_per_stage[:, 1], confidence))

R = 5  # stages
N = 10
Ntr = 200
probs_5_10 = []
probs_5_10_CI = []
plot_2_Y = []
plot_2_Y_CI = []
for i in range(len(X)):
	p = X[i]
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
	probs_5_10_CI.append(mean_confidence_asymptotic(-1 * successes + 1, confidence))  # use failures, not successes

	plot_2_Y.append(np.mean(successful_nodes_per_stage, axis=0))
	plot_2_Y_CI.append(mean_confidence_asymptotic(successful_nodes_per_stage[:, 0], confidence))
	plot_2_Y_CI.append(mean_confidence_asymptotic(successful_nodes_per_stage[:, 1], confidence))
	plot_2_Y_CI.append(mean_confidence_asymptotic(successful_nodes_per_stage[:, 2], confidence))
	plot_2_Y_CI.append(mean_confidence_asymptotic(successful_nodes_per_stage[:, 3], confidence))
	plot_2_Y_CI.append(mean_confidence_asymptotic(successful_nodes_per_stage[:, 4], confidence))

## PLOTTING prob of end given link p
plt.scatter(X, probs_2_2, s=2, c='r')
plt.scatter(X, probs_5_10, s=2, c='g')

for i in range(len(X)):
	x = X[i]
	CI = probs_2_2_CI[i]
	plt.plot([x, x], CI, 'r-', linewidth=1)
	CI = probs_5_10_CI[i]
	plt.plot([x, x], CI, 'g-', linewidth=1)

theor_X = values[:, 0]
theor_Y = values[:, 1]
plt.plot(theor_X, theor_Y, 'k-', linewidth=1)
theor_Y = values[:, 2]
plt.plot(theor_X, theor_Y, 'b-', linewidth=1)

plt.show()

## PLOTTING mean messages at stages given link p for case 2,2
for i in range(len(X)):
	x = X[i]
	plt.scatter([x] * 2, plot_1_Y[i], c=['r', 'b'], s=2)
	plt.plot([x] * 2, plot_1_Y_CI[2 * i], 'r-', linewidth=1)
	plt.plot([x] * 2, plot_1_Y_CI[2 * i + 1], 'b-', linewidth=1)
plt.show()

## PLOTTING mean messages at stages given link p for case 10,5
for i in range(len(X)):
	x = X[i]
	plt.scatter([x] * 5, plot_2_Y[i], c=['r', 'b', 'k', 'g', 'm'], s=2)
	plt.plot([x] * 2, plot_2_Y_CI[5 * i], 'r-', linewidth=1)
	plt.plot([x] * 2, plot_2_Y_CI[5 * i + 1], 'b-', linewidth=1)
	plt.plot([x] * 2, plot_2_Y_CI[5 * i + 2], 'k-', linewidth=1)
	plt.plot([x] * 2, plot_2_Y_CI[5 * i + 3], 'g-', linewidth=1)
	plt.plot([x] * 2, plot_2_Y_CI[5 * i + 4], 'm-', linewidth=1)
plt.show()