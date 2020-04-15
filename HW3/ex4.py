import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon, poisson, chi2
from math import sqrt, pow, floor, ceil, exp, log, sin, pi
from statsmodels.api import qqplot
from pandas import read_csv
from numpy.random import rand

Pt = 5
Pn = 0.000032
k = 2
Thetas = np.arange(1, 320, 1)

Ntr = 1000
Nreal = 50
probabilities = []
for theta in Thetas:
	failures = 0
	ros = expon.ppf(rand(Ntr, Nreal))  # average 1 => scale = 1 => default value
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

plt.scatter(Thetas, probabilities, s=2, c='g')
plt.show()

Ntr = 10000
Nreal = 1
probabilities = []
for theta in Thetas:
	failures = 0
	ros = expon.ppf(rand(Ntr, Nreal))  # average 1 => scale = 1 => default value
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

plt.scatter(Thetas, probabilities, s=2, c='r')
plt.show()

Ntr = 1000
Nreal = 1
probabilities = []
for theta in Thetas:
	failures = 0
	ros = expon.ppf(rand(Ntr, Nreal))  # average 1 => scale = 1 => default value
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

plt.scatter(Thetas, probabilities, s=2, c='k')
plt.show()

Nreal = 10000
probabilities = []
for theta in Thetas:
	failures = 0
	ros = expon.ppf(rand(Nreal))  # average 1 => scale = 1 => default value
	for ro in ros:
		xi = rand() * 20
		yi = rand() * 60
		xj = rand() * 20 + 60
		yj = rand() * 60
		dist = sqrt((xj - xi)**2 + (yj - yi)**2)
		gamma = ro * Pt * pow(dist, -k) / Pn
		if gamma < theta:
			failures += 1
	prob = failures / Nreal
	probabilities.append(prob)

probabilities = np.array(probabilities)

plt.scatter(Thetas, probabilities, s=2, c='b')
plt.show()

# TEST WITH INDEPENDENT DRAWS
Ntest = 100000
probabilities = []
for theta in Thetas:
	ros = expon.ppf(rand(Ntest))  # average 1 => scale = 1 => default value
	xis = rand(Ntest) * 20
	yis = rand(Ntest) * 60
	xjs = rand(Ntest) * 20 + 60
	yjs = rand(Ntest) * 60
	dists = np.sqrt((xjs - xis)**2 + (yjs - yis)**2)
	gammas = ros * Pt * np.power(dists, -k) / Pn

	failures = np.where(gammas < theta, 1, 0)
	prob = mean(failures)
	probabilities.append(prob)

probabilities = np.array(probabilities)

plt.scatter(Thetas, probabilities, s=2, c='m')
# plt.show()

# TEST WITH INDEPENDENT DRAWS, change area
probabilities = []
for theta in Thetas:
	ros = expon.ppf(rand(Ntest))  # average 1 => scale = 1 => default value
	xis = rand(Ntest) * 20
	yis = rand(Ntest) * 60
	xjs = rand(Ntest) * 20 + 100
	yjs = rand(Ntest) * 60
	dists = np.sqrt((xjs - xis)**2 + (yjs - yis)**2)
	gammas = ros * Pt * np.power(dists, -k) / Pn

	failures = np.where(gammas < theta, 1, 0)
	prob = mean(failures)
	probabilities.append(prob)

probabilities = np.array(probabilities)

plt.scatter(Thetas, probabilities, s=2, c='g')
# plt.show()

# TEST WITH INDEPENDENT DRAWS, change area
probabilities = []
for theta in Thetas:
	ros = expon.ppf(rand(Ntest))  # average 1 => scale = 1 => default value
	xis = rand(Ntest) * 20
	yis = rand(Ntest) * 60
	xjs = rand(Ntest) * 20 + 20
	yjs = rand(Ntest) * 60
	dists = np.sqrt((xjs - xis)**2 + (yjs - yis)**2)
	gammas = ros * Pt * np.power(dists, -k) / Pn

	failures = np.where(gammas < theta, 1, 0)
	prob = mean(failures)
	probabilities.append(prob)

probabilities = np.array(probabilities)

plt.scatter(Thetas, probabilities, s=2, c='r')
plt.show()
