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
          "0%": { opacity: "0", transform: "translateY(-10%)" },
          "100%": { opacity: "1", transform: "translateY(0);" },
        },
        rotateLong:{
          "0%": { transform: "rotate(0deg)" },
          "100%": { transform: "rotate(-17deg);" },
        }
      },
      animation: {
        fadeIn: "fadeIn 1s ease-in",
        rotateLong: "rotateLong 1.1s ease-in forwards",
      },
    },
  },
  plugins: [],
}

