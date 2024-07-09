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
          secondaryColor: "#7b62cc",
          supportColor: "#E8E7E7",
        }
      },
      keyframes:{
        fadeIn:{
          "0%": { opacity: "0", transform: "translateX(-10%)" },
          "100%": { opacity: "1", transform: "translateX(0);" },
        }
      },
      animation: {
        fadeIn: "fadeIn 1s ease-in",
      },
    },
  },
  plugins: [],
}

