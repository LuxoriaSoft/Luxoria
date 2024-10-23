module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'body-max-line-length': [0, 'always', 150], // Set limit to 150 (or any other value you prefer)
    'subject-case': [
      2, // Set to 2 for error (1 for warning)
      'always',
      [
        'lower-case', // Ensure the subject is in lowercase before the colon
        'sentence-case', // Allow first letter after the colon to be uppercase
      ],
    ],
  },
};
