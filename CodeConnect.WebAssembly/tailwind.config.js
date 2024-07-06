/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./**/*.{razor,html}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors:{
        light: {
          primaryColor: "#F8F9FA",
          secondaryColor: "#5C42A7",
          supportColor: "#333333",
        },
        dark: {
          primaryColor: "#444444",
          secondaryColor: "#5C42A7",
          supportColor: "#E8E7E7",
        }
      }
    },
  },
  plugins: [],
}

