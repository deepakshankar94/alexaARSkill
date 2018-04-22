'use strict';

var Alexa = require('alexa-sdk');



var handlers = {

  // Open skill
  'LaunchRequest': function() {
    if(Object.keys(this.attributes).length === 0) {
        this.attributes.userData = {
          isFirstTime : false,
          currentGameCode : ""
        }



        this.response
            .listen('Welcome to Wizard chess. Please download the wizard chess app from the playstore and share the code displayed. ',
            'Please share the code in the app?');

      } else {
        // var currentLanguage = this.attributes.flashcards.currentLanguage;
        // var numberCorrect = this.attributes.flashcards.languages[currentLanguage].numberCorrect;
        // var currentFlashcardIndex = this.attributes.flashcards.languages[currentLanguage].currentFlashcardIndex;

        this.response
            .listen('Welcome back to Wizard Chess. Would you like to start a new game or continue the previos game.');

      }
      this.response.cardRenderer("Wizard chess app link","link to the app")
    this.emit(':responseReady');
  },

  'SetMyLanguageIntent': function() {
    this.attributes.flashcards.currentLanguage = this.event.request.intent.slots.languages.value;
    var currentLanguage = this.attributes.flashcards.currentLanguage

    this.response
        .speak('Okay, I will ask you some questions about ' +
        currentLanguage + '. Here is your first question.' + 
                this.AskQuestion).listen(this.AskQuestion);
    this.emit(':responseReady');
  },

  // User gives an answer
  'AnswerIntent': function() {
    var currentLanguage = this.attributes.flashcards.currentLanguage;
    var currentFlashcardIndex = this.attributes.flashcards.languages[currentLanguage].currentFlashcardIndex;
    var userAnswer = this.event.request.intent.slots.answer.value;
    var languageAnswer = currentLanguage + 'Answer';
    var correctAnswer = flashcardsDictionary[currentFlashcardIndex][languageAnswer];

    if (userAnswer == correctAnswer){
        this.attributes.flashcards.languages[currentLanguage].numberCorrect++;
        var numberCorrect = this.attributes.flashcards.languages[currentLanguage].numberCorrect;
        this.response
          .speak('Nice job! The correct answer is ' + correctAnswer + '. You ' +
            'have gotten ' + numberCorrect + ' out of ' + DECK_LENGTH + ' ' +
            currentLanguage + ' questions correct. Here is your next question. ' + this.AskQuestion)
          .listen(this.AskQuestion);
    } else {
        var numberCorrect = this.attributes.flashcards.languages[currentLanguage].numberCorrect;
        this.response
          .speak('Sorry, the correct answer is ' + correctAnswer + '. You ' +
          'have gotten ' + numberCorrect + ' out of ' + DECK_LENGTH + ' ' +
          currentLanguage + ' questions correct. Here is your next question.' + 
                 this.AskQuestion).listen(this.AskQuestion);
    }

    this.attributes.flashcards.languages[currentLanguage].currentFlashcardIndex++;
    this.emit(':responseReady');
  },
  
   // Test my {language} knowledge
  'AskQuestion': function() {
    var currentLanguage = this.attributes.flashcards.currentLanguage;
    var currentFlashcardIndex = this.attributes.flashcards.languages[currentLanguage].currentFlashcardIndex;
    var currentQuestion = flashcardsDictionary[currentFlashcardIndex].question;

    this.response.listen('In ' + currentLanguage +', ' + currentQuestion);
    this.emit(':responseReady');
  },

  // Stop
  'AMAZON.StopIntent': function() {
      this.response.speak('Ok, let\'s play again soon.');
      this.emit(':responseReady');
  },

  // Cancel
  'AMAZON.CancelIntent': function() {
      this.response.speak('Ok, let\'s play again soon.');
      this.emit(':responseReady');
  },

  // Save state
  'SessionEndedRequest': function() {
    console.log('session ended!');
    this.emit(':saveState', true);
  }

};

exports.handler = function(event, context, callback){
    var alexa = Alexa.handler(event, context, callback);
    alexa.dynamoDBTableName = 'WizardChess';
    alexa.registerHandlers(handlers);
    alexa.execute();
};
