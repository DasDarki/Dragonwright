module.exports = {
  api: {
    input: {
      target: "http://localhost:5078/openapi/v1.json",
    },
    output: {
      mode: "single",
      client: "fetch",
      target: "src/api/index.ts",
      clean: false,
      override: {
        mutator: {
          path: "./src/api/http.ts",
          name: "customFetch",
        },
      },
    },
  },
};
