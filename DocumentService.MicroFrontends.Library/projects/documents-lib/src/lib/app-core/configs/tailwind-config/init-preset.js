module.exports = {
  theme: {
    screens: {
      mob: "0px",
      sm: "640px",
      md: "768px",
      lg: "1024px",
      xl: "1280px",
      "2xl": "1536",
    },
    container: {
      center: true,
    },
    extend: {
      spacing: {
        1: "8px",
        2: "12px",
        3: "16px",
        4: "24px",
        5: "32px",
        6: "48px",
      },
      fontSize: {
        sm: ["14px", "20px"],
        base: ["16px", "24px"],
        lg: ["20px", "28px"],
        xl: ["24px", "32px"],
      },
      colors: {
        transparent: "transparent",
        current: "currentColor",
        white: "#ffffff",
        layout: "#F8F8F8",
        silver: "#F8F8F8",
        gray: {
          eagle: "#EAEAEA",
          belton: "#DCDCDC",
          smoky: "#CFCFCF",
        },
        blue: {
          light: "#d4e1ff",
          basic: "#3f42d4",
        },
      },
    },
  },
  plugins: [],
};
