/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ['./**/*.{razor,html}'],
    safelist: [
        // Background colors
        'bg-gray-50',
        'bg-zinc-50',
        'bg-neutral-50',
        'bg-slate-50',
        'bg-sky-50',
        'bg-emerald-50',
        'bg-cyan-50',
        'bg-pink-50',
        'bg-rose-50',
        'bg-blue-50',
        'bg-indigo-50',
        'bg-violet-50',
        'bg-red-50',
        'bg-yellow-50',
        'bg-purple-50',
        'bg-fuchsia-50',
        'bg-orange-50',
        'bg-lime-50',
        'bg-amber-50',
        'bg-green-50',

        'bg-gray-100',
        'bg-zinc-100',
        'bg-neutral-100',
        'bg-slate-100',
        'bg-sky-100',
        'bg-emerald-100',
        'bg-cyan-100',
        'bg-pink-100',
        'bg-rose-100',
        'bg-blue-100',
        'bg-indigo-100',
        'bg-violet-100',
        'bg-red-100',
        'bg-white',
        'bg-purple-100',
        'bg-fuchsia-100',
        'bg-orange-100',
        'bg-lime-100',
        'bg-white',
        'bg-white',

        'bg-gray-200',
        'bg-zinc-200',
        'bg-neutral-200',
        'bg-slate-200',
        'bg-sky-200',
        'bg-emerald-200',
        'bg-cyan-200',
        'bg-pink-200',
        'bg-rose-200',
        'bg-blue-200',
        'bg-indigo-200',
        'bg-violet-200',
        'bg-red-200',
        'bg-yellow-200',
        'bg-purple-200',
        'bg-fuchsia-200',
        'bg-orange-200',
        'bg-lime-200',
        'bg-amber-200',
        'bg-green-200',

        'bg-gray-300',
        'bg-zinc-300',
        'bg-neutral-300',
        'bg-slate-300',
        'bg-sky-300',
        'bg-emerald-300',
        'bg-cyan-300',
        'bg-pink-300',
        'bg-rose-300',
        'bg-blue-300',
        'bg-indigo-300',
        'bg-violet-300',
        'bg-red-300',
        'bg-yellow-300',
        'bg-purple-300',
        'bg-fuchsia-300',
        'bg-orange-300',
        'bg-lime-300',
        'bg-amber-300',
        'bg-green-300',
        
        'bg-gray-400',
        'bg-zinc-400',
        'bg-neutral-400',
        'bg-slate-400',
        'bg-sky-400',
        'bg-emerald-400',
        'bg-cyan-400',
        'bg-pink-400',
        'bg-rose-400',
        'bg-blue-400',
        'bg-indigo-400',
        'bg-violet-400',
        'bg-red-400',
        'bg-yellow-400',
        'bg-purple-400',
        'bg-fuchsia-400',
        'bg-orange-400',
        'bg-lime-400',
        'bg-amber-400',
        'bg-green-400',

        'bg-gray-500',
        'bg-zinc-500',
        'bg-neutral-500',
        'bg-slate-500',
        'bg-sky-500',
        'bg-emerald-500',
        'bg-cyan-500',
        'bg-pink-500',
        'bg-rose-500',
        'bg-blue-500',
        'bg-indigo-500',
        'bg-violet-500',
        'bg-red-500',
        'bg-yellow-500',
        'bg-purple-500',
        'bg-fuchsia-500',
        'bg-orange-500',
        'bg-lime-500',
        'bg-amber-500',
        'bg-green-500',

        'bg-gray-600',
        'bg-zinc-600',
        'bg-neutral-600',
        'bg-slate-600',
        'bg-sky-600',
        'bg-emerald-600',
        'bg-cyan-600',
        'bg-pink-600',
        'bg-rose-600',
        'bg-stone-600',
        'bg-indigo-600',
        'bg-violet-600',
        'bg-red-600',
        'bg-yellow-600',
        'bg-purple-600',
        'bg-fuchsia-600',
        'bg-orange-600',
        'bg-lime-600',
        'bg-amber-600',
        'bg-lime-600',

        'bg-gray-700',
        'bg-zinc-700',
        'bg-neutral-700',
        'bg-slate-700',
        'bg-sky-700',
        'bg-emerald-700',
        'bg-cyan-700',
        'bg-pink-700',
        'bg-rose-700',
        'bg-blue-700',
        'bg-indigo-700',
        'bg-violet-700',
        'bg-red-700',
        'bg-yellow-700',
        'bg-purple-700',
        'bg-fuchsia-700',
        'bg-orange-700',
        'bg-lime-700',
        'bg-amber-700',
        'bg-green-700',

        // Ring colors
        'ring-gray-50',
        'ring-zinc-50',
        'ring-neutral-50',
        'ring-slate-50',
        'ring-sky-50',
        'ring-emerald-50',
        'ring-cyan-50',
        'ring-pink-50',
        'ring-rose-50',
        'ring-blue-50',
        'ring-indigo-50',
        'ring-violet-50',
        'ring-red-50',
        'ring-yellow-50',
        'ring-purple-50',
        'ring-fuchsia-50',
        'ring-orange-50',
        'ring-lime-50',
        'ring-amber-50',
        'ring-green-50',

        'ring-gray-100',
        'ring-zinc-100',
        'ring-neutral-100',
        'ring-slate-100',
        'ring-sky-100',
        'ring-emerald-100',
        'ring-cyan-100',
        'ring-pink-100',
        'ring-rose-100',
        'ring-blue-100',
        'ring-indigo-100',
        'ring-violet-100',
        'ring-red-100',
        'ring-white',
        'ring-purple-100',
        'ring-fuchsia-100',
        'ring-orange-100',
        'ring-lime-100',
        'ring-white',
        'ring-green-100',

        'ring-gray-200',
        'ring-zinc-200',
        'ring-neutral-200',
        'ring-slate-200',
        'ring-sky-200',
        'ring-emerald-200',
        'ring-cyan-200',
        'ring-pink-200',
        'ring-rose-200',
        'ring-blue-200',
        'ring-indigo-200',
        'ring-violet-200',
        'ring-red-200',
        'ring-yellow-200',
        'ring-purple-200',
        'ring-fuchsia-200',
        'ring-orange-200',
        'ring-lime-200',
        'ring-amber-200',
        'ring-green-200',

        'ring-gray-400',
        'ring-zinc-400',
        'ring-neutral-400',
        'ring-slate-400',
        'ring-sky-400',
        'ring-emerald-400',
        'ring-cyan-400',
        'ring-pink-400',
        'ring-rose-400',
        'ring-blue-400',
        'ring-indigo-400',
        'ring-violet-400',
        'ring-red-400',
        'ring-yellow-400',
        'ring-purple-400',
        'ring-fuchsia-400',
        'ring-orange-400',
        'ring-lime-400',
        'ring-amber-400',
        'ring-green-400',

        // Border colors
        'border-gray-50',
        'border-zinc-50',
        'border-neutral-50',
        'border-slate-50',
        'border-sky-50',
        'border-emerald-50',
        'border-cyan-50',
        'border-pink-50',
        'border-rose-50',
        'border-blue-50',
        'border-indigo-50',
        'border-violet-50',
        'border-red-50',
        'border-yellow-50',
        'border-purple-50',
        'border-fuchsia-50',
        'border-orange-50',
        'border-lime-50',
        'border-amber-50',
        'border-green-50',

        'border-gray-100',
        'border-zinc-100',
        'border-neutral-100',
        'border-slate-100',
        'border-sky-100',
        'border-emerald-100',
        'border-cyan-100',
        'border-pink-100',
        'border-rose-100',
        'border-blue-100',
        'border-indigo-100',
        'border-violet-100',
        'border-red-100',
        'border-white',
        'border-purple-100',
        'border-fuchsia-100',
        'border-orange-100',
        'border-lime-100',
        'border-white',
        'border-green-100',

        'border-gray-200',
        'border-zinc-200',
        'border-neutral-200',
        'border-slate-200',
        'border-sky-200',
        'border-emerald-200',
        'border-cyan-200',
        'border-pink-200',
        'border-rose-200',
        'border-blue-200',
        'border-indigo-200',
        'border-violet-200',
        'border-red-200',
        'border-yellow-200',
        'border-purple-200',
        'border-fuchsia-200',
        'border-orange-200',
        'border-lime-200',
        'border-amber-200',
        'border-green-200',

        'border-gray-300',
        'border-zinc-300',
        'border-neutral-300',
        'border-slate-300',
        'border-sky-300',
        'border-emerald-300',
        'border-cyan-300',
        'border-pink-300',
        'border-rose-300',
        'border-blue-300',
        'border-indigo-300',
        'border-violet-300',
        'border-red-300',
        'border-yellow-300',
        'border-purple-300',
        'border-fuchsia-300',
        'border-orange-300',
        'border-lime-300',
        'border-amber-300',
        'border-green-300',

        'border-gray-400',
        'border-zinc-400',
        'border-neutral-400',
        'border-slate-400',
        'border-sky-400',
        'border-emerald-400',
        'border-cyan-400',
        'border-pink-400',
        'border-rose-400',
        'border-blue-400',
        'border-indigo-400',
        'border-violet-400',
        'border-red-400',
        'border-yellow-400',
        'border-purple-400',
        'border-fuchsia-400',
        'border-orange-400',
        'border-lime-400',
        'border-amber-400',
        'border-green-400',

        'border-gray-500',
        'border-zinc-500',
        'border-neutral-500',
        'border-slate-500',
        'border-sky-500',
        'border-emerald-500',
        'border-cyan-500',
        'border-pink-500',
        'border-rose-500',
        'border-blue-500',
        'border-indigo-500',
        'border-violet-500',
        'border-red-500',
        'border-yellow-500',
        'border-purple-500',
        'border-fuchsia-500',
        'border-orange-500',
        'border-lime-500',
        'border-amber-500',
        'border-green-500',

        'hidden'
    ],
    theme: {
        extend: {
            width: {
                102: '25.5rem', // 408px
                106: '26rem', // 426px
                112: '28rem', // 448px
                120: '30rem', // 480px
                128: '32rem', // 512px
                136: '34rem', // 544px
                144: '36rem', // 576px
            },
            fontFamily: {
                sans: ['Roboto', 'ui-sans-serif', 'system-ui', 'sans-serif'],
                serif: ['Roboto Slab', 'ui-serif', 'Georgia', 'serif'],
                'roboto': ['Roboto', 'sans-serif'],
                'roboto-slab': ['Roboto Slab', 'serif'],
                'mono': ['JetBrains Mono', 'Fira Code', 'Menlo', 'Monaco', 'Consolas', 'Liberation Mono', 'monospace'],
            },
        }
    },
    variants: {
        extend: {
            borderColor: ['focus']
        }
    },
    plugins: [
    ]
}