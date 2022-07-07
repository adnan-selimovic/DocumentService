module.exports = {
  content: ["./src/**/*.{html,ts}"],
  presets: [
    require("./src/lib/app-core/configs/tailwind-config/init-preset.js"),
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};
