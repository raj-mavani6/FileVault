/** @type {import('tailwindcss').Config} */
module.exports = {
  prefix: 'tw-',
  important: false,
  corePlugins: {
    preflight: false, // Don't conflict with Bootstrap reset
  },
  content: [
    '../Views/**/*.cshtml',
    './js/**/*.js',
  ],
  theme: {
    extend: {
      colors: {
        'vault-dark': '#151311',
        'vault-accent': '#4B262F',
        'vault-cream': '#EED3BA',
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
