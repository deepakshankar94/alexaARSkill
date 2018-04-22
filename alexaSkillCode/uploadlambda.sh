zip -r alexaSkillCode.zip node_modules index.js package.json 
aws lambda update-function-code --function-name wizardChessFunctions --zip-file fileb://./alexaSkillCode.zip