from matplotlib import pyplot as plt, patches
from matplotlib.lines import Line2D
import numpy as np
from classes import *
from collections import OrderedDict

# GLOBAL SETTINGS
versions_to_plot = [[0,1,3],[1,2],[3,4]]
colors = ["#e41a1c","#377eb8","#ff7f00","#984ea3","#4daf4a","#ffff33","#a65628","#f781bf"]	# for protocol versions
line_styles = ["-", "--", "-.", ":"]	# for duties

def plot_delay_over_lambda_and_duty(runResults):
	duty_cycles = list(OrderedDict.fromkeys([stat.duty for stat in runResults.DLstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.DLstats]))
	N = runResults.DLstats[0].N
	shapes = list(OrderedDict.fromkeys([stat.shape for stat in runResults.DLstats]))
	
	fig, ax = plt.subplots(len(shapes), len(versions_to_plot), figsize=(14,9), sharex=True, sharey=True)
	fig.subplots_adjust(hspace= 0.05)
	fig.subplots_adjust(wspace= 0.03)

	# Add shared legend for duties
	legend_duties = []
	for j in range(len(duty_cycles)):
		duty = duty_cycles[j]
		legend_duties.append(Line2D([0], [0], color='black', linewidth=2, ls=line_styles[j], label=f'Duty_cycle = {duty}'))
	legend_1 = ax[0,len(versions_to_plot)-1].legend(handles=legend_duties, loc='lower right', bbox_to_anchor=(1, 1))
	
	for s in range(len(shapes)):
		for k in range(len(versions_to_plot)):
			legend_versions = []
			for i in range(len(versions_to_plot[k])):
				version = protocol_versions[versions_to_plot[k][i]]
				color = colors[versions_to_plot[k][i]]
				legend_versions.append(patches.Patch(color=color, label=version))
				for j in range(len(duty_cycles)):
					duty = duty_cycles[j]
					avg_delay = []
					lambdas = []

					stats = list(filter(lambda stat : stat.duty == duty and stat.version == version and stat.shape==shapes[s], runResults.DLstats))

					for stat in stats:
						avg_delay.append(np.mean(stat.delay))
						lambdas.append(stat.lam)

					ax[s,k].plot(lambdas, avg_delay,  marker=".", lw=1.25, color=color, ls=line_styles[j])
					
			#ax[s,k].set_xlim(0)
			#ax[s,k].set_ylim(0)
			ax[s,k].grid()
			ax[s,k].legend(handles=legend_versions)
			if s == len(shapes)-1:
				ax[s,k].set_xlabel('$\lambda$', fontsize=12)
		ax[s,0].set_ylabel("Delay"+ ",  Shape="+ shapes[s], fontsize=12)

	ax[0,len(versions_to_plot)-1].add_artist(legend_1)
	plt.suptitle('Average end-to-end packet delay\nN_density='+ str(N), fontsize=17)
	plt.savefig("plt_delay_over_lambda_and_duty.png", dpi=300, pad_inches = 0.05, bbox_inches = 'tight')
	plt.close()

def plot_delay_over_lambda_and_n(runResults):
	Ns = list(OrderedDict.fromkeys([stat.N for stat in runResults.LNstats]))
	protocol_versions = list(OrderedDict.fromkeys([stat.version for stat in runResults.LNstats]))
	d_cycle = runResults.LNstats[0].duty
	shapes = list(OrderedDict.fromkeys([stat.shape for stat in runResults.LNstats]))
	
	fig, ax = plt.subplots(len(shapes), len(versions_to_plot), figsize=(14,9), sharex=True, sharey=True)
	fig.subplots_adjust(hspace= 0.05)
	fig.subplots_adjust(wspace= 0.03)

	# Add shared legend for duties
	legend_Ns = []
	for j in range(len(Ns)):
		N = Ns[j]
		legend_Ns.append(Line2D([0], [0], color='black', linewidth=2, ls=line_styles[j], label=f'N_density = {N}'))
	legend_1 = ax[0,len(versions_to_plot)-1].legend(handles=legend_Ns, loc='lower right', bbox_to_anchor=(1, 1))
	
	for s in range(len(shapes)):
		for k in range(len(versions_to_plot)):
			legend_versions = []
			for i in range(len(versions_to_plot[k])):
				version = protocol_versions[versions_to_plot[k][i]]
				color = colors[versions_to_plot[k][i]]
				legend_versions.append(patches.Patch(color=color, label=version))
				for j in range(len(Ns)):
					N = Ns[j]
					avg_delay = []
					lambdas = []

					stats = list(filter(lambda stat : stat.N == N and stat.version == version and stat.shape==shapes[s], runResults.LNstats))

					for stat in stats:
						avg_delay.append(np.mean(stat.delay))
						lambdas.append(stat.lam)

					ax[s,k].plot(lambdas, avg_delay,  marker=".", lw=1.25, color=color, ls=line_styles[j])
					
			#ax[s,k].set_xlim(0)
			#ax[s,k].set_ylim(0)
			ax[s,k].grid()
			ax[s,k].legend(handles=legend_versions)
			if s == len(shapes)-1:
				ax[s,k].set_xlabel('$\lambda$', fontsize=12)
		ax[s,0].set_ylabel("Delay"+ ",  Shape="+ shapes[s], fontsize=12)

	ax[0,len(versions_to_plot)-1].add_artist(legend_1)
	plt.suptitle('Average end-to-end packet delay\nDuty_cycle='+ str(d_cycle), fontsize=17)
	plt.savefig("plt_delay_over_lambda_and_n.png", dpi=300, pad_inches = 0.05, bbox_inches = 'tight')
	plt.close()