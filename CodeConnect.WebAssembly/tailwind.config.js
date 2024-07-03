/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./**/*.{razor,html,cshtml}"],
  darkMode: 'class',
  theme: {
    extend: {
      colors:{
        light: {
          primaryBg: "#b5b5b5"
          
        },
        dark: {
          primaryBg: "#333333",
          primaryText: "#FFFFFF",
        }
      }
    },
  },
  plugins: [],
}

