"use strict";

var Alexa = require("alexa-sdk");
var firebase = require('firebase');
var Chess = require('chess.js').Chess;


var firebaseConfig = {
    apiKey: "AIzaSyCwiPjtol3775f03Xv1Wlx3jG9bYLiwXUU",
    authDomain: "wizardchess-a4c9e.firebaseapp.com",
    databaseURL: "https://wizardchess-a4c9e.firebaseio.com",
    projectId: "wizardchess-a4c9e",
    storageBucket: "wizardchess-a4c9e.appspot.com",
    messagingSenderId: "959865758259"
}

firebase.initializeApp(firebaseConfig);


function syncData(self,callback){
	var alexaCode = self.attributes.alexaCode;
  	firebase.database().ref("alexaColl/"+alexaCode).once('value').then(function(snapshot){
		var alexaData = snapshot.val();
		self.attributes.currentGameCode = alexaData.currentGameCode;
		firebase.database().ref("gameColl/"+alexaData.currentGameCode).once('value').then(function(snapshot){
			var gameData = snapshot.val();
			self.attributes.gameData = gameData;
			delete self.attributes.gameData.board;
			callback();
		});
	});
}



var handlers = {
  "LaunchRequest": function () {
    this.response.cardRenderer("Wizard chess app link","link to the app : https://drive.google.com/file/d/1ecJkKnmOge_wNTgry7Spw9pJoxJvQQca/view?usp=sharing")
    //get user code for the first time and pair with phone 
    //test code for new user flow 
    if(this.attributes.isSynced !== true) {
    		
		  	var prompt = "Welcome to Wizard chess. Please download the wizard chess app from the playstore and share the code displayed. Accio code."
		  	var reprompt = "please tell the code"
		  	var cardTitle = "Wizard chess app link";
		  	var cardContent = "link to the app : https://bit.ly/2HKfGW3\n App name on playstore: AR Wizard Chess"
		  	//this.response.speak(codeNumber+"_"+codeAnimal);
			this.emit(':askWithCard',prompt,reprompt,cardTitle,cardContent);
    }
    else{
		var prompt = "Welcome back to wizard chess. Please tell the code of the game you want to join or continue game. Connect a new device by saying \"connect new device\""
		  	var reprompt = "please tell the code"
		  	//this.response.speak(codeNumber+"_"+codeAnimal);
			this.emit(':ask',prompt,reprompt);
    }
 //    var self = this;
 //    firebase.database().ref("gameColl/354_lion").once('value').then(function(snapshot){
	// 	var data = snapshot.val();
		
	// });
    
  },
  "continueGame" : function () {
  	if(this.attributes.currentGameCode == undefined){
  		var prompt = "Welcome back to wizard chess. Please tell the code of the game you want to join or start a new game."
	  	var reprompt = "please tell the code"
	  	//this.response.speak(codeNumber+"_"+codeAnimal);
		this.emit(':ask',prompt,reprompt);
  	}
  	this.attributes.continueGame = true;
  	this.emit("codeReceived");
  },

  "codeReceived" : function () {
  	if(this.attributes.continueGame !== true){
  		var codeNumber = this.event.request.intent.slots.code_number.value;
  		var codeAnimal = this.event.request.intent.slots.code_animal.value;
  		var code = codeNumber + "_" + codeAnimal;
  	}
  	else{
  		var code = this.attributes.currentGameCode;
  		delete this.attributes.continueGame;
  	}
  	if(this.attributes.isSynced !== true) {
    		var self = this;
		    firebase.database().ref("alexaColl/"+code).once('value').then(function(snapshot){
				var data = snapshot.val();
				if(data == null){
					var prompt = "the code you entered is incorrect. Please download the wizard chess companion app from the playstore"
					var reprompt = "please tell the code"
				  	//this.response.speak(codeNumber+"_"+codeAnimal);
					self.emit(':ask',prompt,reprompt);
				}
				else{
					firebase.database().ref("alexaColl/"+code).update(
					{
						"alexaId" : true
					}).then(()=>{
						self.attributes.isSynced = true;
						self.attributes.alexaCode =  code;

						var prompt = "Alexa and App have been synced. Please create a new game or tell the code of game to join"
						var reprompt = "please tell the code"
			  			//this.response.speak(codeNumber+"_"+codeAnimal);
						self.emit(':ask',prompt,reprompt);
					})
		
				}

				
			});
		  	
    }
    else{

		var self = this;
	    firebase.database().ref("gameColl/"+code).once('value').then(function(snapshot){
			var gameData = snapshot.val();
			if(gameData == null){
				var prompt = "the code "+codeNumber+" "+codeAnimal+" you said is incorrect. Please Check the code and say it again"
				var reprompt = "please tell the code"
			  	//this.response.speak(codeNumber+"_"+codeAnimal);
				self.emit(':ask',prompt,reprompt);
			}
			else{
				if(gameData.isCompleted){
					var prompt = "This game is already completed. "+ gameData.winner + " won the game.";
					var reprompt = "please tell the code";
					self.emit(':tell',prompt);
					return;
				}

				var alexaCode = self.attributes.alexaCode;

				firebase.database().ref("alexaColl/"+alexaCode).update(
				{
					"currentGameCode" : code,
					"updatedAt": (+ new Date())
				}).then(()=>{
					self.attributes.currentGameCode = code;
					var chess;
					if(gameData.fen !== null){
						chess = new Chess(gameData.fen);
					}
					else{
						chess = new Chess();
					}
					//self.attributes.chessBoard = chess;
					var gameUpdateJson;
					if(gameData.isSameAlexa == true && (gameData.white == null || gameData.white == "")){
						gameUpdateJson = {
						"fen" : chess.fen(),
						"white": alexaCode,
						"black": alexaCode
						}
					}
					else if(gameData.isSameAlexa == false && (gameData.white == null || gameData.white == "")){
						gameUpdateJson = {
						"fen" : chess.fen(),
						"white": alexaCode
						}
					}
					else if(gameData.white && (gameData.black == null || gameData.black == "")){
						gameUpdateJson = {
						"fen" : chess.fen(),
						"black": alexaCode
						}
					}
					else{
						gameUpdateJson = {
						"fen" : chess.fen()
						}
					}
					firebase.database().ref("gameColl/"+code)
					.update(gameUpdateJson)
					.then(()=>{
						var prompt = "Game has been connected. Make your move."
						var reprompt = "please tell the move you want to make"
						self.emit(':ask',prompt,reprompt);
					});
				})
	
			}

			
		});
    }
	
  },
  "makeMove": function() {
  	var self = this;
  	syncData(this,()=>{
	  	if(this.attributes.gameData[this.attributes.gameData.currentTurn] !== this.attributes.alexaCode){
	  		var prompt = "It's not your move. Please wait for your turn";
			var reprompt = "please tell the new move"
			self.emit(':ask',prompt);
	  	}
	  	if(self.event.request.intent.slots.moveFromCode.resolutions.resolutionsPerAuthority && self.event.request.intent.slots.moveFromCode.resolutions.resolutionsPerAuthority[0] && self.event.request.intent.slots.moveFromCode.resolutions.resolutionsPerAuthority[0].values && self.event.request.intent.slots.moveFromCode.resolutions.resolutionsPerAuthority[0].values[0].value && self.event.request.intent.slots.moveFromCode.resolutions.resolutionsPerAuthority[0].values[0].value.name){
	  		var codeFrom = self.event.request.intent.slots.moveFromCode.resolutions.resolutionsPerAuthority[0].values[0].value.name || self.event.request.intent.slots.moveFromCode.value; // check if correct value with regex
	  	}
	  	else{
	    	var codeFrom =  self.event.request.intent.slots.moveFromCode.value; // check if correct value with regex
	  	}

	  	if(self.event.request.intent.slots.moveToCode.resolutions.resolutionsPerAuthority && self.event.request.intent.slots.moveToCode.resolutions.resolutionsPerAuthority[0] && self.event.request.intent.slots.moveToCode.resolutions.resolutionsPerAuthority[0].values && self.event.request.intent.slots.moveToCode.resolutions.resolutionsPerAuthority[0].values[0].value && self.event.request.intent.slots.moveToCode.resolutions.resolutionsPerAuthority[0].values[0].value.name){
	  		var codeTo = self.event.request.intent.slots.moveToCode.resolutions.resolutionsPerAuthority[0].values[0].value.name || self.event.request.intent.slots.moveToCode.value; // check if correct value with regex
	  	}
	  	else{
	    	var codeTo =  self.event.request.intent.slots.moveToCode.value; // check if correct value with regex
	  	}


	    var moveApproved = false;
	    var chess;
	  	if(!(/[a-hA-H][1-8]/gi.test(codeFrom))){
			const slotToElicit = 'moveFromCode';
            const speechOutput = 'Where would you like to move from?';
            const repromptSpeech = speechOutput;
            this.emit(':elicitSlot', slotToElicit, speechOutput, repromptSpeech);
	  	}
	  	else if(!(/[a-hA-H][1-8]/gi.test(codeTo))){
	  		const slotToElicit = 'moveToCode';
            const speechOutput = 'Where would you like to move to from '+codeFrom+' ?';
            const repromptSpeech = speechOutput;
            this.emit(':elicitSlot', slotToElicit, speechOutput, repromptSpeech);
	  	}
	  	else{
	  		chess = new Chess(this.attributes.gameData.fen);
	  		codeFrom = codeFrom.replace(/ /gi,"").toLowerCase();
	  		codeTo = codeTo.replace(/ /gi,"").toLowerCase();
	  		var moves = chess.moves({square:codeFrom});
	  		var moveValid = false;
	  		for (var i = 0; i < moves.length; i++) {
	  			if(moves[i].match(/[a-h][1-8]/gi) == codeTo){
					moveValid=true;
					break;
	  			}
	  		}
	  		if(moves.length == 0){
	  			var prompt = "No valid moves available from "+codeFrom+". Please make a different move."
				var reprompt = "please tell the new move"
				self.emit(':ask',prompt,reprompt);
	  		}
	  		else if(!moveValid){
	  			const slotToElicit = 'moveToCode';
	            var speechOutput = 'The move you made is invalid. Please choose a move from ';
	            for(var i=0;i<Math.min(5,moves.length);i++){
	            	speechOutput += moves[i].match(/[a-h][1-8]/gi)+ " ";
	            }
	            const repromptSpeech = speechOutput + "codeTo: " + codeTo;
	            this.emit(':elicitSlot', slotToElicit, speechOutput+"codeTo: " + codeTo + (moves[0].match(/[a-hA-H][1-8]/gi))  + (moves[1].match(/[a-hA-H][1-8]/gi)), repromptSpeech);
	  		}
	  		else{
	  			chess.move({ from: codeFrom, to: codeTo })
	  			moveApproved=true;
	  		}
	  	}

	  	if(moveApproved){
	  		var gameData = self.attributes.gameData;
	  		var gameOver = false;
	  		var winner;
	  		if(chess.game_over()){
	  			gameOver = true;
	  		}
	  		if(gameData.moves === undefined){
	  			gameData.moves = [];
	  		}
	  		gameData.moves.push({
			    "from" : codeFrom,
			    "to" : codeTo,
			    "updatedWithAlexa" : true
	  		});
	  		if(!gameOver){
	  			var turn = gameData.currentTurn == "black"?"white":"black";
	  			var updateJson = {
		  			"currentTurn":turn,
		  			"moves":gameData.moves,
		  			"fen":chess.fen()
		  		};
	  		}
	  		else{
	  			winner = gameData.currentTurn;
	  			var turn = gameData.currentTurn
	  			var updateJson = {
		  			"currentTurn":turn,
		  			"moves":gameData.moves,
		  			"fen":chess.fen(),
		  			"isCompleted" : gameOver,
		  			"winner": winner
		  		};
	  		}
	  		firebase.database().ref("gameColl/"+self.attributes.currentGameCode)
			.update(updateJson)
			.then(()=>{
				if(!gameOver){
					var prompt = "move made"
					if(gameData.isSameAlexa){
						var reprompt = "please tell the next move"
					}
					else{
						var reprompt = "please wait while the opponent completes his move"
					}
					self.emit(':ask',prompt,reprompt);
				}
				else{
					var prompt = winner + " won the game.";
					var reprompt = "please tell the code";
					self.emit(':tell',prompt);
				}
			});	
	  	}
	  	delete self.attributes.gameData;
	});  
  
  },
  "connectNewDevice": function (){
  		this.attributes.isSynced = false;
  		this.emit("LaunchRequest");
  }, 
  "gameEnd": function (){
  		var prompt = ""
		var reprompt = "please tell the code"
		self.emit(':ask',prompt,reprompt);
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
    var keys = Object.keys(this.attributes);
    for (var i = 0; i < keys.length; i++) {
    	if(keys[i] == "" || keys[i] == undefined )
    	{
    		delete this.attributes[keys[i]]
    	}
    }
    delete this.attributes.gameData;
    console.log(this.attributes)
    this.emit(':saveState', true);
  }
};

exports.handler = function(event, context, callback){
  var alexa = Alexa.handler(event, context);
  	
    alexa.registerHandlers(handlers);
    alexa.dynamoDBTableName = 'WizardChess';

    
    alexa.execute();
};