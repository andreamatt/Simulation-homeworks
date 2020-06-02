import resolve from 'rollup-plugin-node-resolve'

export default {
	input: 'dist/graphic_debugger.js',
	output: {
		format: 'umd',
		name: 'app',
		file: 'public/build.js',
	},
	plugins: [
		resolve({
			jsnext: true,
			main: true,
			module: true
		})
	],
}