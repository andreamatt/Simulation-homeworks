import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot
from pandas import read_csv
from numpy.random import rand

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


Ntr = 10000
Nreal = 1
Pt = 5
Pn = 0.000032
k = 2

Thetas = np.arange(1, 320, 10)
probabilities = []
for theta in Thetas:
	failures = 0
	ros = -np.log(rand(Ntr, Nreal))  # average 1 => scale = 1 => default value
	for t in range(Ntr):
		xi = rand() * 20
		yi = rand() * 60
		xj = rand() * 20 + 60
		yj = rand() * 60
		dist = sqrt((xj - xi)**2 + (yj - yi)**2)

		gammas = ros[t] * Pt * pow(dist, -k) / Pn
		failures += np.count_nonzero(gammas < theta)
	prob = failures / (Ntr * Nreal)
	probabilities.append(prob)

probabilities = np.array(probabilities)

plt.scatter(Thetas, probabilities, s=0.5, c='r')

Ntr = 1
Nreal = 10000

probabilities = []
for theta in Thetas:
	failures = 0
	ros = -np.log(rand(Ntr, Nreal))  # average 1 => scale = 1 => default value
	for t in range(Ntr):
		xi = rand() * 20
		yi = rand() * 60
		xj = rand() * 20 + 60
		yj = rand() * 60
		dist = sqrt((xj - xi)**2 + (yj - yi)**2)

		gammas = ros[t] * Pt * pow(dist, -k) / Pn
		failures += np.count_nonzero(gammas < theta)
	prob = failures / (Ntr * Nreal)
	probabilities.append(prob)

probabilities = np.array(probabilities)

plt.scatter(Thetas, probabilities, s=0.5, c='k')

Nreal = 10000

probabilities = []
for theta in Thetas:
	failures = 0
	ros = -np.log(rand(Nreal))  # average 1 => scale = 1 => default value
	for ro in ros:
		xi = rand() * 20
		yi = rand() * 60
		xj = rand() * 20 + 60
		yj = rand() * 60
		dist = sqrt((xj - xi)**2 + (yj - yi)**2)
		gamma = ro * Pt * pow(dist, -k) / Pn
		if gamma < theta:
			failures += 1
	prob = failures / (Ntr * Nreal)
	probabilities.append(prob)

probabilities = np.array(probabilities)

plt.scatter(Thetas, probabilities, s=0.5, c='b')
# plt.show()


Ntest = 1000

probabilities = []
for theta in Thetas:
	ros = -np.log(rand(Ntest))  # average 1 => scale = 1 => default value
	xis = rand(Ntest) * 20
	yis = rand(Ntest) * 60
	xjs = rand(Ntest) * 20 + 60
	yjs = rand(Ntest) * 60
	dists = np.sqrt((xjs - xis)**2 + (yjs - yis)**2)
	gammas = ros * Pt * np.power(dists, -k) / Pn

	failures = np.where(gammas < theta, 1, 0)
	interval = mean_confidence_asymptotic(failures, 0.95)
	plt.plot([theta, theta], interval, 'y-', linewidth=0.3)
	prob = mean(failures)
	probabilities.append(prob)

probabilities = np.array(probabilities)

plt.scatter(Thetas, probabilities, s=0.5, c='y')
plt.show()
