# Exercise 2
# Load the data from the CSV file data_ex2.csv. These are samples from three different, independent Gaussian
# distributions, all mixed together.
# 1. Implement the Expectation-Maximization algorithm to fit a mixture of three Gaussian distributions to the data.
#    Try both with and without the prior update step. Discuss the results.
# 2. Give the parameters of the distributions thus found, and plot the corresponding PDFs on top of the
#    empirical PDFs of the data (e.g., the histogram).
# import and solve data
from pandas import read_csv
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon
from math import sqrt, pow, floor, ceil, exp


# define corrected functions for std and variance
def variance(values):
	return np.var(values, ddof=1)


def std(values):
	return np.std(values, ddof=1)


data = 'HW2/data'
data_ex2 = read_csv(f'{data}\\data_ex2.csv', header=None)
values = data_ex2.to_numpy()[:, 0]


def expectation_maximization_normal(values, curve_n, max_iter=10000, curve_prob=None, prior=True):
	n = len(values)
	# use the same std for all of them
	deviations = np.array([std(values)] * curve_n)
	# for 3 curves, use 1/4, 2/4, 3/4 quantiles as means
	means = np.array(quantile(values, np.arange(1, curve_n + 1, 1) / (curve_n + 1)))
	if curve_prob == None:
		curve_prob = [1 / curve_n] * curve_n
	curve_prob = np.array(curve_prob)

	values_repeated = np.array([values] * curve_n)

	iteration = 0
	prev_total = sum(means + deviations + curve_prob)
	while iteration < max_iter:
		pdfs_per_curve = norm.pdf(values_repeated.T, means, deviations)
		# print("pdfs per curve", pdfs_per_curve.shape)
		denoms = np.sum(pdfs_per_curve * curve_prob, axis=1)
		# print("denoms", denoms.shape)
		b = (pdfs_per_curve * curve_prob).T / denoms
		#print("b", b.shape)
		sum_b = np.sum(b, axis=1)
		# print("sum_b", sum_b.shape)
		means = np.sum(b * values, axis=1) / sum_b
		# print("new_m", means.shape, means)
		variances = np.sum(b.T * ((values_repeated.T - means)**2), axis=0) / sum_b
		deviations = np.sqrt(variances)
		# print("new_s", deviations.shape, deviations)
		if prior:
			curve_prob = sum_b / n

		iteration += 1
		new_total = sum(means + deviations + curve_prob)
		# stop when it's no longer changing
		if abs(new_total / prev_total - 1) <= 0.0001:
			break
		prev_total = new_total

	print(f"done {iteration} iterations")
	return np.array([means, deviations, curve_prob]).T


print("prior true")
curves = expectation_maximization_normal(values, 3, max_iter=10000, prior=True)
print(curves)
plt.hist(values, bins=50, density=True, color='y', linewidth=0.1, edgecolor='b')
X_prob = np.arange(min(values), max(values), 1 / 100)
for c in curves:
	plt.scatter(X_prob, c[2] * norm.pdf(X_prob, c[0], c[1]), s=1, zorder=2)
plt.show()

print("prior false")
curves = expectation_maximization_normal(values, 3, max_iter=10000, prior=False)
print(curves)
plt.hist(values, bins=50, density=True, color='y', linewidth=0.1, edgecolor='b')
X_prob = np.arange(min(values), max(values), 1 / 100)
for c in curves:
	plt.scatter(X_prob, c[2] * norm.pdf(X_prob, c[0], c[1]), s=1, zorder=2)
plt.show()
